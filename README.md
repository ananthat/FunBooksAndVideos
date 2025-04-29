# FunBooksAndVideos E-commerce API (ASP.NET Core)

This project implements a simplified e-commerce backend system for FunBooksAndVideos using C# and ASP.NET Core, as described in the technical task. It includes an object-oriented model, a flexible purchase order processor, and REST APIs.

## Features

- **Object-Oriented Model:** Represents Customers, Products (Books, Videos), Memberships, Purchase Orders, and Shipping Slips in C#.
- **Purchase Order Processing:** Handles purchase orders containing products and/or memberships.
- **Business Rules Implementation:**
    - Activates memberships immediately upon purchase (BR1).
    - Generates shipping slips for physical products (BR2).
- **REST API:** Exposes functionality through ASP.NET Core Web API endpoints.
- **Flexible Design:** Uses the Observer pattern (via dependency injection of `IPurchaseOrderObserver`) in the Purchase Order Processor to allow easy addition of new business rules.
- **Testing:** Includes unit tests using xUnit and Moq.
- **Layered Architecture:** Follows clean architecture principles with Core, Application, Infrastructure, and API layers.

## Project Structure

```
FunBooksAndVideos.CSharp/
├── src/
│   ├── FunBooksAndVideos.Api/          # ASP.NET Core Web API project
│   │   ├── Controllers/
│   │   ├── Models/                 # DTOs
│   │   └── Program.cs              # Application entry point & DI setup
│   ├── FunBooksAndVideos.Application/  # Application logic, interfaces, observers
│   │   ├── Contracts/
│   │   ├── Features/
│   │   └── ...
│   ├── FunBooksAndVideos.Core/       # Domain models
│   │   └── Models/
│   └── FunBooksAndVideos.Infrastructure/ # Data access, external services (in-memory)
│       ├── Persistence/
│       └── InfrastructureServiceRegistration.cs
├── tests/
│   └── FunBooksAndVideos.Tests/      # xUnit test project
│       ├── Api/
│       └── Application/
├── FunBooksAndVideos.sln             # Solution file
└── README.md                         # This file
```

## Setup and Installation

1.  **Ensure .NET 8 SDK is installed.** (Installation steps were performed earlier in the sandbox environment).
2.  **Clone the repository (or ensure files are in the correct structure).**
3.  **Navigate to the solution directory:**
    ```bash
    cd /home/ubuntu/FunBooksAndVideos.CSharp
    ```
4.  **Restore dependencies:**
    ```bash
    dotnet restore
    ```

## Running the Application

1.  **Navigate to the API project directory:**
    ```bash
    cd /Working-Repo/FunBooksAndVideos.CSharp/src/FunBooksAndVideos.Api
    ```
2.  **Run the application:**
    ```bash
    dotnet run
    ```
    *(Note: The application is configured to run on HTTPS. You might need to trust the development certificate or configure HTTP if needed. For sandbox access, HTTP might be easier)*

3.  **Access the API:**
    - The application typically runs on `https://localhost:7xxx` and `http://localhost:5xxx`. Check the console output for the exact ports.
    - **Swagger UI (Docs):** `https://localhost:7xxx/swagger` or `http://localhost:5xxx/swagger`

## Running Tests

1.  **Navigate to the solution directory:**
    ```bash
    cd /Working-Repo/FunBooksAndVideos.CSharp
    ```
2.  **Run tests:**
    ```bash
    dotnet test
    ```

## API Endpoints

- `POST /api/v1/PurchaseOrders`: Create and process a new purchase order.
- `GET /api/v1/PurchaseOrders/{id}`: Retrieve details of a specific purchase order.
- `GET /api/v1/Customers/{id}`: Retrieve customer details (including active memberships).
- `GET /api/v1/ShippingSlips/byPurchaseOrder/{purchaseOrderId}`: Retrieve the shipping slip for a purchase order (if applicable).

Refer to the Swagger UI (`/swagger`) for detailed request/response models and to try out the API.

## Design Notes

- **In-Memory Storage:** The application uses simple static `ConcurrentDictionary` instances within the Infrastructure layer for data storage. In a real-world application, this would be replaced with a proper database (e.g., Maria DB, Mongo DB, SQL Server) and an ORM (e.g., Entity Framework Core).
- **Observer Pattern:** The `PurchaseOrderProcessor` receives an `IEnumerable<IPurchaseOrderObserver>` via dependency injection. `MembershipActivator` and `ShippingSlipGenerator` are registered implementations of this interface. This makes it easy to add new rules by creating new observer classes and registering them in `InfrastructureServiceRegistration.cs`.
- **Dependency Injection:** ASP.NET Core's built-in dependency injection is used to manage dependencies between layers.
- **Error Handling:** Basic error handling (e.g., 404 Not Found, 400 Bad Request) is implemented in the API controllers.

