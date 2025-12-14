using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ILogger<LoansController> _log;

        public LoansController(ILoanService loanService, ILogger<LoansController> logger)
        {
            _loanService = loanService;
            _log = logger;
        }

        // List all loans, can filter by status and page
        [HttpGet]
        public async Task<IActionResult> GetLoans(string? status, int page = 1, int size = 10)
        {
            if (page <= 0)
                return BadRequest("Page must be greater than 0.");
            if (size <= 0 || size > 100)
                return BadRequest("Size must be between 1 and 100.");

            try
            {
                var (loans, total) = await _loanService.GetLoansAsync(status, page, size);
                _log.LogInformation($"Fetched {loans.Count()} loans for status {status}, page {page}");
                return Ok(new { loans, total, page, size });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to get loans");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Get one loan by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoan(Guid id)
        {
            try
            {
                var loan = await _loanService.GetByIdAsync(id);
                if (loan == null)
                    return NotFound("Loan not found");
                _log.LogInformation($"Retrieved loan {id}");
                return Ok(loan);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error getting loan {id}");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Borrow a book
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var loan = await _loanService.BorrowBookAsync(request.BookId, request.MemberId);
                _log.LogInformation($"Book {request.BookId} borrowed by {request.MemberId}, loan {loan.LoanId}");
                return CreatedAtAction(nameof(GetLoan), new { id = loan.LoanId }, loan);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to borrow book");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Return a book
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromBody] ReturnRequest request)
        {
            try
            {
                await _loanService.ReturnBookAsync(request.LoanId);
                _log.LogInformation($"Book returned for loan {request.LoanId}");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error returning book for loan {request.LoanId}");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Get overdue loans
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueLoans()
        {
            try
            {
                var loans = await _loanService.GetOverdueLoansAsync();
                _log.LogInformation($"Found {loans.Count()} overdue loans");
                return Ok(loans);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to get overdue loans");
                return StatusCode(500, "Something went wrong");
            }
        }

        // Delete a loan
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(Guid id)
        {
            try
            {
                var loan = await _loanService.GetByIdAsync(id);
                if (loan == null)
                    return NotFound("Loan not found");

                await _loanService.DeleteAsync(id);
                _log.LogInformation($"Deleted loan {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"Error deleting loan {id}");
                return StatusCode(500, "Something went wrong");
            }
        }
    }

    public class BorrowRequest
    {
        [Required(ErrorMessage = "Book ID is required.")]
        public Guid BookId { get; set; }

        [Required(ErrorMessage = "Member ID is required.")]
        public Guid MemberId { get; set; }
    }

    public class ReturnRequest
    {
        [Required(ErrorMessage = "Loan ID is required.")]
        public Guid LoanId { get; set; }
    }
}