# SchedulIX - Complete File Reference

## Project Files Structure

### 📁 Root Directory Files
```
D:\VisualStudio projects\SchedulIX\SchedulIX\
├── SchedulIX.slnx                 - Solution file
├── README.md                       - Technical specifications
├── IMPLEMENTATION.md               - Technical implementation guide
├── MIGRATION_GUIDE.md             - Database setup instructions
├── QUICKSTART.md                  - 5-minute getting started
└── SUMMARY.md                     - Project completion summary
```

---

## 📂 SchedulIX Project Structure

### Controllers (3 files)
```
SchedulIX/Controllers/
├── SchedulesController.cs         - Schedule management endpoints
│                                   POST /api/schedules/generate
│                                   GET /api/schedules/{id}
│                                   POST /api/schedules/validate
│
├── RoomsController.cs            - Room management endpoints
│                                   GET/POST/PUT /api/rooms
│                                   GET /api/rooms/{id}/calendar
│                                   PATCH /api/rooms/{id}/status
│
└── ExportController.cs           - Export functionality
									GET /api/export/excel
									GET /api/export/pdf
									GET /api/export/room/{id}/excel
```

### Models/Entities (11 files)
```
SchedulIX/Models/Entities/
├── RoomType.cs                   - Room category (Lecture, Lab, Seminar)
├── Room.cs                       - Physical classroom resource
├── RoomAvailability.cs           - Room scheduling windows
├── Teacher.cs                    - Faculty member information
├── TeacherPreference.cs          - Teacher constraints & preferences
├── Discipline.cs                 - Subject/Course definition
├── TimeSlot.cs                   - Fixed schedule grid (5 slots)
├── AcademicGroup.cs              - Student group (e.g., CR-211)
├── Subgroup.cs                   - Group subdivision
├── Series.cs                     - Collection of groups
└── EducationForm.cs              - Program definition
├── Schedule.cs                   - Individual class session
```

### Models/DTOs (4 files)
```
SchedulIX/Models/DTOs/
├── RoomDtos.cs                   - Room-related DTOs
│   ├── RoomTypeDto
│   ├── RoomDto
│   ├── RoomAvailabilityDto
│   ├── CreateRoomRequest
│   ├── RoomCalendarDto
│   └── RoomScheduleItemDto
│
├── TeacherDtos.cs                - Teacher-related DTOs
│   ├── TeacherDto
│   ├── CreateTeacherRequest
│   ├── TeacherPreferenceDto
│   └── CreateTeacherPreferenceRequest
│
├── DisciplineDtos.cs             - Discipline-related DTOs
│   ├── DisciplineDto
│   └── CreateDisciplineRequest
│
└── AcademicStructureDtos.cs      - Academic structure DTOs
	├── AcademicGroupDto
	├── SubgroupDto
	├── CreateAcademicGroupRequest
	├── CreateSubgroupRequest
	├── SeriesDto
	├── CreateSeriesRequest
	├── EducationFormDto
	└── CreateEducationFormRequest
```

### Models (1 file)
```
SchedulIX/Models/
└── ScheduleDto.cs                - Core schedule DTOs
	├── ScheduleDto
	├── ScheduleItemDto
	├── GenerateScheduleRequestDto
	└── ExportScheduleDto
```

### Services (5 files)
```
SchedulIX/Services/
├── Interfaces/
│   ├── IScheduleService.cs       - Schedule business logic contract
│   │   ├── GetByIdAsync()
│   │   ├── GenerateScheduleAsync()
│   │   └── ValidateScheduleAsync()
│   │
│   └── IExportService.cs         - Export functionality contract
│       ├── ExportToExcelAsync()
│       ├── ExportToPdfAsync()
│       └── ExportRoomScheduleToExcelAsync()
│
├── Implementations/
│   ├── ScheduleService.cs        - Schedule generation & validation
│   └── ExportService.cs          - Excel/PDF export implementation
│
└── ScheduleService.cs            - Main schedule service (legacy location)
```

### Repositories (5 files)
```
SchedulIX/Repositories/
├── Interfaces/
│   ├── IRoomRepository.cs        - Room data access contract
│   │   ├── GetByIdAsync()
│   │   ├── GetAllAsync()
│   │   ├── CreateAsync()
│   │   ├── UpdateAsync()
│   │   ├── DeleteAsync()
│   │   └── GetByTypeAsync()
│   │
│   └── IScheduleRepository.cs    - Schedule data access contract
│       ├── GetByIdAsync()
│       ├── GetAllAsync()
│       ├── CreateAsync()
│       ├── CreateBatchAsync()
│       ├── UpdateAsync()
│       ├── DeleteAsync()
│       ├── GetByRoomAsync()
│       ├── GetByGroupAsync()
│       └── GetByTeacherAsync()
│
└── Implementations/
	├── RoomRepository.cs         - Room data access implementation
	└── ScheduleRepository.cs     - Schedule data access implementation
```

### Data (1 file)
```
SchedulIX/Data/
└── ScheduleDbContext.cs          - Entity Framework Core context
	├── DbSet definitions for all 12 entities
	├── Relationship configuration (OnModelCreating)
	├── Cascade delete policies
	├── Foreign key setup
	└── Default TimeSlot seeding
```

### Interfaces (1 file - Legacy Location)
```
SchedulIX/Interfaces/
└── IScheduleService.cs           - Legacy interface location
									(New services in Services/Interfaces/)
```

### Configuration Files (3 files)
```
SchedulIX/
├── Program.cs                    - Application startup & DI configuration
│                                   ├── DbContext registration
│                                   ├── Service registration
│                                   ├── Repository registration
│                                   ├── CORS configuration
│                                   └── Middleware setup
│
├── SchedulIX.csproj              - Project configuration
│                                   ├── .NET 10 target
│                                   ├── NuGet package references
│                                   └── Build settings
│
└── appsettings.json              - Application settings
									├── Database connection string
									└── Logging configuration
```

### Development Configuration (2 files)
```
SchedulIX/Properties/
├── launchSettings.json           - Launch profiles & ports
└── ...

SchedulIX/
└── appsettings.Development.json  - Development-specific settings
```

### Static Files (1 file)
```
SchedulIX/wwwroot/
└── index.html                    - Frontend landing page
```

### Project Definition (1 file)
```
SchedulIX/
└── SchedulIX.http                - HTTP request examples for testing
```

---

## 📊 File Statistics

| Category | Count | Type |
|----------|-------|------|
| **Entities** | 11 | .cs |
| **DTOs** | 4 groups | .cs |
| **Controllers** | 3 | .cs |
| **Services** | 3 | .cs |
| **Repositories** | 4 | .cs |
| **Interfaces** | 4 | .cs |
| **Configuration** | 3 | .cs/.json |
| **Documentation** | 4 | .md |
| **Total Source Files** | 35+ | .cs |

---

## 🔑 Key Implementation Files

### Must-Read Files
1. **Program.cs** - Understand dependency injection and middleware
2. **ScheduleDbContext.cs** - Understand database schema
3. **SchedulesController.cs** - Understand API endpoints
4. **ExportService.cs** - Understand export implementation

### Architecture Files
1. **Repositories/Interfaces/*.cs** - Data access abstraction
2. **Services/Interfaces/*.cs** - Business logic abstraction
3. **Models/DTOs/*.cs** - API contracts

### Database Files
1. **Models/Entities/**.cs** - Domain model definitions
2. **Data/ScheduleDbContext.cs** - Configuration & relationships

---

## 🛠 Configuration Details

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

### SchedulIX.csproj
```xml
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>

<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.7" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
<PackageReference Include="ClosedXML" Version="0.102.2" />
<PackageReference Include="iTextSharp" Version="5.5.13.3" />
```

---

## 🗄 Database Tables Generated

When migrations are applied, these tables are created:

1. **TipuriSali** → RoomType
2. **Sali** → Room
3. **DisponibilitateSali** → RoomAvailability
4. **Profesori** → Teacher (referenced, not created automatically)
5. **PreferinteProfesori** → TeacherPreference
6. **Discipline** → Discipline
7. **GrilaOrare** → TimeSlot (pre-seeded with 5 slots)
8. **Grupe** → AcademicGroup
9. **Subgrupe** → Subgroup
10. **Serii** → Series
11. **FormeEducatie** → EducationForm
12. **Orar** → Schedule

---

## 📝 Naming Conventions

### C# Classes
- PascalCase for class names: `RoomType`, `TeacherPreference`
- PascalCase for properties: `FirstName`, `IsAvailable`
- PascalCase for methods: `GetByIdAsync()`, `CreateAsync()`

### Database Tables
- PascalCase: `Sali`, `Profesori`, `Orar`
- Some plural forms in Romanian: `Sali` (rooms), `Grupe` (groups)

### API Endpoints
- Lowercase with slashes: `/api/rooms`, `/api/schedules`
- Kebab-case for multi-word routes: `/api/rooms/{id}/calendar`

### DTOs
- Suffix with `Dto`: `RoomDto`, `ScheduleItemDto`
- Request/Response patterns: `CreateRoomRequest`, `RoomCalendarDto`

---

## 🔄 Dependency Flow

```
Controllers
	↓ (depends on)
Services (Interfaces)
	↓ (depends on)
Repositories (Interfaces)
	↓ (depends on)
DbContext
	↓ (maps to)
Entities
	↓ (stored in)
Database
```

---

## 📚 Documentation Mapping

| Document | File | Purpose |
|----------|------|---------|
| Technical Specs | README.md | Original requirements |
| Implementation | IMPLEMENTATION.md | Deep technical dive |
| Database Setup | MIGRATION_GUIDE.md | EF Core migrations |
| Quick Start | QUICKSTART.md | 5-minute guide |
| Summary | SUMMARY.md | Project completion |
| File Reference | This file | File organization |

---

## 🧪 Testing Guidance

### Unit Tests Should Cover
- Services/**Service.cs** - Business logic
- Repositories/Implementations/*.cs** - Data access
- Validation in ScheduleService

### Integration Tests Should Cover
- Controllers/*.cs** - API endpoints
- Database operations via DbContext
- Export functionality

### Files to Test First
1. ScheduleService.cs - Core logic
2. ExportService.cs - Export generation
3. SchedulesController.cs - API validation

---

## 🚀 Quick File Navigation

### To Add New Endpoint
1. Create method in Controller (Controllers/)
2. Call existing Service method (Services/Implementations/)
3. Update Service Interface if needed (Services/Interfaces/)
4. Existing repositories handle data access

### To Add New Entity
1. Create entity class (Models/Entities/)
2. Add DbSet to ScheduleDbContext.cs
3. Configure relationships in OnModelCreating()
4. Create Migration: `dotnet ef migrations add`
5. Apply Migration: `dotnet ef database update`

### To Add New Service
1. Create interface (Services/Interfaces/INewService.cs)
2. Create implementation (Services/Implementations/NewService.cs)
3. Register in Program.cs: `builder.Services.AddScoped<INewService, NewService>()`
4. Inject into controller via constructor

---

## 💾 Version Control

**Important Files for Version Control:**
- Keep: All .cs, .json, .csproj, .md files
- Exclude: bin/, obj/, .vs/, *.user files
- Recommended: Use .gitignore for standard .NET projects

---

## 🔒 File Security Notes

**Sensitive Files to Protect:**
- appsettings.json (contains connection strings)
- appsettings.Development.json (dev secrets)
- launchSettings.json (port configuration)

**Recommendation**: Use user-secrets for production credentials

---

## 📞 Quick Reference

### Main Entry Point
→ **Program.cs** (application startup)

### API Definition
→ **Controllers/*.cs** (endpoint routes)

### Business Logic
→ **Services/Implementations/*.cs** (algorithms)

### Data Access
→ **Repositories/Implementations/*.cs** (CRUD)

### Database Schema
→ **Models/Entities/*.cs** (structure) + **Data/ScheduleDbContext.cs** (config)

### API Contracts
→ **Models/DTOs/*.cs** (request/response)

---

This document provides a complete reference for navigating the SchedulIX project structure.

