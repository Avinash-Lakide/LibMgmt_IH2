using LibraryManagementSystem.Data;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories
{
    public class LoanRepository : Repository<Loan>, ILoanRepository
    {
        public LoanRepository(LibraryDbContext context) : base(context) { }

        public async Task<IEnumerable<Loan>> GetLoansByMemberAsync(Guid memberId)
        {
            return await _dbSet.Include(l => l.Book).Include(l => l.Member)
                               .Where(l => l.MemberId == memberId).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetOverdueLoansAsync()
        {
            return await _dbSet.Include(l => l.Book).Include(l => l.Member)
                               .Where(l => l.DueDate < DateTime.Now && l.ReturnedDate == null).AsNoTracking().ToListAsync();
        }

        public async Task<Loan?> GetActiveLoanAsync(Guid bookId, Guid memberId)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(l => l.BookId == bookId && l.MemberId == memberId && l.ReturnedDate == null);
        }

        public async Task<(IEnumerable<Loan> loans, int total)> GetLoansAsync(string? status, int page, int size)
        {
            var query = _dbSet.Include(l => l.Book).Include(l => l.Member).AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active")
                    query = query.Where(l => l.ReturnedDate == null);
                else if (status == "history")
                    query = query.Where(l => l.ReturnedDate != null);
                else if (status == "overdue")
                    query = query.Where(l => l.ReturnedDate == null && l.DueDate < DateTime.UtcNow);
            }
            var total = await query.CountAsync();
            var loans = await query.Skip((page - 1) * size).Take(size).AsNoTracking().ToListAsync();
            return (loans, total);
        }
    }
}