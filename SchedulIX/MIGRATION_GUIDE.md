# SchedulIX Database Migration Guide

## Initial Setup

This guide explains how to create the database and run migrations.

### Prerequisites
- .NET 8+ CLI installed
- SQL Server LocalDB or a remote SQL Server instance
- Connection string configured in `appsettings.json`

### Step 1: Create Initial Migration

From the project root directory, run:

```bash
dotnet ef migrations add InitialCreate --project SchedulIX --startup-project SchedulIX
```

This will generate the first migration file based on your `ScheduleDbContext` configuration, including all entity models and relationships.

### Step 2: Update Database

Apply the migration to create the database schema:

```bash
dotnet ef database update --project SchedulIX --startup-project SchedulIX
```

This command will:
1. Create the `schedulix_db` database (if it doesn't exist)
2. Create all tables according to the entity relationships
3. Seed default TimeSlot data

### Step 3: Verify Installation

Connect to your database using SQL Server Management Studio (SSMS) or Data Studio and verify these tables exist:

- TipuriSali (RoomTypes)
- Sali (Rooms)
- DisponibilitateSali (RoomAvailabilities)
- Profesori (Teachers) - *Note: Not yet created, add if needed
- PreferinteProfesori (TeacherPreferences)
- Discipline (Disciplines)
- GrilaOrare (TimeSlots) - *Pre-populated with 5 slots*
- Grupe (AcademicGroups)
- Subgrupe (Subgroups)
- Serii (Series)
- FormeEducatie (EducationForms)
- Orar (Schedules)

---

## Making Schema Changes

When you need to modify the database schema:

### 1. Update Entity Models
Edit the entity classes in `Models/Entities/` directory.

### 2. Create New Migration
```bash
dotnet ef migrations add <MigrationName> --project SchedulIX
```

Example:
```bash
dotnet ef migrations add AddTeacherContactInfo --project SchedulIX
```

### 3. Review Migration File
The generated migration file will be in `Migrations/` folder. Review it to ensure it reflects your changes correctly.

### 4. Apply Migration
```bash
dotnet ef database update --project SchedulIX
```

---

## Seed Data Script

After the database is created, you can populate initial data. Create a seed service or run the following SQL directly:

### Room Types
```sql
USE schedulix_db;

INSERT INTO [TipuriSali] ([Denumire], [AreCalculatoare])
VALUES
  (N'Curs', 0),
  (N'Laborator', 1),
  (N'Seminar', 0),
  (N'Sala Multimedia', 1);
```

### Sample Rooms
```sql
INSERT INTO [Sali] ([NumarSala], [Capacitate], [TipSalaId], [EsteDisponibila])
VALUES
  (N'201', 50, 1, 1),
  (N'202', 40, 1, 1),
  (N'Lab 105', 25, 2, 1),
  (N'Lab 106', 30, 2, 1),
  (N'Seminar 301', 35, 3, 1);
```

### Room Availabilities (Example)
```sql
INSERT INTO [DisponibilitateSali] ([SalaId], [ZiuaSaptamanii], [OraInceput], [OraSfarsit])
VALUES
  (1, 1, '08:00:00', '18:00:00'),  -- Room 201 available Monday 8 AM - 6 PM
  (1, 2, '08:00:00', '18:00:00'),  -- Room 201 available Tuesday 8 AM - 6 PM
  (1, 3, '08:00:00', '18:00:00'),
  (1, 4, '08:00:00', '18:00:00'),
  (1, 5, '08:00:00', '18:00:00');
```

### Sample Disciplines
```sql
INSERT INTO [Discipline] ([Name], [CourseHours], [SeminarHours], [LabHours])
VALUES
  (N'Programare C#', 2, 2, 2),
  (N'Baze de Date', 2, 2, 2),
  (N'Arhitectură Software', 2, 0, 0),
  (N'Securitate Informații', 2, 2, 0);
```

### Sample Education Form
```sql
INSERT INTO [EducationForms] ([Name], [Year], [Semester])
VALUES
  (N'Full-Time', 1, 1),
  (N'Full-Time', 1, 2),
  (N'Part-Time', 1, 1);
```

### Sample Academic Groups
```sql
INSERT INTO [AcademicGroups] ([Name], [EducationFormId], [SeriesId])
VALUES
  (N'CR-211', 1, NULL),
  (N'CR-212', 1, NULL),
  (N'CR-213', 1, NULL);
```

---

## Connection String Examples

### SQL Server LocalDB
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=schedulix_db;Integrated Security=true;"
}
```

### SQL Server Express (Named Instance)
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=schedulix_db;Integrated Security=true;"
}
```

### Remote SQL Server
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server.database.windows.net,1433;Database=schedulix_db;User Id=sa;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;"
}
```

### PostgreSQL (when available)
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=schedulix_db;Username=postgres;Password=password;Port=5432"
}
```

---

## Troubleshooting

### Migration Not Found
```bash
dotnet ef migrations list --project SchedulIX
```

### Reset Database
⚠️ **WARNING: This will delete all data!**

```bash
dotnet ef database drop --project SchedulIX
dotnet ef database update --project SchedulIX
```

### Connection Issues
- Verify LocalDB is running: `sqllocaldb info` or start with `sqllocaldb start mssqllocaldb`
- Check SQL Server service is running
- Verify connection string in `appsettings.json`

### Entity Framework Issues
- Clear EF cache: `dotnet ef migrations remove` then recreate
- Clean and rebuild: `dotnet clean && dotnet build`

---

## Backup & Restore

### Backup Database
```bash
# Using SQL Server Management Studio
# Database > Tasks > Back Up...

# Or using T-SQL
BACKUP DATABASE [schedulix_db] TO DISK = 'C:\Backups\schedulix_db.bak';
```

### Restore Database
```bash
RESTORE DATABASE [schedulix_db] FROM DISK = 'C:\Backups\schedulix_db.bak';
```

---

## Next Steps

1. ✅ Run initial migration
2. ✅ Seed data (see SQL scripts above)
3. ✅ Test API endpoints with Swagger UI
4. ✅ Develop schedule generation algorithm
5. ✅ Implement authentication/authorization
6. ✅ Add advanced validation rules

---

## References

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Connection Strings](https://www.connectionstrings.com/)

