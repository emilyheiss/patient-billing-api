# Patient Billing API

A simple ASP.NET Core Web API demonstrating .NET + SQL-style persistence (EF Core + SQLite) for a healthcare billing workflow.

## Tech Stack
- C# / ASP.NET Core Minimal API
- Entity Framework Core
- SQLite
- Swagger (OpenAPI)

## Features
- CRUD-style endpoints for Patients and Invoices
- Invoice lifecycle: mark an invoice as Paid
- Query filtering on invoices (status, patientId, amount range)
- Swagger UI for interactive testing

## Run Locally
```bash
dotnet restore
ASPNETCORE_ENVIRONMENT=Development dotnet run