using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Components;

namespace LibraryManagementSystem.Services
{
    public class BookService : Service<Book>, IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly CachingService _cache;
        private readonly ValidationService _validation;

        public BookService(IBookRepository bookRepository, CachingService cache, ValidationService validation) : base(bookRepository)
        {
            _bookRepository = bookRepository;
            _cache = cache;
            _validation = validation;
        }

        public async Task<Book> CreateAsync(Book entity)
        {
            if (!_validation.IsValid(entity))
            {
                throw new ArgumentException("Invalid book data.");
            }
            return await base.CreateAsync(entity);
        }

        public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
        {
            return await _cache.GetOrSetAsync("available_books", async () => await _bookRepository.GetAvailableBooksAsync(), TimeSpan.FromMinutes(5));
        }

        public async Task UpdateAsync(Book entity)
        {
            if (!_validation.IsValid(entity))
            {
                throw new ArgumentException("Invalid book data.");
            }
            await base.UpdateAsync(entity);
            _cache.Remove("available_books"); // Invalidate cache
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author)
        {
            return await _bookRepository.GetBooksByAuthorAsync(author);
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