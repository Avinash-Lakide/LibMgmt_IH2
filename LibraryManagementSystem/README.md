# Library Management System

This is a Web API I built for managing library operations like books, members, and loans.

## What I Used

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

## Getting Started

First, make sure you have SQL Server running and create a database called `LibMgmt` with the schema from the project.

Then, check the connection string in `appsettings.json` and update it if needed.

To run: `dotnet run`

## API Endpoints

### Books

- GET /api/books - List all books
- GET /api/books/{id} - Get a specific book
- POST /api/books - Add a new book
- PUT /api/books/{id} - Update book details
- DELETE /api/books/{id} - Remove a book

### Members

- GET /api/members - List all members
- GET /api/members/{id} - Get a specific member
- POST /api/members - Add a new member
- PUT /api/members/{id} - Update member info
- DELETE /api/members/{id} - Remove a member

### Loans

- GET /api/loans - List all loans
- GET /api/loans/{id} - Get a specific loan
- POST /api/loans/borrow - Borrow a book (send bookId and memberId in body)
- PUT /api/loans/return/{id} - Return a book
- DELETE /api/loans/{id} - Delete a loan

## Key Features

- Error handling with proper logging
- Returns appropriate HTTP codes
- Follows OOP concepts
- Uses EF Core for DB stuff

## Testing Flow

Follow this step-by-step to test all APIs. Use tools like Postman or curl. Replace `{id}` placeholders with actual Guids from responses. the API runs on `http://localhost:5264`.

## Here's a summary of the recent enhancements on 16th Dec:

**Middleware Added**
- LoggingMiddleware: Logs incoming requests (method, path, IP) and outgoing responses (status code, processing time).
- ExceptionHandlingMiddleware: Catches unhandled exceptions, logs them, and returns a standardized error response.

Note: Both middlewares are registered in Program.cs and will handle requests globally.

**Reusable Components**
- ValidationService: Provides model validation using DataAnnotations. Integrated into BookService for create/update operations.
- CachingService: Implements in-memory caching with expiration. Used in BookService to cache available books for 5 minutes, reducing database queries.

**Performance Optimizations**
- EF Core Improvements: Added AsNoTracking() to read-only queries in repositories to avoid unnecessary change tracking.
- Transactions: Wrapped BorrowBookAsync and ReturnBookAsync in database transactions for data consistency and to reduce round-trips.
- Caching: Cached frequently accessed data (available books) to improve response times.
- Validation: Added input validation to prevent invalid data from being processed.

**Code Quality Improvements**
- Used dependency injection for all new components.
- Maintained async/await patterns throughout.
- Added proper error handling with transactions.


### 1. Create Sample Data

**Add a Book**
```
POST http://localhost:5264/api/books
Content-Type: application/json

{
  "bookIsbn": "978-3-16-148410-0",
  "bookTitle": "Sample Book",
  "bookSubTitle": "A Subtitle",
  "authorName": "John Author",
  "totalCopies": 5,
  "availableCopies": 5
}
```
- Note the `bookId` from the response (e.g., `123e4567-e89b-12d3-a456-426614174000`).

**Add Another Book**
```
POST http://localhost:5264/api/books
Content-Type: application/json

{
  "bookIsbn": "978-1-23-456789-0",
  "bookTitle": "Another Book",
  "authorName": "Jane Writer",
  "totalCopies": 3,
  "availableCopies": 3
}
```
- Note the `bookId`.

**Add a Member**
```
POST http://localhost:5264/api/members
Content-Type: application/json

{
  "fullName": "Alice Johnson",
  "emailId": "alice@example.com"
}
```
- Note the `memberId`.

**Add Another Member**
```
POST http://localhost:5264/api/members
Content-Type: application/json

{
  "fullName": "Bob Smith",
  "emailId": "bob@example.com"
}
```
- Note the `memberId`.

### 2. Test Books CRUD

**List Books**
```
GET http://localhost:5264/api/books?page=1&size=10
```

**Get a Book**
```
GET http://localhost:5264/api/books/{bookId}
```

**Update a Book**
```
PUT http://localhost:5264/api/books/{bookId}
Content-Type: application/json

{
  "bookId": "{bookId}",
  "bookIsbn": "978-3-16-148410-0",
  "bookTitle": "Updated Sample Book",
  "authorName": "John Author",
  "totalCopies": 5,
  "availableCopies": 4
}
```

**Delete a Book**
```
DELETE http://localhost:5264/api/books/{bookId}
```

### 3. Test Members CRUD

**List Members**
```
GET http://localhost:5264/api/members?page=1&size=10
```

**Get a Member**
```
GET http://localhost:5264/api/members/{memberId}
```

**Update a Member**
```
PUT http://localhost:5264/api/members/{memberId}
Content-Type: application/json

{
  "memberId": "{memberId}",
  "fullName": "Updated Alice",
  "emailId": "alice.updated@example.com"
}
```

**Delete a Member**
```
DELETE http://localhost:5264/api/members/{memberId}
```

### 4. Test Loans

**Borrow a Book**
```
POST http://localhost:5264/api/loans/borrow
Content-Type: application/json

{
  "bookId": "{bookId}",
  "memberId": "{memberId}"
}
```
- Note the `loanId`.

**List Loans**
```
GET http://localhost:5264/api/loans?page=1&size=10
```

**Get a Loan**
```
GET http://localhost:5264/api/loans/{loanId}
```

**List Overdue Loans**
```
GET http://localhost:5264/api/loans/overdue
```

**Return a Book**
```
POST http://localhost:5264/api/loans/return
Content-Type: application/json

{
  "loanId": "{loanId}"
}
```

**Delete a Loan**
```
DELETE http://localhost:5264/api/loans/{loanId}
```

## Database Script

Run this in SQL Server to set up the DB:

```sql
Create Database LibMgmt
Go
Use LibMgmt
Go

-- Books table, added soft delete for easy recovery
CREATE TABLE LibBooks (
    BookId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    BookIsbn NVARCHAR(30) NOT NULL,
    BookTitle NVARCHAR(250) NOT NULL,
    BookSubTitle NVARCHAR(250) NULL,
    AuthorName NVARCHAR(200) NOT NULL,
    TotalCopies INT NOT NULL,
    AvailableCopies INT NOT NULL,
    AddedOn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    VersionStamp ROWVERSION,
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

-- Unique ISBN to avoid duplicates
ALTER TABLE LibBooks ADD CONSTRAINT UQ_LibBooks_Isbn UNIQUE(BookIsbn);
GO
-- Make sure available doesn't go over total
ALTER TABLE LibBooks ADD CONSTRAINT CHK_Available_LE_Total CHECK (AvailableCopies <= TotalCopies);
GO

-- Members table
CREATE TABLE LibMembers (
    MemberId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    FullName NVARCHAR(180) NOT NULL,
    EmailId NVARCHAR(200) NOT NULL,
    JoinedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Emails should be unique
ALTER TABLE LibMembers ADD CONSTRAINT UQ_LibMembers_Email UNIQUE(EmailId);
GO
-- Names should be unique too
ALTER TABLE LibMembers ADD CONSTRAINT UQ_LibMembers_FullName UNIQUE(FullName);
GO

-- Loans table for tracking borrows
CREATE TABLE LibLoans (
    LoanId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    BookId UNIQUEIDENTIFIER NOT NULL,
    MemberId UNIQUEIDENTIFIER NOT NULL,
    BorrowedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    DueDate DATETIME2 NOT NULL,
    ReturnedDate DATETIME2 NULL
);
GO

-- Link to books
ALTER TABLE LibLoans
ADD CONSTRAINT FK_LibLoans_Book FOREIGN KEY (BookId) REFERENCES LibBooks (BookId);

-- Link to members
ALTER TABLE LibLoans
ADD CONSTRAINT FK_LibLoans_Member FOREIGN KEY (MemberId) REFERENCES LibMembers (MemberId);
GO

-- Due date should be after borrow
ALTER TABLE LibLoans ADD CONSTRAINT CHK_DueDate_After_Borrowed CHECK (DueDate >= BorrowedDate);
GO
-- If returned, it should be after borrow
ALTER TABLE LibLoans ADD CONSTRAINT CHK_ReturnedDate_After_Borrowed CHECK (ReturnedDate IS NULL OR ReturnedDate >= BorrowedDate);
GO

-- Indexes for faster joins
CREATE INDEX IX_LibLoans_BookId ON LibLoans (BookId);
GO
CREATE INDEX IX_LibLoans_MemberId ON LibLoans (MemberId);
GO
```