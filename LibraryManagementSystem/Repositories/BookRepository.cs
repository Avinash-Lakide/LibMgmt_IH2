using LibraryManagementSystem.Data;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(LibraryDbContext context) : base(context) { }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author)
        {
            return await _dbSet.Where(b => b.AuthorName.Contains(author)).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
        {
            return await _dbSet.Where(b => b.AvailableCopies > 0).AsNoTracking().ToListAsync();
        }

        public async Task<(IEnumerable<Book> books, int total)> GetBooksAsync(string? title, string? author, int page, int size)
        {
            var query = _dbSet.Where(b => !b.IsDeleted);
            if (!string.IsNullOrEmpty(title))
                query = query.Where(b => b.BookTitle.Contains(title));
            if (!string.IsNullOrEmpty(author))
                query = query.Where(b => b.AuthorName.Contains(author));

            var total = await query.CountAsync();
            var books = await query.Skip((page - 1) * size).Take(size).AsNoTracking().ToListAsync();
            return (books, total);
        }
    }
}