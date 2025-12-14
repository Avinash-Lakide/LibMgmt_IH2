using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class BookService : Service<Book>, IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository) : base(bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author)
        {
            return await _bookRepository.GetBooksByAuthorAsync(author);
        }

        public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
        {
            return await _bookRepository.GetAvailableBooksAsync();
        }

        public async Task<bool> IsBookAvailableAsync(object bookId)
        {
            var book = await _repository.GetByIdAsync(bookId);
            return book != null && book.AvailableCopies > 0;
        }

        public async Task<(IEnumerable<Book> books, int total)> GetBooksAsync(string? title, string? author, int page, int size)
        {
            return await _bookRepository.GetBooksAsync(title, author, page, size);
        }
    }
}