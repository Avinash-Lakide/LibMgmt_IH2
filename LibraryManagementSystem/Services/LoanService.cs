using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Services
{
    public class LoanService : Service<Loan>, ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IBookService _bookService;
        private readonly LibraryDbContext _context;

        public LoanService(ILoanRepository loanRepository, IBookService bookService, LibraryDbContext context) : base(loanRepository)
        {
            _loanRepository = loanRepository;
            _bookService = bookService;
            _context = context;
        }

        public async Task<IEnumerable<Loan>> GetLoansByMemberAsync(Guid memberId)
        {
            return await _loanRepository.GetLoansByMemberAsync(memberId);
        }

        public async Task<IEnumerable<Loan>> GetOverdueLoansAsync()
        {
            return await _loanRepository.GetOverdueLoansAsync();
        }

        public async Task<Loan> BorrowBookAsync(object bookId, Guid memberId)
        {
            if (!await CanBorrowAsync(bookId, memberId))
            {
                throw new InvalidOperationException("Cannot borrow this book.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var loan = new Loan
                {
                    BookId = (Guid)bookId,
                    MemberId = memberId,
                    BorrowedDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14) // Assuming 2 weeks loan period
                };

                // Decrease available copies
                var book = await _bookService.GetByIdAsync(bookId);
                if (book != null)
                {
                    book.AvailableCopies--;
                    await _bookService.UpdateAsync(book);
                }

                var createdLoan = await CreateAsync(loan);
                await transaction.CommitAsync();
                return createdLoan;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ReturnBookAsync(Guid loanId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var loan = await GetByIdAsync(loanId);
                if (loan == null || loan.ReturnedDate.HasValue)
                {
                    throw new InvalidOperationException("Loan not found or already returned.");
                }

                loan.ReturnedDate = DateTime.Now;

                // Increase available copies
                var book = await _bookService.GetByIdAsync(loan.BookId);
                if (book != null)
                {
                    book.AvailableCopies++;
                    await _bookService.UpdateAsync(book);
                }

                await UpdateAsync(loan);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CanBorrowAsync(object bookId, Guid memberId)
        {
            var activeLoan = await _loanRepository.GetActiveLoanAsync((Guid)bookId, memberId);
            if (activeLoan != null)
            {
                return false; // Already borrowed
            }

            return await _bookService.IsBookAvailableAsync(bookId);
        }

        public async Task<(IEnumerable<Loan> loans, int total)> GetLoansAsync(string? status, int page, int size)
        {
            return await _loanRepository.GetLoansAsync(status, page, size);
        }
    }
}