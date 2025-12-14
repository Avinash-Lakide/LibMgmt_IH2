using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces
{
    public interface ILoanService : IService<Loan>
    {
        Task<IEnumerable<Loan>> GetLoansByMemberAsync(Guid memberId);
        Task<IEnumerable<Loan>> GetOverdueLoansAsync();
        Task<Loan> BorrowBookAsync(object bookId, Guid memberId);
        Task ReturnBookAsync(Guid loanId);
        Task<bool> CanBorrowAsync(object bookId, Guid memberId);
        Task<(IEnumerable<Loan> loans, int total)> GetLoansAsync(string? status, int page, int size);
    }
}