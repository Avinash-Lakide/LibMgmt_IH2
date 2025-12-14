using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces
{
    public interface ILoanRepository : IRepository<Loan>
    {
        Task<IEnumerable<Loan>> GetLoansByMemberAsync(Guid memberId);
        Task<IEnumerable<Loan>> GetOverdueLoansAsync();
        Task<Loan?> GetActiveLoanAsync(Guid bookId, Guid memberId);
        Task<(IEnumerable<Loan> loans, int total)> GetLoansAsync(string? status, int page, int size);
    }
}