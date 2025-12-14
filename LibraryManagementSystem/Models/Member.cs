using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LibraryManagementSystem.Models
{
    public class Member
    {
        public Guid MemberId { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(180, ErrorMessage = "Full name cannot exceed 180 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string EmailId { get; set; } = string.Empty;

        public DateTime JoinedDate { get; set; }

        // Navigation
        [JsonIgnore]
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}