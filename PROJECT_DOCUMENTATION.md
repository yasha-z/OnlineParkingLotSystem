# Online Parking Lot System — Project Documentation

**Assignment:** Online Parking Lot System (24-06-2026)  
**Technology:** ASP.NET Core 8 Web API + Entity Framework Core + MySQL (Pomelo)  
**Project path:** `C:\Users\Yasha Ali\Projects\OnlineParkingLotSystem`

---

## Table of Contents

1. [What This Project Does](#1-what-this-project-does)
2. [High-Level Architecture](#2-high-level-architecture)
3. [Project Folder Structure](#3-project-folder-structure)
4. [Domain Entities and Relationships](#4-domain-entities-and-relationships)
5. [OOP Concepts (Assignment Requirements)](#5-oop-concepts-assignment-requirements)
6. [Business Logic Flow](#6-business-logic-flow)
7. [API Endpoints](#7-api-endpoints)
8. [Authentication (JWT)](#8-authentication-jwt)
9. [DTOs, Validation, and Mapping](#9-dtos-validation-and-mapping)
10. [Services and Dependency Injection](#10-services-and-dependency-injection)
11. [Database (EF Core + MySQL)](#11-database-ef-core--mysql)
12. [Middleware and Error Handling](#12-middleware-and-error-handling)
13. [Session Requirements Checklist](#13-session-requirements-checklist)
14. [How to Run and Test](#14-how-to-run-and-test)
15. [What to Say During Evaluation](#15-what-to-say-during-evaluation)

---

## 1. What This Project Does

This is a **backend API** for a multi-floor parking facility. It supports four main operations:


| #   | Scenario            | What happens                                                                                                                      |
| --- | ------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **Vehicle arrives** | System finds a free spot that fits the vehicle type, issues a parking ticket, marks the spot as occupied, and records entry time. |
| 2   | **Vehicle leaves**  | System calculates the parking fee, closes the ticket, and frees the spot.                                                         |
| 3   | **Manager view**    | Anyone can see all currently parked vehicles (active tickets).                                                                    |
| 4   | **Ticket lookup**   | Anyone can look up a specific ticket by its ID.                                                                                   |


Protected operations (park and exit) require a **JWT token** obtained via login.

---

## 2. High-Level Architecture

The project follows a **layered architecture** — each layer has a clear responsibility:

```
HTTP Request
    ↓
Controllers          → Receive HTTP, return status codes. No database access.
    ↓
Application Layer    → DTOs, validation, services, mappers.
    ↓
Domain Layer         → Business rules, entities, OOP (vehicles, spots, fees).
    ↓
Infrastructure       → EF Core DbContext, MySQL, seed data, migrations.
    ↓
MySQL Database       → ParkingLotDb
```

**Why this matters for evaluation:**  
The controller never talks to the database directly. All logic goes through services. Domain rules (like “can this vehicle fit this spot?”) live in entity classes, not scattered in controllers.

---

## 3. Project Folder Structure

```
OnlineParkingLotSystem/
│
├── Controllers/                    # HTTP endpoints
│   ├── AuthController.cs           # POST api/auth/login
│   └── ParkingController.cs        # All parking endpoints
│
├── Application/
│   ├── DTOs/
│   │   ├── Requests/               # Data coming IN from the client
│   │   │   ├── LoginRequest.cs
│   │   │   └── ParkVehicleRequest.cs
│   │   └── Responses/              # Data going OUT to the client
│   │       ├── AuthResponse.cs
│   │       ├── ExitParkingResponse.cs
│   │       └── ParkingTicketResponse.cs
│   ├── Mappings/
│   │   └── ParkingMapper.cs        # Converts entities → response DTOs
│   └── Services/
│       ├── IAuthService.cs / AuthService.cs
│       └── IParkingService.cs / ParkingService.cs
│
├── Domain/
│   ├── Entities/                   # Core business objects
│   ├── Enums/                      # VehicleType, SpotType, FeeStrategyType, etc.
│   ├── Exceptions/                 # Custom domain exceptions
│   ├── Factories/
│   │   └── VehicleFactory.cs       # Creates the right vehicle subclass
│   └── Strategies/                 # Fee calculation (Strategy pattern)
│
├── Infrastructure/
│   └── Data/
│       ├── AppDbContext.cs         # EF Core database context
│       └── DbSeeder.cs             # Initial parking lot data
│
├── Middleware/
│   ├── RequestLoggingMiddleware.cs
│   └── GlobalExceptionHandler.cs
│
├── Migrations/                     # EF Core database migrations
├── Program.cs                      # App startup, DI, JWT, middleware
├── appsettings.json                # Connection string, JWT, login credentials
└── OnlineParkingLotSystem.http     # Sample API requests for testing
```

---

## 4. Domain Entities and Relationships

There are **five entities**, as required by the assignment.

### 4.1 Entity Diagram

```
ParkingLot (1) ──────< (many) Floor (1) ──────< (many) ParkingSpot

Vehicle (1) ──────< (many) ParkingTicket (many) >────── (1) ParkingSpot
                              (while active: exactly one spot per ticket)
```

### 4.2 ParkingLot

Represents the entire parking facility.


| Property   | Type       | Description                  |
| ---------- | ---------- | ---------------------------- |
| `Id`       | int        | Primary key                  |
| `Name`     | string     | e.g. "City Center Parking"   |
| `Location` | string     | e.g. "Main Street, Downtown" |
| `Floors`   | Collection | All floors in this lot       |


### 4.3 Floor

Represents one level inside the parking lot.


| Property       | Type       | Description               |
| -------------- | ---------- | ------------------------- |
| `Id`           | int        | Primary key               |
| `FloorNumber`  | int        | e.g. 1, 2                 |
| `ParkingLotId` | int        | Foreign key to ParkingLot |
| `ParkingSpots` | Collection | All spots on this floor   |


### 4.4 ParkingSpot (Abstract Base Class)

Represents a single parking space.


| Property     | Type             | Description                                |
| ------------ | ---------------- | ------------------------------------------ |
| `Id`         | int              | Primary key                                |
| `SpotNumber` | string           | e.g. "C-101", "L-201"                      |
| `SpotType`   | enum             | Compact, Large, or Handicapped             |
| `FloorId`    | int              | Foreign key to Floor                       |
| `IsOccupied` | bool (read-only) | Whether a vehicle is currently parked here |


**Subclasses:**


| Class             | Fits which vehicles?   |
| ----------------- | ---------------------- |
| `CompactSpot`     | Motorcycle, Car        |
| `LargeSpot`       | Motorcycle, Car, Truck |
| `HandicappedSpot` | Motorcycle, Car        |


Each subclass **overrides** `CanFit(Vehicle vehicle)` to decide compatibility.

### 4.5 Vehicle (Abstract Base Class)

Represents a vehicle that can park multiple times over its lifetime.


| Property         | Type              | Description                           |
| ---------------- | ----------------- | ------------------------------------- |
| `Id`             | int               | Primary key                           |
| `LicensePlate`   | string            | Unique identifier (e.g. "ABC-1234")   |
| `VehicleType`    | enum              | Motorcycle, Car, or Truck             |
| `Size`           | abstract property | Small, Medium, or Large               |
| `ParkingTickets` | Collection        | All parking sessions for this vehicle |


**Subclasses:**


| Class        | Size   | VehicleType |
| ------------ | ------ | ----------- |
| `Motorcycle` | Small  | Motorcycle  |
| `Car`        | Medium | Car         |
| `Truck`      | Large  | Truck       |


### 4.6 ParkingTicket

Represents **one parking session** (one visit).


| Property          | Type      | Description                               |
| ----------------- | --------- | ----------------------------------------- |
| `Id`              | int       | Primary key (ticket ID)                   |
| `VehicleId`       | int       | Which vehicle parked                      |
| `ParkingSpotId`   | int       | Which spot was assigned                   |
| `EntryTime`       | DateTime  | When the vehicle entered                  |
| `ExitTime`        | DateTime? | When the vehicle left (null while active) |
| `FeePaid`         | decimal?  | Fee charged on exit (null while active)   |
| `IsActive`        | bool      | `true` = still parked, `false` = closed   |
| `FeeStrategyType` | enum      | Hourly, Daily, or Flat                    |


**Important rules:**

- A vehicle can have many tickets over time, but **only one active ticket** at a time.
- While active, a ticket is linked to **exactly one spot**.
- A spot can have **only one active ticket** at a time (enforced by `Occupy()` / `Release()`).

---

## 5. OOP Concepts (Assignment Requirements)

This section maps directly to what evaluators will look for.

### 5.1 Inheritance — Vehicles

**Requirement:** Three vehicle types with different sizes using an abstract base class.

**Implementation:**

- **Abstract base:** `Vehicle` — defines `LicensePlate`, `VehicleType`, and abstract `Size`.
- **Concrete classes:** `Motorcycle` (Small), `Car` (Medium), `Truck` (Large).

**Why inheritance here:** Each vehicle type knows its own size. The parking spot’s `CanFit()` method checks `vehicle.Size` without caring about the specific vehicle class name.

**Factory:** `VehicleFactory.Create(licensePlate, vehicleType)` creates the correct subclass based on the enum from the API request.

### 5.2 Inheritance — Parking Spots

**Requirement:** Three spot types, each deciding whether a vehicle fits.

**Implementation:**

- **Abstract base:** `ParkingSpot` — defines common properties and abstract `CanFit(Vehicle vehicle)`.
- **Concrete classes:** `CompactSpot`, `LargeSpot`, `HandicappedSpot` — each overrides `CanFit()`.

**Example (CompactSpot):**

```csharp
public override bool CanFit(Vehicle vehicle) =>
    vehicle.Size is VehicleSize.Small or VehicleSize.Medium;
```

This is **polymorphism**: the service calls `spot.CanFit(vehicle)` on any spot type, and the correct subclass logic runs automatically.

### 5.3 Encapsulation — Spot Occupancy

**Requirement:** Occupied status cannot be changed directly from outside. Occupying an occupied spot or freeing a free spot must be prevented.

**Implementation:**

```csharp
private bool _isOccupied;           // Private field — cannot be set from outside
public bool IsOccupied => _isOccupied;  // Read-only property

public void Occupy()   { /* throws if already occupied */ }
public void Release()  { /* throws if already free */ }
```

- Nobody can write `_isOccupied = true` from another class.
- Status changes **only** through `Occupy()` and `Release()`.
- Invalid operations throw `SpotAlreadyOccupiedException` or `SpotAlreadyFreeException`.

EF Core is configured to use the private field `_isOccupied` for persistence:

```csharp
entity.Property(spot => spot.IsOccupied).HasField("_isOccupied");
```

### 5.4 Interface + Polymorphism — Fee Calculation

**Requirement:** Three fee strategies via an interface. Exit logic must **not** use if/else on fee type.

**Implementation:**

**Interface:**

```csharp
public interface IFeeStrategy
{
    FeeStrategyType StrategyType { get; }
    decimal CalculateFee(DateTime entryTime, DateTime exitTime);
}
```

**Three implementations:**


| Strategy | Class               | Rule                                        |
| -------- | ------------------- | ------------------------------------------- |
| Hourly   | `HourlyFeeStrategy` | Rs 50 per hour (minimum 1 hour, rounded up) |
| Daily    | `DailyFeeStrategy`  | Rs 500 per day (minimum 1 day, rounded up)  |
| Flat     | `FlatFeeStrategy`   | Rs 200 flat, regardless of duration         |


**Resolver:** `FeeStrategyResolver` picks the right strategy from DI:

```csharp
var feeStrategy = _feeStrategyResolver.Resolve(ticket.FeeStrategyType);
var fee = feeStrategy.CalculateFee(ticket.EntryTime, exitTime);
```

No `if (feeType == Hourly) ... else if (feeType == Daily) ...` in exit logic — that satisfies the assignment rule.

All three strategies are registered in `Program.cs`:

```csharp
builder.Services.AddSingleton<IFeeStrategy, HourlyFeeStrategy>();
builder.Services.AddSingleton<IFeeStrategy, DailyFeeStrategy>();
builder.Services.AddSingleton<IFeeStrategy, FlatFeeStrategy>();
builder.Services.AddSingleton<FeeStrategyResolver>();
```

---

## 6. Business Logic Flow

### 6.1 Park Vehicle (`ParkVehicleAsync`)

Step-by-step:

1. **Normalize** license plate (trim, uppercase).
2. **Find vehicle** in database by license plate, or **create new** via `VehicleFactory`.
3. If vehicle exists but **type mismatch** → throw `VehicleTypeMismatchException`.
4. If vehicle **already has active ticket** → throw `VehicleAlreadyParkedException`.
5. **Query unoccupied spots** from database using LINQ: `.Where(spot => !spot.IsOccupied)`.
6. **Filter in memory** with polymorphism: `.FirstOrDefault(spot => spot.CanFit(vehicle))`.
7. If no spot found → throw `NoAvailableSpotException`.
8. Call `availableSpot.Occupy()` (encapsulation).
9. Create `ParkingTicket` with `EntryTime = UtcNow`, `IsActive = true`, and fee strategy (default Hourly if not specified).
10. Save to database and return response DTO.

### 6.2 Exit Vehicle (`ExitVehicleAsync`)

1. Load ticket by ID (with vehicle, spot, floor). Not found → `TicketNotFoundException`.
2. If ticket not active → `TicketAlreadyClosedException`.
3. Resolve fee strategy via `FeeStrategyResolver` (no if/else on fee type).
4. Calculate fee using `CalculateFee(entryTime, exitTime)`.
5. Set `ExitTime`, `FeePaid`, `IsActive = false`.
6. Call `ticket.ParkingSpot.Release()`.
7. Save and return exit response DTO.

### 6.3 Get Active Tickets

- Query all tickets where `IsActive == true`.
- Include vehicle, spot, and floor data.
- Map to list of `ParkingTicketResponse`.

### 6.4 Get Ticket By ID

- Load ticket with related data.
- Not found → `TicketNotFoundException`.
- Map to `ParkingTicketResponse`.

---

## 7. API Endpoints

Base URL (local): `http://localhost:5143`

### 7.1 POST `/api/auth/login` — Login (Public)

**Purpose:** Get a JWT token for protected endpoints.

**Request body:**

```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Success (200):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-06-25T17:00:00Z"
}
```

Credentials are stored in `appsettings.json` under `"Auth"`.

---

### 7.2 POST `/api/parking/park` — Park Vehicle (JWT Required)

**Purpose:** Vehicle arrives; system assigns a spot and creates a ticket.

**Headers:**

```
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

**Request body:**

```json
{
  "licensePlate": "ABC-1234",
  "vehicleType": 2,
  "feeStrategyType": 1
}
```

**Enum values:**


| vehicleType | Meaning    |
| ----------- | ---------- |
| 1           | Motorcycle |
| 2           | Car        |
| 3           | Truck      |



| feeStrategyType | Meaning | Rate         |
| --------------- | ------- | ------------ |
| 1               | Hourly  | Rs 50/hour   |
| 2               | Daily   | Rs 500/day   |
| 3               | Flat    | Rs 200 fixed |


`feeStrategyType` is **optional** — defaults to Hourly (extension task included).

**Success (201 Created):** Returns full ticket details including spot number, floor, entry time.

**Possible errors:**


| Status | When                                         |
| ------ | -------------------------------------------- |
| 400    | Invalid request body / vehicle type mismatch |
| 401    | Missing or invalid JWT                       |
| 409    | Vehicle already parked / no available spot   |


---

### 7.3 PUT `/api/parking/exit/{ticketId}` — Exit Vehicle (JWT Required)

**Purpose:** Vehicle leaves; fee calculated, ticket closed, spot freed.

**Headers:**

```
Authorization: Bearer <your-jwt-token>
```

**Example:** `PUT /api/parking/exit/1`

**Success (200):**

```json
{
  "ticketId": 1,
  "licensePlate": "ABC-1234",
  "feePaid": 50.00,
  "entryTime": "2026-06-25T10:00:00Z",
  "exitTime": "2026-06-25T10:45:00Z",
  "spotNumber": "C-101"
}
```

**Possible errors:**


| Status | When                   |
| ------ | ---------------------- |
| 401    | Missing or invalid JWT |
| 404    | Ticket not found       |
| 409    | Ticket already closed  |


---

### 7.4 GET `/api/parking/active` — Active Tickets (Public)

**Purpose:** Manager view — all currently parked vehicles.

**Success (200):** Array of ticket objects.

---

### 7.5 GET `/api/parking/ticket/{ticketId}` — Ticket By ID (Public)

**Purpose:** Look up any ticket (active or closed).

**Example:** `GET /api/parking/ticket/1`

**Success (200):** Single ticket object.

**Error (404):** Ticket not found.

---

## 8. Authentication (JWT)

### How it works

1. Client sends username/password to `/api/auth/login`.
2. `AuthService` compares against hardcoded values in `appsettings.json`.
3. If valid, a **JWT token** is generated (signed with secret key from config).
4. Client sends token in header: `Authorization: Bearer <token>`.
5. ASP.NET Core validates token on protected endpoints.

### Configuration (`appsettings.json`)

```json
"Jwt": {
  "Key": "OnlineParkingLotSystemSecretKey2026Assignment!",
  "Issuer": "OnlineParkingLotSystem",
  "Audience": "OnlineParkingLotSystem",
  "ExpiryMinutes": 60
},
"Auth": {
  "Username": "admin",
  "Password": "admin123"
}
```

### Which endpoints need JWT?


| Endpoint                             | Auth required? |
| ------------------------------------ | -------------- |
| POST `/api/auth/login`               | No             |
| POST `/api/parking/park`             | **Yes**        |
| PUT `/api/parking/exit/{ticketId}`   | **Yes**        |
| GET `/api/parking/active`            | No             |
| GET `/api/parking/ticket/{ticketId}` | No             |


Implemented using `[Authorize]` and `[AllowAnonymous]` attributes on controller actions.

---

## 9. DTOs, Validation, and Mapping

### Why DTOs?

We never expose entity classes directly to the API. DTOs (Data Transfer Objects) control exactly what the client sends and receives.

### Request DTOs (incoming)

`**ParkVehicleRequest`:**

- `[Required]` on license plate and vehicle type
- `[StringLength(20, MinimumLength = 2)]` on license plate
- `[EnumDataType]` ensures valid enum values

`**LoginRequest`:**

- `[Required]` on username and password

ASP.NET Core automatically returns **400 Bad Request** if validation fails (via `[ApiController]`).

### Response DTOs (outgoing)


| DTO                     | Used when                   |
| ----------------------- | --------------------------- |
| `ParkingTicketResponse` | Park, get active, get by ID |
| `ExitParkingResponse`   | Exit                        |
| `AuthResponse`          | Login                       |


### Static Mapper

`ParkingMapper` is a **static class** with methods like `ToResponse()` and `ToExitResponse()`.

**Why static mapper:** Converts entities → DTOs in one place. Controllers and services stay clean.

**Rule followed:** Controller never maps manually — service returns DTOs after calling the mapper.

---

## 10. Services and Dependency Injection

### Interfaces


| Interface         | Implementation   | Responsibility             |
| ----------------- | ---------------- | -------------------------- |
| `IParkingService` | `ParkingService` | All parking business logic |
| `IAuthService`    | `AuthService`    | Login and JWT generation   |


### Registration (`Program.cs`)

```csharp
builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
```

### Rules followed

- Controllers receive services via **constructor injection** — no `new ParkingService()` in controllers.
- All service methods are `**async`** (Session 2 requirement).
- Controllers **never touch the database** — only call service methods.

---

## 11. Database (EF Core + MySQL)

### Provider

**Pomelo.EntityFrameworkCore.MySql** — connects EF Core to MySQL.

### Connection string

In `appsettings.json`:

```
Server=localhost;Port=3306;Database=ParkingLotDb;User=root;Password=<your-password>;
```

Database name: `**ParkingLotDb**` — lives on your MySQL server (not a file in the project folder).

### DbContext — Five DbSets (required)

```csharp
public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();
public DbSet<Floor> Floors => Set<Floor>();
public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();
public DbSet<Vehicle> Vehicles => Set<Vehicle>();
public DbSet<ParkingTicket> ParkingTickets => Set<ParkingTicket>();
```

### Inheritance in database (TPH — Table Per Hierarchy)

Both `Vehicle` and `ParkingSpot` use **discriminators** — one table per hierarchy, with a column indicating the subclass type:

- `Vehicle` table: discriminator column `VehicleType` (Motorcycle / Car / Truck)
- `ParkingSpot` table: discriminator column `SpotType` (Compact / Large / Handicapped)

### Migrations

Migration name: `**InitialCreate`** (in `Migrations/` folder).

**Create/update database:**

```powershell
dotnet ef database update
```

Migrations also run automatically on app startup via:

```csharp
await context.Database.MigrateAsync();
```

### Seed data (`DbSeeder`)

Runs after migration if no parking lot exists yet.


| Item            | Details                                            |
| --------------- | -------------------------------------------------- |
| Parking lot     | "City Center Parking" at "Main Street, Downtown"   |
| Floors          | Floor 1 and Floor 2                                |
| Spots per floor | 5 Compact, 3 Large, 2 Handicapped (20 spots total) |


Spot numbering examples: `C-101` (compact), `L-101` (large), `H-101` (handicapped) on floor 1; `C-201`, etc. on floor 2.

---

## 12. Middleware and Error Handling

### Request Logging Middleware

Logs every HTTP request:

- **Incoming:** method and path
- **Completed:** method, path, status code, duration in ms

Registered in `Program.cs`:

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

### Global Exception Handler

Implements `IExceptionHandler` (.NET 8 pattern).

Catches all unhandled exceptions and returns **ProblemDetails** JSON:

```json
{
  "status": 404,
  "title": "Ticket not found",
  "detail": "Parking ticket with id '99' was not found.",
  "instance": "/api/parking/ticket/99"
}
```

### Custom domain exceptions


| Exception                       | HTTP Status | When thrown                |
| ------------------------------- | ----------- | -------------------------- |
| `TicketNotFoundException`       | 404         | Ticket ID doesn't exist    |
| `InvalidCredentialsException`   | 401         | Wrong login                |
| `VehicleAlreadyParkedException` | 409         | Vehicle has active ticket  |
| `VehicleTypeMismatchException`  | 400         | Same plate, different type |
| `TicketAlreadyClosedException`  | 409         | Exit on closed ticket      |
| `NoAvailableSpotException`      | 409         | No free compatible spot    |
| `SpotAlreadyOccupiedException`  | 409         | Occupy occupied spot       |
| `SpotAlreadyFreeException`      | 409         | Release free spot          |
| Other `DomainException`         | 400         | General domain errors      |
| Anything else                   | 500         | Unexpected server error    |


All custom exceptions inherit from abstract `DomainException`.

---

## 13. Session Requirements Checklist

Use this table during evaluation to show you covered every session.

### Session 1 — OOP + LINQ


| Requirement                          | Where implemented                                                      |
| ------------------------------------ | ---------------------------------------------------------------------- |
| Abstract vehicle hierarchy           | `Domain/Entities/Vehicle.cs` + subclasses                              |
| Abstract spot hierarchy + `CanFit()` | `Domain/Entities/ParkingSpot.cs` + subclasses                          |
| Fee strategy interface               | `Domain/Strategies/IFeeStrategy.cs`                                    |
| LINQ for available spots             | `ParkingService.ParkVehicleAsync` — `.Where(spot => !spot.IsOccupied)` |


### Session 2 — Async, exceptions, middleware


| Requirement                | Where implemented                              |
| -------------------------- | ---------------------------------------------- |
| Async service methods      | All methods in `ParkingService`, `AuthService` |
| Custom exceptions          | `Domain/Exceptions/`                           |
| Request logging middleware | `Middleware/RequestLoggingMiddleware.cs`       |
| Global exception handler   | `Middleware/GlobalExceptionHandler.cs`         |


### Session 3 — DTOs, DI, validation


| Requirement                 | Where implemented                                        |
| --------------------------- | -------------------------------------------------------- |
| Request/response DTOs       | `Application/DTOs/`                                      |
| Static mapper               | `Application/Mappings/ParkingMapper.cs`                  |
| DI registration             | `Program.cs`                                             |
| Validation on DTOs          | Data annotations on request DTOs                         |
| Controller doesn't touch DB | Controllers only call `IParkingService` / `IAuthService` |


### Session 4 — EF Core + MySQL + JWT


| Requirement               | Where implemented                                  |
| ------------------------- | -------------------------------------------------- |
| EF Core with Pomelo MySQL | `Program.cs`, `AppDbContext.cs`                    |
| Five DbSets               | `AppDbContext.cs`                                  |
| Migrations                | `Migrations/InitialCreate`                         |
| Seed data                 | `Infrastructure/Data/DbSeeder.cs`                  |
| JWT in Program.cs         | `Program.cs` — `AddAuthentication`, `AddJwtBearer` |


### Optional extension implemented


| Extension                               | Status                                      |
| --------------------------------------- | ------------------------------------------- |
| Fee strategy selectable on park request | ✅ `feeStrategyType` in `ParkVehicleRequest` |
| History by license plate                | ❌ Not implemented                           |
| Pagination on active tickets            | ❌ Not implemented                           |


---

## 14. How to Run and Test

### Prerequisites

- .NET 8 SDK
- MySQL running locally (MySQL Workbench)
- EF tools: `dotnet tool install --global dotnet-ef`

### Steps

1. **Update** `appsettings.json` with your MySQL username and password.
2. **Create database:**
  ```powershell
   cd "C:\Users\Yasha Ali\Projects\OnlineParkingLotSystem"
   dotnet ef database update
  ```
3. **Run the API:**
  ```powershell
   dotnet run
  ```
4. **Verify in MySQL Workbench:**
  ```sql
   SHOW DATABASES;
   USE ParkingLotDb;
   SHOW TABLES;
   SELECT * FROM ParkingLots;
   SELECT * FROM ParkingSpots LIMIT 10;
  ```
5. **Test flow:**
  - Login → copy token
  - Park a vehicle (with token)
  - GET active tickets (no token)
  - Exit vehicle (with token)
  - GET ticket by ID (no token)

Use `OnlineParkingLotSystem.http` in VS Code / Cursor for ready-made requests.

---

## 15. What to Say During Evaluation

Short talking points if the evaluator asks you to explain the project:

1. **Architecture:** "I used a layered design — controllers handle HTTP, services contain business logic, domain entities hold rules, and infrastructure handles the database."
2. **Vehicle/Spot inheritance:** "Both use abstract base classes. Spots override `CanFit()` so each spot type decides compatibility polymorphically."
3. **Encapsulation:** "Spot occupancy uses a private field. You can only change it through `Occupy()` and `Release()`, which throw if the operation is invalid."
4. **Fee strategies:** "I used the Strategy pattern with `IFeeStrategy`. On exit, the resolver picks the strategy — there's no if/else on fee type in the exit method."
5. **LINQ:** "I query unoccupied spots with LINQ from the database, then use `CanFit()` in memory because EF can't translate that polymorphic call to SQL."
6. **Security:** "Park and exit require JWT. Login uses hardcoded credentials from config. GET endpoints are public for manager lookup."
7. **Errors:** "Domain exceptions map to HTTP status codes through a global exception handler that returns ProblemDetails."
8. **Database:** "EF Core with Pomelo for MySQL. TPH inheritance for vehicles and spots. Migrations create schema; seeder adds one lot, two floors, and twenty spots."

---

## NuGet Packages Used


| Package                                         | Purpose                 |
| ----------------------------------------------- | ----------------------- |
| `Pomelo.EntityFrameworkCore.MySql`              | MySQL database provider |
| `Microsoft.EntityFrameworkCore.Design`          | Migrations tooling      |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT authentication      |
| `System.IdentityModel.Tokens.Jwt`               | JWT token creation      |


---

*Documentation generated for assignment evaluation — Online Parking Lot System, June 2026.*