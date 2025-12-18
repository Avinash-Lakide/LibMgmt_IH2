using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Middleware;
using LibraryManagementSystem.Components;
using LibraryManagementSystem.Models;
using System.Collections.Generic;
using Serilog;

// Log.Logger = new LoggerConfiguration()
//     .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
//     .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

// Register services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ILoanService, LoanService>();

// Register components
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<CachingService>();

// Add caching
builder.Services.AddMemoryCache();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.EnsureCreated();

    // Seed data
    if (!db.LibBooks.Any())
    {
        var books = new List<Book>
        {
            new Book { BookTitle = "The Great Gatsby", AuthorName = "F. Scott Fitzgerald", BookIsbn = "9780743273565", TotalCopies = 5, AvailableCopies = 5, AddedOn = DateTime.Now },
            new Book { BookTitle = "To Kill a Mockingbird", AuthorName = "Harper Lee", BookIsbn = "9780061120084", TotalCopies = 3, AvailableCopies = 3, AddedOn = DateTime.Now },
            new Book { BookTitle = "1984", AuthorName = "George Orwell", BookIsbn = "9780451524935", TotalCopies = 4, AvailableCopies = 4, AddedOn = DateTime.Now },
            new Book { BookTitle = "Pride and Prejudice", AuthorName = "Jane Austen", BookIsbn = "9780486284736", TotalCopies = 2, AvailableCopies = 2, AddedOn = DateTime.Now },
            new Book { BookTitle = "The Catcher in the Rye", AuthorName = "J.D. Salinger", BookIsbn = "9780316769488", TotalCopies = 6, AvailableCopies = 6, AddedOn = DateTime.Now }
        };
        db.LibBooks.AddRange(books);
        db.SaveChanges();
    }

    if (!db.LibMembers.Any())
    {
        var members = new List<Member>
        {
            new Member { FullName = "John Doe", EmailId = "john@example.com", JoinedDate = DateTime.Now },
            new Member { FullName = "Jane Smith", EmailId = "jane@example.com", JoinedDate = DateTime.Now },
            new Member { FullName = "Bob Johnson", EmailId = "bob@example.com", JoinedDate = DateTime.Now },
            new Member { FullName = "Alice Brown", EmailId = "alice@example.com", JoinedDate = DateTime.Now },
            new Member { FullName = "Charlie Wilson", EmailId = "charlie@example.com", JoinedDate = DateTime.Now }
        };
        db.LibMembers.AddRange(members);
        db.SaveChanges();
    }

    if (!db.LibLoans.Any())
    {
        var loans = new List<Loan>
        {
            new Loan { BookId = db.LibBooks.First(b => b.BookTitle == "The Great Gatsby").BookId, MemberId = db.LibMembers.First(m => m.FullName == "John Doe").MemberId, BorrowedDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14) },
            new Loan { BookId = db.LibBooks.First(b => b.BookTitle == "To Kill a Mockingbird").BookId, MemberId = db.LibMembers.First(m => m.FullName == "Jane Smith").MemberId, BorrowedDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14) },
            new Loan { BookId = db.LibBooks.First(b => b.BookTitle == "1984").BookId, MemberId = db.LibMembers.First(m => m.FullName == "Bob Johnson").MemberId, BorrowedDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14) },
            new Loan { BookId = db.LibBooks.First(b => b.BookTitle == "Pride and Prejudice").BookId, MemberId = db.LibMembers.First(m => m.FullName == "Alice Brown").MemberId, BorrowedDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14) },
            new Loan { BookId = db.LibBooks.First(b => b.BookTitle == "The Catcher in the Rye").BookId, MemberId = db.LibMembers.First(m => m.FullName == "Charlie Wilson").MemberId, BorrowedDate = DateTime.Now, DueDate = DateTime.Now.AddDays(14) }
        };
        db.LibLoans.AddRange(loans);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.MapControllers();

app.Run();
