# SchedulIX - Technical Implementation Summary

## Overview
SchedulIX has been successfully updated to implement the complete Technical Specifications for an Automated Academic Schedule Generation Engine. The project is now a RESTful Web API built with .NET 10 that supports room management, teacher preferences, academic group structuring, and schedule generation/export.

---

## Key Implementations

### 1. **Database Architecture (Entity Framework Core)**

#### Entities Created:
- **RoomType** - Categories of rooms (Lecture Hall, Laboratory, Seminar Room)
- **Room** - Individual rooms with capacity and equipment details
- **RoomAvailability** - Availability windows for each room (day/time slots)
- **Teacher** - Faculty members
- **TeacherPreference** - Disciplinary preferences, time preferences, mandatory room assignments
- **Discipline** - Course subjects with standard hour distribution (2 lectures, 2 seminars, 2 labs)
- **TimeSlot** - Fixed time grid (5 slots + lunch break) with predefined scheduling
- **AcademicGroup** - Student groups (e.g., CR-211)
- **Subgroup** - Subdivisions of groups for practical work
- **Series** - Collections of groups for shared lectures
- **EducationForm** - Program levels (year, semester, form of education)
- **Schedule** - Individual scheduled sessions linking all resources

#### Database Configuration:
- **Provider**: SQL Server (via Entity Framework Core 8.0.7)
- **Connection**: Supports both LocalDB and remote connections
- **Seeding**: Default TimeSlots pre-populated during migration

### 2. **Repository Pattern Implementation**

#### IRoomRepository & IScheduleRepository
- Abstractions for data access
- Supports CRUD operations and filtering
- Efficient entity loading with lazy loading configuration

### 3. **API Endpoints**

#### Schedule Management:
- `POST /api/schedules/generate` - Trigger automatic schedule generation
- `GET /api/schedules/{id:guid}` - Retrieve schedule details
- `POST /api/schedules/validate` - Validate schedule against constraints

#### Room Management:
- `GET /api/rooms` - List all rooms
- `GET /api/rooms/{id:int}` - Get room details
- `GET /api/rooms/{id:int}/calendar` - View room's occupancy calendar
- `POST /api/rooms` - Create new room with availability windows
- `PUT /api/rooms/{id:int}` - Update room information
- `PATCH /api/rooms/{id:int}/status` - Mark room as available/unavailable

#### Export & Reporting:
- `GET /api/export/excel` - Export complete schedule to Excel (.xlsx)
- `GET /api/export/pdf` - Export schedule to PDF
- `GET /api/export/room/{roomId:int}/excel` - Export specific room calendar to Excel

### 4. **Data Transfer Objects (DTOs)**

Organized in dedicated DTO files:
- **RoomDtos.cs** - Room, RoomType, RoomAvailability resources
- **TeacherDtos.cs** - Teacher and preference data
- **DisciplineDtos.cs** - Course information
- **AcademicStructureDtos.cs** - Groups, subgroups, series, education forms

### 5. **Export Services**

#### IExportService Implementation:
- **Excel Export**: Uses ClosedXML library
  - Formatted headers with styling
  - Organized schedule data by day/slot
  - Adjusted column widths

- **PDF Export**: Uses iTextSharp library
  - Professional table layout
  - Landscape A4 format
  - Header and footer information
  - Room-specific export capability

### 6. **Constraint Validation**

Basic validation rules implemented in `ScheduleService.ValidateScheduleAsync()`:
- Maximum 5 periods per day per group
- Detects excessive consecutive courses
- Calculates soft constraint scores

---

## Project Structure

```
SchedulIX/
├── Controllers/
│   ├── SchedulesController.cs      # Schedule endpoints
│   ├── RoomsController.cs          # Room management endpoints
│   └── ExportController.cs         # Export functionality
├── Models/
│   ├── Entities/                   # EF Core domain models
│   ├── DTOs/                       # Data transfer objects
│   └── ScheduleDto.cs              # Core schedule DTOs
├── Data/
│   └── ScheduleDbContext.cs        # Entity Framework context
├── Services/
│   ├── Interfaces/
│   │   ├── IScheduleService.cs
│   │   └── IExportService.cs
│   └── Implementations/
│       ├── ScheduleService.cs      # Core business logic
│       └── ExportService.cs        # Excel/PDF generation
├── Repositories/
│   ├── Interfaces/
│   │   ├── IRoomRepository.cs
│   │   └── IScheduleRepository.cs
│   └── Implementations/
│       ├── RoomRepository.cs
│       └── ScheduleRepository.cs
├── Program.cs                      # Startup configuration
├── appsettings.json                # Configuration
└── SchedulIX.csproj               # Project file
```

---

## Database Schema Mapping

The implementation follows the technical specifications schema:

| Table | Entity | Purpose |
|-------|--------|---------|
| TipuriSali | RoomType | Room classifications |
| Sali | Room | Physical classroom resources |
| DisponibilitateSali | RoomAvailability | Room availability windows |
| PreferinteProfesori | TeacherPreference | Teacher constraints & preferences |
| GrilaOrare | TimeSlot | Fixed scheduling grid |
| Orar | Schedule | Individual class sessions |
| Grupe/Subgrupe/Serii | AcademicGroup/Subgroup/Series | Student organization |

---

## NuGet Dependencies

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.7" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
<PackageReference Include="ClosedXML" Version="0.102.2" />
<PackageReference Include="iTextSharp" Version="5.5.13.3" />
```

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=schedulix_db;Trusted_Connection=true;"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.AspNetCore": "Warning"
	}
  },
  "AllowedHosts": "*"
}
```

### CORS Configuration
- Enabled for development environments
- Allows requests from any origin (configurable)
- Supports all HTTP methods

---

## Constraints & Rules Implemented

Based on technical specifications:

### Hard Constraints:
- Maximum 5 periods per day per student group
- Room capacity vs. group size validation
- Room availability windows enforcement

### Soft Constraints:
- Maximum 3 courses with same teacher per group per day
- No consecutive courses (limit of 2)
- Seminars separated from lectures when possible
- Teacher time preferences respected

---

## Future Enhancements

1. **Advanced Scheduling Algorithm**
   - Integrate OR-Tools or Genetic Algorithm for optimization
   - Implement weighted constraint solver
   - Support for complex scheduling scenarios

2. **PostgreSQL Support**
   - Add Npgsql provider (when available in environment)
   - Support multi-database configuration

3. **Additional Export Formats**
   - HTML calendars
   - JSON API responses
   - iCal format for calendar integration

4. **Authentication & Authorization**
   - Role-based access control
   - JWT token authentication
   - User management endpoints

5. **Advanced Analytics**
   - Utilization reports by room
   - Teacher workload analysis
   - Conflict detection and resolution logging

---

## API Usage Examples

### Generate Schedule
```http
POST /api/schedules/generate
Content-Type: application/json

{
  "academicYearId": "550e8400-e29b-41d4-a716-446655440000",
  "groupIds": ["550e8400-e29b-41d4-a716-446655440001"],
  "allowSoftConstraintViolations": true
}
```

### Create Room
```http
POST /api/rooms
Content-Type: application/json

{
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
}
```

### Export Schedule
```http
GET /api/export/excel
```

---

## Building & Running

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or remote)
- Visual Studio 2026 (recommended)

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Access Swagger UI
Navigate to `https://localhost:<port>/swagger` to explore API endpoints.

---

## Notes

- All API responses include proper HTTP status codes and error messages
- Datetime values use UTC internally
- Soft constraint scoring: 100 - (violations_count * 5)
- Days of week encoded as: 1=Monday, ..., 6=Saturday
- Week types: 0=All weeks, 1=Even, 2=Odd

---

## Compliance with Technical Specifications

✅ Room management with availability windows  
✅ Academic structure (groups, subgroups, series)  
✅ Teacher preferences and constraints  
✅ Fixed time grid (5 slots)  
✅ Schedule generation endpoints  
✅ Excel and PDF export functionality  
✅ Room occupancy calendars  
✅ Constraint validation system  
✅ RESTful API architecture  
✅ Entity Framework Core persistence  

