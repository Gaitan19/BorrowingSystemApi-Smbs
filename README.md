# 📚 Borrowing System API

![.NET](https://img.shields.io/badge/.NET-7.0-%23512BD4?logo=.net)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-%23CC2927?logo=microsoft-sql-server)
![JWT](https://img.shields.io/badge/JWT-Auth-%23000000?logo=json-web-tokens)

A robust REST API for managing borrowing systems. Track items, users, loans, and returns with full audit capabilities.

## 🌟 Key Features
- 📖 **Item Management** (Books/Resources)
- 👥 **User Authentication** with JWT
- 🔄 **Loan/Renewal/Return Workflows**
- 📆 **Due Date Calculations & Reminders**
- 📊 **Reporting System** (Overdue items, User history)
- 🛡️ **Role-Based Access Control** (Admin/User)

## 🛠️ Tech Stack
| **Layer**       | **Technologies**                                                                 | Icons |
|------------------|----------------------------------------------------------------------------------|-------|
| **Core**         | .NET 7, C# 11, FluentValidation                                                 | ⚙️   |
| **Data**         | Entity Framework Core 7, SQL Server, Dapper                                      | 🗃️   |
| **API**          | REST Standards, Swagger/OpenAPI, JWT Authentication                             | 🌐   |
| **Utilities**    | AutoMapper, XUnit, Moq, Serilog                                                 | 🔧   |

## 🚀 Getting Started

### Prerequisites
- .NET 7 SDK
- SQL Server 2019+
- Redis (for caching - optional)

### Installation
```bash
# Clone repository
git clone https://github.com/Gaitan19/BorrowingSystemAPI.git

# Restore dependencies
dotnet restore

# Configure appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=BorrowingSystem;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "Secret": "your-secure-key-here",
    "ExpiryDays": 7
  }
}

# Run migrations
dotnet ef database update

# Start application
dotnet run
```
# BorrowingSystemApi-Smbs
