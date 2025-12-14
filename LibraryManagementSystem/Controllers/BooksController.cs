using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _log;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _log = logger;
        }

        // Fetch all books, can filter and page
        [HttpGet]
        public async Task<IActionResult> GetBooks(string? title, string? author, int page = 1, int size = 10)
        {
            if (page <= 0)
                return BadRequest("Page must be greater than 0.");
            if (size <= 0 || size > 100)
                return BadRequest("Size must be between 1 and 100.");

            try
            {
                var (books, total) = await _bookService.GetBooksAsync(title, author, page, size);
                _log.LogInformation($"Fetched {books.Count()} books for page {page}");
                return Ok(new { books, total, page, size });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to get books");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Get one book by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(Guid id)
        {
            try
            {
                var book = await _bookService.GetByIdAsync(id);
                if (book == null)
                    return NotFound("Book not found");
                _log.LogInformation($"Retrieved book {id}");
                return Ok(book);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error getting book {id}");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Create a new book
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            _log.LogInformation("Creating a new book");
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (book.AvailableCopies > book.TotalCopies)
                    return BadRequest("Available copies can't be more than total copies");

                book.BookId = Guid.NewGuid();
                book.AddedOn = DateTime.UtcNow;
                book.IsDeleted = false;
                var createdBook = await _bookService.CreateAsync(book);
                _log.LogInformation($"Created book with ID {createdBook.BookId}");
                return CreatedAtAction(nameof(GetBook), new { id = createdBook.BookId }, createdBook);
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    _log.LogWarning("Tried to add duplicate ISBN");
                    return BadRequest("ISBN already exists");
                }
                _log.LogError(dbEx, "Database error creating book");
                return StatusCode(500, "Something went wrong");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error creating book");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Edit a book
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(Guid id, [FromBody] Book book)
        {
            if (id != book.BookId)
                return BadRequest("ID mismatch");
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (book.AvailableCopies > book.TotalCopies)
                    return BadRequest("Available copies can't exceed total");

                var existingBook = await _bookService.GetByIdAsync(id);
                if (existingBook == null || existingBook.IsDeleted)
                    return NotFound("Book not found");

                existingBook.BookIsbn = book.BookIsbn;
                existingBook.BookTitle = book.BookTitle;
                existingBook.BookSubTitle = book.BookSubTitle;
                existingBook.AuthorName = book.AuthorName;
                existingBook.TotalCopies = book.TotalCopies;
                existingBook.AvailableCopies = book.AvailableCopies;

                await _bookService.UpdateAsync(existingBook);
                _log.LogInformation($"Updated book {id}");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bookService.GetByIdAsync(id) == null)
                    return NotFound("Book not found");
                else
                    return Conflict("Book was changed by someone else");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error updating book {id}");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Soft delete a book
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            try
            {
                var book = await _bookService.GetByIdAsync(id);
                if (book == null || book.IsDeleted)
                    return NotFound("Book not found");
                book.IsDeleted = true;
                await _bookService.UpdateAsync(book);
                _log.LogInformation($"Soft deleted book {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error deleting book {id}");
                return StatusCode(500, "Something went wrong");
            }
        }

        private async Task<bool> BookExists(Guid id) => await _bookService.GetByIdAsync(id) != null;
    }
}