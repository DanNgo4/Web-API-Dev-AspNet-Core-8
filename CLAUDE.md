# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Working rules

- **Ask mode**: when the user is asking a question (not requesting a change), respond with explanation and code snippets only — do **not** edit files. Only modify files when the user explicitly asks for the change to be made.

## What this repository is

A personal study companion for the book *Web API Development with ASP.NET Core 8* by Xiaodi Yan (Packt). It is **not a single application** — it is a collection of independent demo solutions, one set per book chapter (`C2`–`C11`). Each chapter folder contains one or more self-contained `.sln` solutions that illustrate a specific topic. There is no top-level solution tying them together.

`notes.txt` holds the author's personal scaffolding commands (creating projects/solutions, managing LocalDB). The book PDF is checked into the repo root.

## Working across chapters

Always operate at the level of an individual solution, not the repo root. Each `.sln` is independent with its own NuGet packages and target framework (`net8.0`). Build/test/run commands must be run against a specific `.sln` or `.csproj`.

```powershell
# Build a specific solution
dotnet build "C9\InvoiceApp\InvoiceApp.sln"

# Run a web API project (cd into the project, not the solution)
dotnet run --project "C9\InvoiceApp\InvoiceApp.WebApi"

# Run all tests in a test project / solution
dotnet test "C9\InvoiceApp\InvoiceApp.sln"

# Run a single test by fully-qualified name or filter
dotnet test "C9\InvoiceApp\InvoiceApp.sln" --filter "FullyQualifiedName~InvoiceControllerTests"
dotnet test "C9\InvoiceApp\InvoiceApp.sln" --filter "DisplayName~Should_Return_NotFound"
```

Chapter → topic map (use this to find the right solution for a topic):

- **C2** — first Web API, minimal APIs, dependency injection
- **C3** — configuration, environments, routing
- **C4** — logging, custom middleware
- **C5** — EF Core basics
- **C6** — EF Core relationships
- **C7** — EF Core (full), reverse engineering, concurrency conflicts
- **C8** — authentication
- **C9** — `InvoiceApp`: the fullest example — layered Web API + unit tests
- **C10** — testing: `IntegrationTestDemo` (the C9 InvoiceApp + integration tests) and `AuthTestDemo` (claims-based authorization + integration tests)
- **C11** — `GrpcDemo`: gRPC service with `.proto` contracts

## InvoiceApp architecture (C9 / C10)

`InvoiceApp` is the reference application and reappears across C9 and C10. It uses a conventional layered structure inside `InvoiceApp.WebApi`:

- **Controllers** → **Services** (`Interfaces/I*.cs` + `Services/*.cs`) → **Repositories** (`Interfaces/IInvoiceRepository.cs` + `Repositories/`) → **EF Core DbContext** (`Data/InvoiceDbContext.cs`).
- All services and repositories are registered with `AddScoped` in `Program.cs` and consumed via constructor injection through their interfaces.
- Entity mappings live in `Data/*Configuration.cs` and are applied via `ApplyConfigurationsFromAssembly` in `InvoiceDbContext.OnModelCreating` — add a new `IEntityTypeConfiguration<T>` class rather than configuring inline.
- The DbContext reads its connection string from configuration (`ConnectionStrings:DefaultConnection`) inside `OnConfiguring`, using SQL Server LocalDB.

### Database

The apps target **SQL Server LocalDB** (`(localdb)\mssqllocaldb`). EF Core migrations live in `Migrations/`. To manage the schema:

```powershell
dotnet ef migrations add <Name> --project "C9\InvoiceApp\InvoiceApp.WebApi"
dotnet ef database update --project "C9\InvoiceApp\InvoiceApp.WebApi"
```

To reset a stuck LocalDB instance (from `notes.txt`): `sqllocaldb stop` then `sqllocaldb delete`.

## Testing approach

Test projects use **xUnit + FluentAssertions + Moq**, with `coverlet.collector` for coverage.

The tests run against **real LocalDB databases**, not in-memory providers — fixtures spin up dedicated test databases (e.g. `InvoiceTransactionalTestDb`) via `EnsureDeleted`/`EnsureCreated` and seed known data. This means **LocalDB must be available to run the test suite**. Key patterns:

- `Fixtures/TransactionalTestDatabaseFixture.cs` — seeds the DB and wraps each test in a transaction that is rolled back, keeping tests isolated.
- Collection fixtures (e.g. `TransactionTestsCollection.cs`) share one database setup across a test class group.
- Integration tests (C10) use `CustomIntegrationTestsFixture` built on `WebApplicationFactory` to exercise the API end-to-end over HTTP.

## gRPC (C11)

`GrpcDemo` defines message/service contracts in `Protos/*.proto`; C# types are generated into `Generated/Protos/`. **Edit the `.proto` file, not the generated `.cs`** — the generated code regenerates on build. Note that proto `string` fields (e.g. `invoice_id`) generate `string` properties, so a `Guid` must be converted with `.ToString()` before assignment.
