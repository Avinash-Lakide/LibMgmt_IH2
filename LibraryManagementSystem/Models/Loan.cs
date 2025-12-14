using System;

namespace LibraryManagementSystem.Models
{
    public class Loan
    {
        public Guid LoanId { get; set; }
        public Guid BookId { get; set; }
        public Guid MemberId { get; set; }
        public DateTime BorrowedDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnedDate { get; set; }

        // Refs to book and member
        public Book? Book { get; set; }
        public Member? Member { get; set; }
    }
}