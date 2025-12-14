using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces
{
    public interface IBookService : IService<Book>
    {
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author);
        Task<IEnumerable<Book>> GetAvailableBooksAsync();
        Task<bool> IsBookAvailableAsync(object bookId);
        Task<(IEnumerable<Book> books, int total)> GetBooksAsync(string? title, string? author, int page, int size);
    }
}