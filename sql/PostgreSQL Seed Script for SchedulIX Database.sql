-- Enable extension for UUIDs if needed in the future
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ==========================================
-- 1. DROP EXISTING TABLES (In Dependent Order)
-- ==========================================
DROP TABLE IF EXISTS "Schedules" CASCADE;
DROP TABLE IF EXISTS "Students" CASCADE;
DROP TABLE IF EXISTS "Subgroups" CASCADE;
DROP TABLE IF EXISTS "AcademicGroups" CASCADE;
DROP TABLE IF EXISTS "TeacherPreferences" CASCADE;
DROP TABLE IF EXISTS "RoomAvailabilities" CASCADE;
DROP TABLE IF EXISTS "Rooms" CASCADE;
DROP TABLE IF EXISTS "RoomTypes" CASCADE;
DROP TABLE IF EXISTS "Teachers" CASCADE;
DROP TABLE IF EXISTS "Disciplines" CASCADE;
DROP TABLE IF EXISTS "TimeSlots" CASCADE;
DROP TABLE IF EXISTS "Series" CASCADE;
DROP TABLE IF EXISTS "EducationForms" CASCADE;

-- ==========================================
-- 2. CREATE TABLES
-- ==========================================

-- 1. RoomTypes
CREATE TABLE "RoomTypes" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "HasComputers" BOOLEAN NOT NULL DEFAULT FALSE
);

-- 2. Rooms
CREATE TABLE "Rooms" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RoomNumber" VARCHAR(100) NOT NULL,
    "Capacity" INT NOT NULL DEFAULT 30,
    "RoomTypeId" INT NOT NULL,
    "IsAvailable" BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT "FK_Rooms_RoomTypes_RoomTypeId" 
        FOREIGN KEY ("RoomTypeId") REFERENCES "RoomTypes"("Id") ON DELETE RESTRICT
);

-- 3. RoomAvailabilities
CREATE TABLE "RoomAvailabilities" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RoomId" INT NOT NULL,
    "DayOfWeek" INT NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    CONSTRAINT "FK_RoomAvailabilities_Rooms_RoomId" 
        FOREIGN KEY ("RoomId") REFERENCES "Rooms"("Id") ON DELETE CASCADE
);

-- 4. Teachers
CREATE TABLE "Teachers" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(255) NOT NULL UNIQUE
);

-- 5. Disciplines
CREATE TABLE "Disciplines" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "CourseHours" INT NOT NULL DEFAULT 2,
    "SeminarHours" INT NOT NULL DEFAULT 2,
    "LabHours" INT NOT NULL DEFAULT 2
);

-- 6. TeacherPreferences
CREATE TABLE "TeacherPreferences" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "TeacherId" INT NOT NULL,
    "DisciplineId" INT NULL,
    "MandatoryRoomId" INT NULL,
    "PreferredStartTime" TIME NULL,
    "PreferredEndTime" TIME NULL,
    CONSTRAINT "FK_TeacherPreferences_Teachers_TeacherId" 
        FOREIGN KEY ("TeacherId") REFERENCES "Teachers"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TeacherPreferences_Disciplines_DisciplineId" 
        FOREIGN KEY ("DisciplineId") REFERENCES "Disciplines"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_TeacherPreferences_Rooms_MandatoryRoomId" 
        FOREIGN KEY ("MandatoryRoomId") REFERENCES "Rooms"("Id") ON DELETE SET NULL
);

-- 7. TimeSlots (Primary Key is SlotNumber)
CREATE TABLE "TimeSlots" (
    "SlotNumber" INT PRIMARY KEY,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    "IsLunchBreak" BOOLEAN NOT NULL DEFAULT FALSE
);

-- 8. EducationForms
CREATE TABLE "EducationForms" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL
);

-- 9. Series
CREATE TABLE "Series" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL
);

-- 10. AcademicGroups
CREATE TABLE "AcademicGroups" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "EducationFormId" INT NOT NULL,
    "SeriesId" INT NULL,
    CONSTRAINT "FK_AcademicGroups_EducationForms_EducationFormId" 
        FOREIGN KEY ("EducationFormId") REFERENCES "EducationForms"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AcademicGroups_Series_SeriesId" 
        FOREIGN KEY ("SeriesId") REFERENCES "Series"("Id") ON DELETE SET NULL
);

-- 11. Subgroups
CREATE TABLE "Subgroups" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "GroupId" INT NOT NULL,
    "StudentCount" INT NOT NULL DEFAULT 0,
    CONSTRAINT "FK_Subgroups_AcademicGroups_GroupId" 
        FOREIGN KEY ("GroupId") REFERENCES "AcademicGroups"("Id") ON DELETE CASCADE
);

-- 12. Students
CREATE TABLE "Students" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "GroupId" INT NOT NULL,
    "SubgroupId" INT NULL,
    CONSTRAINT "FK_Students_AcademicGroups_GroupId" 
        FOREIGN KEY ("GroupId") REFERENCES "AcademicGroups"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Students_Subgroups_SubgroupId" 
        FOREIGN KEY ("SubgroupId") REFERENCES "Subgroups"("Id") ON DELETE SET NULL
);

-- 13. Schedules
CREATE TABLE "Schedules" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "DisciplineId" INT NOT NULL,
    "TeacherId" INT NOT NULL,
    "RoomId" INT NOT NULL,
    "GroupId" INT NULL,
    "SubgroupId" INT NULL,
    "SeriesId" INT NULL,
    "DayOfWeek" INT NOT NULL,
    "TimeSlotNumber" INT NOT NULL,
    "WeekType" INT NOT NULL DEFAULT 0,
    "ClassType" VARCHAR(100) NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Schedules_Disciplines_DisciplineId" 
        FOREIGN KEY ("DisciplineId") REFERENCES "Disciplines"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Schedules_Teachers_TeacherId" 
        FOREIGN KEY ("TeacherId") REFERENCES "Teachers"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Schedules_Rooms_RoomId" 
        FOREIGN KEY ("RoomId") REFERENCES "Rooms"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Schedules_AcademicGroups_GroupId" 
        FOREIGN KEY ("GroupId") REFERENCES "AcademicGroups"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Schedules_Subgroups_SubgroupId" 
        FOREIGN KEY ("SubgroupId") REFERENCES "Subgroups"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Schedules_Series_SeriesId" 
        FOREIGN KEY ("SeriesId") REFERENCES "Series"("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Schedules_TimeSlots_TimeSlotNumber" 
        FOREIGN KEY ("TimeSlotNumber") REFERENCES "TimeSlots"("SlotNumber") ON DELETE RESTRICT
);

-- ==========================================
-- 3. INSERT SEED DATA
-- ==========================================

-- 1. TimeSlots
INSERT INTO "TimeSlots" ("SlotNumber", "StartTime", "EndTime", "IsLunchBreak") VALUES
(1, '08:00:00', '09:30:00', FALSE),
(2, '09:45:00', '11:15:00', FALSE),
(3, '11:30:00', '13:00:00', FALSE),
(4, '13:00:00', '14:30:00', TRUE),
(5, '14:30:00', '16:00:00', FALSE);

-- 2. RoomTypes
INSERT INTO "RoomTypes" ("Name", "HasComputers") VALUES
('Curs', FALSE),
('Laborator', TRUE),
('Seminar', FALSE);

-- 3. Rooms
INSERT INTO "Rooms" ("RoomNumber", "Capacity", "RoomTypeId", "IsAvailable") VALUES
('Auditorium A1', 120, 1, TRUE),
('Lab 105', 30, 2, TRUE),
('Sala 202', 45, 3, TRUE),
('Sala 301', 50, 3, FALSE);

-- 4. RoomAvailabilities
INSERT INTO "RoomAvailabilities" ("RoomId", "DayOfWeek", "StartTime", "EndTime") VALUES
(1, 1, '08:00:00', '16:00:00'),
(2, 1, '08:00:00', '14:00:00'),
(3, 2, '09:00:00', '15:00:00');

-- 5. Teachers
INSERT INTO "Teachers" ("FirstName", "LastName", "Email") VALUES
('Ion', 'Popescu', 'popescu@university.edu'),
('Maria', 'Ionescu', 'ionescu@university.edu');

-- 6. Disciplines
INSERT INTO "Disciplines" ("Name", "CourseHours", "SeminarHours", "LabHours") VALUES
('Programare C#', 2, 0, 2),
('Baze de Date', 2, 1, 2),
('Arhitectură Software', 2, 2, 0);

-- 7. TeacherPreferences
INSERT INTO "TeacherPreferences" ("TeacherId", "DisciplineId", "MandatoryRoomId", "PreferredStartTime", "PreferredEndTime") VALUES
(1, 1, 2, '08:00:00', '14:00:00'),
(2, 2, 1, '09:45:00', '16:00:00');

-- 8. EducationForms
INSERT INTO "EducationForms" ("Name") VALUES
('Zi'),
('Fără Frecvență');

-- 9. Series
INSERT INTO "Series" ("Name") VALUES
('Seria A - Anul 2'),
('Seria B - Anul 2');

-- 10. AcademicGroups
INSERT INTO "AcademicGroups" ("Name", "EducationFormId", "SeriesId") VALUES
('CR-211', 1, 1),
('CR-212', 1, 1),
('CR-221FR', 2, 2);

-- 11. Subgroups
INSERT INTO "Subgroups" ("Name", "GroupId", "StudentCount") VALUES
('CR-211-A', 1, 15),
('CR-211-B', 1, 15);

-- 12. Students
INSERT INTO "Students" ("FirstName", "LastName", "Email", "GroupId", "SubgroupId") VALUES
('Andrei', 'Vasile', 'andrei.vasile@student.university.edu', 1, 1),
('Elena', 'Radu', 'elena.radu@student.university.edu', 1, 2),
('Mihai', 'Stan', 'mihai.stan@student.university.edu', 2, NULL);

-- 13. Schedules
INSERT INTO "Schedules" ("DisciplineId", "TeacherId", "RoomId", "GroupId", "SubgroupId", "SeriesId", "DayOfWeek", "TimeSlotNumber", "WeekType", "ClassType") VALUES
(1, 1, 4, 1, 1, 1, 1, 1, 0, 'Lab'),
(2, 2, 2, 1, NULL, 1, 1, 2, 0, 'Curs'),
(3, 1, 3, 1, NULL, 1, 2, 3, 0, 'Seminar');