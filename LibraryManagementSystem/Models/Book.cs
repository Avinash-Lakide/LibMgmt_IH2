using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public Guid BookId { get; set; }

        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(30, ErrorMessage = "ISBN cannot exceed 30 characters.")]
        public string BookIsbn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Book title is required.")]
        [StringLength(250, ErrorMessage = "Book title cannot exceed 250 characters.")]
        public string BookTitle { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Book subtitle cannot exceed 250 characters.")]
        public string? BookSubTitle { get; set; }

        [Required(ErrorMessage = "Author name is required.")]
        [StringLength(200, ErrorMessage = "Author name cannot exceed 200 characters.")]
        public string AuthorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total copies is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Total copies must be a non-negative integer.")]
        public int TotalCopies { get; set; }

        [Required(ErrorMessage = "Available copies is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Available copies must be a non-negative integer.")]
        public int AvailableCopies { get; set; }

        public DateTime AddedOn { get; set; }
        public byte[] VersionStamp { get; set; } = Array.Empty<byte>();
        public bool IsDeleted { get; set; } = false;

        // Links to loans
        [JsonIgnore]
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}