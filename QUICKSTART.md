# SchedulIX Quick Start Guide

## Overview
SchedulIX is now a fully-functional RESTful Web API that implements the technical specifications for an Automated Academic Schedule Generation Engine.

## What's Been Implemented

### ✅ Complete Feature Set
- **Room Management**: CRUD operations, availability scheduling, occupancy tracking
- **Teacher Preferences**: Time windows, mandatory room assignments, discipline-specific preferences
- **Academic Structure**: Groups, subgroups, series for flexible student organization
- **Schedule Management**: Generation, validation, and export capabilities
- **Export Functionality**: Excel (.xlsx) and PDF formats
- **Data Persistence**: SQL Server integration via Entity Framework Core 8
- **REST API**: Open API (Swagger) documentation
- **Validation**: Hard and soft constraint checking

### ✅ Architecture
- **Design Patterns**: Repository pattern, Dependency Injection, DTOs
- **Layering**: Controllers → Services → Repositories → Data Layer
- **Configuration**: Environment-based configuration, CORS enablement

---

## Getting Started (5 Minutes)

### 1. Install Dependencies
```bash
dotnet restore
```

### 2. Configure Database Connection
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=schedulix_db;Trusted_Connection=true;"
  }
}
```

### 3. Create & Apply Database
```bash
dotnet ef migrations add InitialCreate --project SchedulIX
dotnet ef database update --project SchedulIX
```

### 4. Run Application
```bash
dotnet run
```

### 5. Open Swagger UI
Navigate to: **https://localhost:5001/swagger** (adjust port if needed)

---

## API Quick Reference

### 🏫 Rooms Endpoints
```
GET    /api/rooms                           - List all rooms
GET    /api/rooms/{id}                      - Get room details
GET    /api/rooms/{id}/calendar             - View room schedule
POST   /api/rooms                           - Create new room
PUT    /api/rooms/{id}                      - Update room
PATCH  /api/rooms/{id}/status               - Mark room available/unavailable
```

### 📋 Schedules Endpoints
```
POST   /api/schedules/generate              - Generate schedule
GET    /api/schedules/{id}                  - Get schedule details
POST   /api/schedules/validate              - Validate schedule
```

### 📊 Export Endpoints
```
GET    /api/export/excel                    - Export full schedule to Excel
GET    /api/export/pdf                      - Export full schedule to PDF
GET    /api/export/room/{roomId}/excel      - Export room calendar
```

---

## Example API Calls

### Create a Room
```bash
curl -X POST https://localhost:5001/api/rooms \
  -H "Content-Type: application/json" \
  -d '{
	"roomNumber": "201",
	"capacity": 50,
	"roomTypeId": 1,
	"availabilities": [
	  {
		"dayOfWeek": 1,
		"startTime": "08:00:00",
		"endTime": "18:00:00"
	  }
	]
  }'
```

### Generate Schedule
```bash
curl -X POST https://localhost:5001/api/schedules/generate \
  -H "Content-Type: application/json" \
  -d '{
	"academicYearId": "00000000-0000-0000-0000-000000000000",
	"groupIds": ["00000000-0000-0000-0000-000000000001"],
	"allowSoftConstraintViolations": true
  }'
```

### Get Room Calendar
```bash
curl -X GET https://localhost:5001/api/rooms/1/calendar
```

### Export Schedule
```bash
curl -X GET https://localhost:5001/api/export/excel \
  -o schedule.xlsx
```

---

## Project Structure

```
SchedulIX/
├── Controllers/          # API endpoints
├── Models/              # Entities and DTOs
│   ├── Entities/       # EF Core domain models
│   └── DTOs/           # Data transfer objects
├── Services/           # Business logic
├── Repositories/       # Data access abstractions
├── Data/              # Entity Framework context
├── Migrations/        # Database version control
└── wwwroot/          # Static files (HTML, CSS, JS)
```

---

## Key Files & Changes

| File | Purpose |
|------|---------|
| `ScheduleDbContext.cs` | EF Core database context with 11 entities |
| `Program.cs` | Dependency injection & middleware configuration |
| `RoomsController.cs` | Room management API endpoints |
| `ExportService.cs` | Excel/PDF generation using ClosedXML & iTextSharp |
| `appsettings.json` | Database connection & application settings |

---

## Database Schema

The following tables are automatically created:

| Table | Description |
|-------|-------------|
| `TipuriSali` | Room types (Lecture, Lab, Seminar) |
| `Sali` | Room resources |
| `DisponibilitateSali` | Room availability windows |
| `Profesori` | Teachers (placeholder structure) |
| `PreferinteProfesori` | Teacher preferences & constraints |
| `Discipline` | Subjects/courses |
| `GrilaOrare` | Time slots (5 fixed periods + lunch) |
| `Grupe` | Student groups |
| `Subgrupe` | Group subdivisions |
| `Serii` | Group collections for shared courses |
| `FormeEducatie` | Education programs |
| `Orar` | Individual scheduled sessions |

---

## NuGet Packages Used

```
✓ Microsoft.EntityFrameworkCore.SqlServer (8.0.7)
✓ Microsoft.EntityFrameworkCore.Design (8.0.7)
✓ Swashbuckle.AspNetCore (10.1.7)  - Swagger/OpenAPI
✓ ClosedXML (0.102.2)               - Excel export
✓ iTextSharp (5.5.13.3)             - PDF generation
```

---

## Validation Rules Implemented

### Hard Constraints (Must be satisfied)
- ✅ Maximum 5 periods per day per group
- ✅ Room capacity must accommodate group size
- ✅ Respect room availability windows

### Soft Constraints (Preferential)
- ⚠️ Maximum 3 courses with same teacher per day
- ⚠️ Avoid 3+ consecutive courses
- ⚠️ Separate seminars from lectures when possible
- ⚠️ Respect teacher time preferences

---

## Common Tasks

### Add a New Room Type
```sql
INSERT INTO TipuriSali (Denumire, AreCalculatoare) 
VALUES ('Conference Room', 1);
```

### Add a Teacher
```sql
INSERT INTO Profesori (FirstName, LastName, Email)
VALUES ('John', 'Doe', 'john.doe@university.edu');
```

### Schedule Export
Use Swagger UI or cURL to download schedules in Excel/PDF format.

---

## Troubleshooting

### Database Connection Error
- Ensure LocalDB is running: `sqllocaldb start mssqllocaldb`
- Verify connection string matches your SQL Server configuration
- Check firewall allows SQL Server connections

### Port Already in Use
```bash
# Change port in launchSettings.json or use:
dotnet run --urls="https://localhost:5002"
```

### Build Fails
```bash
dotnet clean
dotnet restore
dotnet build
```

### Migrations Error
```bash
# Check migration status
dotnet ef migrations list

# Remove failed migration
dotnet ef migrations remove
```

---

## Next Steps

1. **Seed Initial Data** - Use MIGRATION_GUIDE.md for SQL scripts
2. **Implement Authentication** - Add JWT or Azure AD
3. **Optimize Scheduling Algorithm** - Integrate OR-Tools or custom solver
4. **Deploy** - Ready for containerization (Docker) or Azure deployment
5. **Add Frontend** - Build UI to consume these API endpoints

---

## Documentation

- **IMPLEMENTATION.md** - Comprehensive technical implementation details
- **MIGRATION_GUIDE.md** - Database setup and seed data instructions
- **README.md** - Original technical specifications
- **Swagger UI** - Interactive API documentation at `/swagger`

---

## Support Resources

- [Entity Framework Core Docs](https://docs.microsoft.com/ef/)
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/)
- [SQL Server Docs](https://docs.microsoft.com/sql/)
- [ClosedXML Documentation](https://closedxml.codeplex.com/)

---

## Build Status
✅ **Build Successful** - Ready for testing and deployment

