using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Book> LibBooks { get; set; } = null!;
        public DbSet<Member> LibMembers { get; set; } = null!;
        public DbSet<Loan> LibLoans { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasKey(b => b.BookId);
            modelBuilder.Entity<Book>().HasIndex(b => b.BookIsbn).IsUnique();
            modelBuilder.Entity<Book>().Property(b => b.VersionStamp).IsRowVersion();

            modelBuilder.Entity<Member>().HasKey(m => m.MemberId);

            modelBuilder.Entity<Loan>().HasKey(l => l.LoanId);
            modelBuilder.Entity<Loan>().HasOne(l => l.Book).WithMany(b => b.Loans).HasForeignKey(l => l.BookId);
            modelBuilder.Entity<Loan>().HasOne(l => l.Member).WithMany(m => m.Loans).HasForeignKey(l => l.MemberId);
        }
    }
}