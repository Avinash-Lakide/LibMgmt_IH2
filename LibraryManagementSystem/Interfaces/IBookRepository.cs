using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author);
        Task<IEnumerable<Book>> GetAvailableBooksAsync();
        Task<(IEnumerable<Book> books, int total)> GetBooksAsync(string? title, string? author, int page, int size);
    }
}