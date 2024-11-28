# DotNetAPI-Advanced

DotNetAPI Advanced is an enhanced and feature-rich version of the original DotNetAPI project. Built with .NET 8 Core and C#, this RESTful API provides advanced functionality while maintaining robust integration with a Microsoft Azure SQL Database for scalable data management. It incorporates new constructors, additional classes, and extended functionalities to improve usability, efficiency, and scalability. As before, it leverages Swagger for API documentation and testing, ensuring a seamless development and integration experience.
New Features in DotNetAPI Advanced

    New Constructors: Improved flexibility and code reuse with newly designed constructors.
    Additional Classes: Expanded class structure for better modularity and maintainability.
    Extended CRUD Operations: More sophisticated Create, Read, Update, and Delete functionalities with advanced validation and filtering options.
    Batch Processing: Support for bulk data operations (insert/update/delete).
    Custom Middleware: Enhanced request handling, logging, and security with custom middleware.
    Role-Based Access Control (RBAC): Built-in user role and permission management.
    Improved Error Handling: Detailed error responses with additional context and traceability.
    Caching: Enhanced performance with integrated caching mechanisms (e.g., in-memory caching).

Key Features (Inherited and Enhanced)

    CRUD Operations: Perform Create, Read, Update, and Delete operations on the database.
    Azure SQL Database Integration: Uses Azure SQL for scalable and secure data management.
    Swagger UI: Interactive API documentation for testing and exploration.
    Entity Framework Core: Simplified database interactions through an ORM.
    Error Handling: Structured and user-friendly error responses.
    Scalability: Designed to scale with your application's needs in cloud environments.

Technologies Used

    Backend: .NET Core, C#
    Database: Azure SQL Database
    Documentation: Swagger (OpenAPI)
    ORM: Entity Framework Core
    Middleware: Custom middleware for enhanced processing
    Caching: Built-in caching (e.g., In-Memory Caching)

Getting Started

To get started with DotNetAPI Advanced, follow the steps below.
Prerequisites

    .NET 8 Core SDK
    Azure SQL Database setup with connection string
    Visual Studio or VS Code (optional)
    PowerShell (or Terminal with PowerShell support)
    Postman or a browser for testing via Swagger UI

Installation

    Clone this repository:

git clone https://github.com/yourusername/DotNetAPIAdvanced.git
cd DotNetAPIAdvanced

Configure the connection string in appsettings.json:

"ConnectionStrings": {
    "DefaultConnection": "Server=localhost; Database=your-database-name; Trusted_Connection=false; TrustServerCertificate=true; User Id=your-username; Password=your-password;"
}

Run database migrations to initialize the database schema:

dotnet ef database update

Start the application with live reload:

    dotnet watch run

Adding Dependencies

To add required packages for additional functionalities, use the .csproj file or the following command. For example, to add AutoMapper:

dotnet add package AutoMapper

All necessary packages are listed in the <ItemGroup> section of the DotNetAPIAdvanced.csproj file.
Usage

    Access Swagger UI at:

http://localhost:5000/swagger

Test the new and existing endpoints directly through the Swagger interface or use a tool like Postman.
