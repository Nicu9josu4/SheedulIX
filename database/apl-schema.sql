--
-- PostgreSQL database dump
--

\restrict Ocrucpvb3VM3iZ8zhmd4gkiNhUEuOLocRlhBTKep02UkxiVF47081Mh2ZAk7pkz

-- Dumped from database version 16.15
-- Dumped by pg_dump version 16.15

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: ApprovalSteps; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ApprovalSteps" (
    "Id" uuid NOT NULL,
    "StaffingPlanId" uuid NOT NULL,
    "FromStatus" character varying(30) NOT NULL,
    "ToStatus" character varying(30) NOT NULL,
    "ActorName" text NOT NULL,
    "Comment" text,
    "CreatedAt" timestamp with time zone NOT NULL
);


--
-- Name: AvailabilityBlocks; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AvailabilityBlocks" (
    "Id" uuid NOT NULL,
    "ProfessorProfileId" uuid NOT NULL,
    "AcademicYear" character varying(9) NOT NULL,
    "DayOfWeek" integer NOT NULL,
    "SlotNumber" integer NOT NULL,
    "State" character varying(20) NOT NULL,
    CONSTRAINT "CK_AvailabilityBlocks_Day" CHECK ((("DayOfWeek" >= 1) AND ("DayOfWeek" <= 6)))
);


--
-- Name: AvailabilityPreferences; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."AvailabilityPreferences" (
    "Id" uuid NOT NULL,
    "ProfessorProfileId" uuid NOT NULL,
    "AcademicYear" character varying(9) NOT NULL,
    "MaxTeachingDays" integer,
    "Notes" character varying(2000) NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);


--
-- Name: Contracts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Contracts" (
    "Id" uuid NOT NULL,
    "DepartmentEmploymentId" uuid NOT NULL,
    "Number" text NOT NULL,
    "Type" character varying(20) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone NOT NULL,
    "BaseEmployer" text,
    "AnnualHours" numeric(7,2) DEFAULT 0.0 NOT NULL
);


--
-- Name: ConversionCoefficients; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ConversionCoefficients" (
    "Id" uuid NOT NULL,
    "ActivityType" character varying(20) NOT NULL,
    "Coefficient" numeric(5,3) NOT NULL,
    "WorkloadModelId" uuid DEFAULT '00000000-0000-0000-0000-000000000000'::uuid NOT NULL
);


--
-- Name: CurriculumSubjects; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."CurriculumSubjects" (
    "Id" uuid NOT NULL,
    "CurriculumId" uuid NOT NULL,
    "SubjectId" uuid NOT NULL,
    "LanguageId" uuid NOT NULL,
    "YearOfStudy" integer NOT NULL,
    "Semester" integer NOT NULL,
    "LectureHours" integer NOT NULL,
    "SeminarHours" integer NOT NULL,
    "LabHours" integer NOT NULL
);


--
-- Name: Curriculums; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Curriculums" (
    "Id" uuid NOT NULL,
    "StudyProgramId" uuid NOT NULL,
    "Version" text NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT '-infinity'::timestamp with time zone NOT NULL
);


--
-- Name: DepartmentEmployments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."DepartmentEmployments" (
    "Id" uuid NOT NULL,
    "PersonId" uuid NOT NULL,
    "DepartmentId" uuid NOT NULL,
    "EmploymentType" integer NOT NULL,
    "Position" text NOT NULL,
    "TeachingLoad" numeric NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone,
    "Active" boolean NOT NULL
);


--
-- Name: Departments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Departments" (
    "Id" uuid NOT NULL,
    "Code" text NOT NULL,
    "Name" text NOT NULL,
    "Description" text NOT NULL,
    "Active" boolean NOT NULL
);


--
-- Name: EmploymentDocuments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."EmploymentDocuments" (
    "Id" uuid NOT NULL,
    "PersonId" uuid NOT NULL,
    "ContractId" uuid,
    "Type" character varying(30) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "FileName" text,
    "StoragePath" text,
    "UploadedAt" timestamp with time zone,
    "ValidatedAt" timestamp with time zone,
    "ValidatedBy" text
);


--
-- Name: ExternalCommitments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ExternalCommitments" (
    "Id" uuid NOT NULL,
    "ProfessorProfileId" uuid NOT NULL,
    "AcademicYear" character varying(9) NOT NULL,
    "DayOfWeek" integer NOT NULL,
    "StartTime" interval NOT NULL,
    "EndTime" interval NOT NULL,
    "Location" character varying(200) NOT NULL,
    CONSTRAINT "CK_ExternalCommitments_Day" CHECK ((("DayOfWeek" >= 1) AND ("DayOfWeek" <= 6)))
);


--
-- Name: Groups; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Groups" (
    "Id" uuid NOT NULL,
    "Code" text NOT NULL,
    "StudyProgramId" uuid NOT NULL,
    "CurriculumId" uuid NOT NULL,
    "LanguageId" uuid NOT NULL,
    "YearOfStudy" integer NOT NULL,
    "StudentCount" integer NOT NULL,
    "EntryYear" integer
);


--
-- Name: Languages; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Languages" (
    "Id" uuid NOT NULL,
    "Code" text NOT NULL,
    "Name" text NOT NULL
);


--
-- Name: Persons; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Persons" (
    "Id" uuid NOT NULL,
    "FirstName" text NOT NULL,
    "LastName" text NOT NULL,
    "Patronymic" text,
    "Email" text NOT NULL,
    "Phone" text NOT NULL,
    "AcademicTitle" text NOT NULL,
    "AcademicDegree" text NOT NULL,
    "Active" boolean NOT NULL
);


--
-- Name: PositionNorms; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."PositionNorms" (
    "Id" uuid NOT NULL,
    "Position" character varying(100) NOT NULL,
    "BaseNormHours" numeric(7,2) NOT NULL,
    "MinHours" numeric(7,2) NOT NULL,
    "MaxHours" numeric(7,2) NOT NULL,
    "WorkloadModelId" uuid DEFAULT '00000000-0000-0000-0000-000000000000'::uuid NOT NULL
);


--
-- Name: ProfessorLanguages; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ProfessorLanguages" (
    "ProfessorProfileId" uuid NOT NULL,
    "LanguageId" uuid NOT NULL
);


--
-- Name: ProfessorProfiles; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ProfessorProfiles" (
    "Id" uuid NOT NULL,
    "PersonId" uuid NOT NULL
);


--
-- Name: ProfessorSubjects; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ProfessorSubjects" (
    "ProfessorProfileId" uuid NOT NULL,
    "SubjectId" uuid NOT NULL
);


--
-- Name: Rooms; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Rooms" (
    "Id" uuid NOT NULL,
    "Code" character varying(50) NOT NULL,
    "Building" character varying(100) NOT NULL,
    "Capacity" integer NOT NULL,
    "Type" character varying(20) NOT NULL,
    "HasComputers" boolean NOT NULL,
    "Active" boolean NOT NULL,
    CONSTRAINT "CK_Rooms_Capacity" CHECK (("Capacity" > 0))
);


--
-- Name: StaffingPlans; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."StaffingPlans" (
    "Id" uuid NOT NULL,
    "DepartmentId" uuid NOT NULL,
    "AcademicYear" character varying(9) NOT NULL,
    "Status" character varying(30) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: StudyPrograms; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."StudyPrograms" (
    "Id" uuid NOT NULL,
    "Code" text NOT NULL,
    "Name" text NOT NULL,
    "DegreeLevel" text NOT NULL,
    "DurationYears" integer NOT NULL
);


--
-- Name: Subgroups; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Subgroups" (
    "Id" uuid NOT NULL,
    "GroupId" uuid NOT NULL,
    "Name" text NOT NULL,
    "StudentCount" integer NOT NULL
);


--
-- Name: Subjects; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Subjects" (
    "Id" uuid NOT NULL,
    "Code" text NOT NULL,
    "Name" text NOT NULL,
    "Description" text NOT NULL,
    "Credits" integer NOT NULL,
    "TotalHours" integer NOT NULL,
    "Active" boolean NOT NULL
);


--
-- Name: TeachingAssignments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."TeachingAssignments" (
    "Id" uuid NOT NULL,
    "ProfessorProfileId" uuid NOT NULL,
    "SubjectId" uuid NOT NULL,
    "GroupId" uuid NOT NULL,
    "SubgroupId" uuid,
    "LanguageId" uuid NOT NULL,
    "ActivityType" integer NOT NULL,
    "Hours" integer NOT NULL,
    "Semester" integer NOT NULL,
    "AcademicYear" character varying(9) NOT NULL,
    "Status" integer NOT NULL,
    CONSTRAINT "CK_TeachingAssignments_Hours" CHECK (("Hours" > 0)),
    CONSTRAINT "CK_TeachingAssignments_Semester" CHECK (("Semester" = ANY (ARRAY[1, 2])))
);


--
-- Name: TimeSlots; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."TimeSlots" (
    "SlotNumber" integer NOT NULL,
    "StartTime" interval NOT NULL,
    "EndTime" interval NOT NULL,
    "IsLunchBreak" boolean NOT NULL
);


--
-- Name: TimetableEntries; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."TimetableEntries" (
    "Id" uuid NOT NULL,
    "TimetableRunId" uuid NOT NULL,
    "TeachingAssignmentId" uuid NOT NULL,
    "DayOfWeek" integer NOT NULL,
    "SlotNumber" integer NOT NULL,
    "RoomId" uuid NOT NULL,
    "Parity" character varying(10) NOT NULL
);


--
-- Name: TimetableRuns; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."TimetableRuns" (
    "Id" uuid NOT NULL,
    "AcademicYear" character varying(9) NOT NULL,
    "Semester" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "SolverStatus" character varying(30) NOT NULL,
    "SessionsTotal" integer NOT NULL,
    "SessionsPlaced" integer NOT NULL,
    "Penalty" integer NOT NULL,
    "TimeLimitSeconds" integer NOT NULL,
    "SolveSeconds" double precision NOT NULL
);


--
-- Name: TimetableUnplaced; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."TimetableUnplaced" (
    "Id" uuid NOT NULL,
    "TimetableRunId" uuid NOT NULL,
    "TeachingAssignmentId" uuid NOT NULL,
    "Sessions" integer NOT NULL,
    "Reason" character varying(300) NOT NULL
);


--
-- Name: WorkloadModels; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."WorkloadModels" (
    "Id" uuid NOT NULL,
    "AcademicYear" character varying(9) NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Name: ApprovalSteps PK_ApprovalSteps; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ApprovalSteps"
    ADD CONSTRAINT "PK_ApprovalSteps" PRIMARY KEY ("Id");


--
-- Name: AvailabilityBlocks PK_AvailabilityBlocks; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AvailabilityBlocks"
    ADD CONSTRAINT "PK_AvailabilityBlocks" PRIMARY KEY ("Id");


--
-- Name: AvailabilityPreferences PK_AvailabilityPreferences; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AvailabilityPreferences"
    ADD CONSTRAINT "PK_AvailabilityPreferences" PRIMARY KEY ("Id");


--
-- Name: Contracts PK_Contracts; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Contracts"
    ADD CONSTRAINT "PK_Contracts" PRIMARY KEY ("Id");


--
-- Name: ConversionCoefficients PK_ConversionCoefficients; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ConversionCoefficients"
    ADD CONSTRAINT "PK_ConversionCoefficients" PRIMARY KEY ("Id");


--
-- Name: CurriculumSubjects PK_CurriculumSubjects; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CurriculumSubjects"
    ADD CONSTRAINT "PK_CurriculumSubjects" PRIMARY KEY ("Id");


--
-- Name: Curriculums PK_Curriculums; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Curriculums"
    ADD CONSTRAINT "PK_Curriculums" PRIMARY KEY ("Id");


--
-- Name: DepartmentEmployments PK_DepartmentEmployments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."DepartmentEmployments"
    ADD CONSTRAINT "PK_DepartmentEmployments" PRIMARY KEY ("Id");


--
-- Name: Departments PK_Departments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Departments"
    ADD CONSTRAINT "PK_Departments" PRIMARY KEY ("Id");


--
-- Name: EmploymentDocuments PK_EmploymentDocuments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."EmploymentDocuments"
    ADD CONSTRAINT "PK_EmploymentDocuments" PRIMARY KEY ("Id");


--
-- Name: ExternalCommitments PK_ExternalCommitments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ExternalCommitments"
    ADD CONSTRAINT "PK_ExternalCommitments" PRIMARY KEY ("Id");


--
-- Name: Groups PK_Groups; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Groups"
    ADD CONSTRAINT "PK_Groups" PRIMARY KEY ("Id");


--
-- Name: Languages PK_Languages; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Languages"
    ADD CONSTRAINT "PK_Languages" PRIMARY KEY ("Id");


--
-- Name: Persons PK_Persons; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Persons"
    ADD CONSTRAINT "PK_Persons" PRIMARY KEY ("Id");


--
-- Name: PositionNorms PK_PositionNorms; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PositionNorms"
    ADD CONSTRAINT "PK_PositionNorms" PRIMARY KEY ("Id");


--
-- Name: ProfessorLanguages PK_ProfessorLanguages; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProfessorLanguages"
    ADD CONSTRAINT "PK_ProfessorLanguages" PRIMARY KEY ("ProfessorProfileId", "LanguageId");


--
-- Name: ProfessorProfiles PK_ProfessorProfiles; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProfessorProfiles"
    ADD CONSTRAINT "PK_ProfessorProfiles" PRIMARY KEY ("Id");


--
-- Name: ProfessorSubjects PK_ProfessorSubjects; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProfessorSubjects"
    ADD CONSTRAINT "PK_ProfessorSubjects" PRIMARY KEY ("ProfessorProfileId", "SubjectId");


--
-- Name: Rooms PK_Rooms; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Rooms"
    ADD CONSTRAINT "PK_Rooms" PRIMARY KEY ("Id");


--
-- Name: StaffingPlans PK_StaffingPlans; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."StaffingPlans"
    ADD CONSTRAINT "PK_StaffingPlans" PRIMARY KEY ("Id");


--
-- Name: StudyPrograms PK_StudyPrograms; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."StudyPrograms"
    ADD CONSTRAINT "PK_StudyPrograms" PRIMARY KEY ("Id");


--
-- Name: Subgroups PK_Subgroups; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Subgroups"
    ADD CONSTRAINT "PK_Subgroups" PRIMARY KEY ("Id");


--
-- Name: Subjects PK_Subjects; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Subjects"
    ADD CONSTRAINT "PK_Subjects" PRIMARY KEY ("Id");


--
-- Name: TeachingAssignments PK_TeachingAssignments; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TeachingAssignments"
    ADD CONSTRAINT "PK_TeachingAssignments" PRIMARY KEY ("Id");


--
-- Name: TimeSlots PK_TimeSlots; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimeSlots"
    ADD CONSTRAINT "PK_TimeSlots" PRIMARY KEY ("SlotNumber");


--
-- Name: TimetableEntries PK_TimetableEntries; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimetableEntries"
    ADD CONSTRAINT "PK_TimetableEntries" PRIMARY KEY ("Id");


--
-- Name: TimetableRuns PK_TimetableRuns; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimetableRuns"
    ADD CONSTRAINT "PK_TimetableRuns" PRIMARY KEY ("Id");


--
-- Name: TimetableUnplaced PK_TimetableUnplaced; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimetableUnplaced"
    ADD CONSTRAINT "PK_TimetableUnplaced" PRIMARY KEY ("Id");


--
-- Name: WorkloadModels PK_WorkloadModels; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."WorkloadModels"
    ADD CONSTRAINT "PK_WorkloadModels" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_ApprovalSteps_StaffingPlanId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ApprovalSteps_StaffingPlanId" ON public."ApprovalSteps" USING btree ("StaffingPlanId");


--
-- Name: IX_AvailabilityBlocks_ProfessorProfileId_AcademicYear_DayOfWee~; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_AvailabilityBlocks_ProfessorProfileId_AcademicYear_DayOfWee~" ON public."AvailabilityBlocks" USING btree ("ProfessorProfileId", "AcademicYear", "DayOfWeek", "SlotNumber");


--
-- Name: IX_AvailabilityPreferences_ProfessorProfileId_AcademicYear; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_AvailabilityPreferences_ProfessorProfileId_AcademicYear" ON public."AvailabilityPreferences" USING btree ("ProfessorProfileId", "AcademicYear");


--
-- Name: IX_Contracts_DepartmentEmploymentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Contracts_DepartmentEmploymentId" ON public."Contracts" USING btree ("DepartmentEmploymentId");


--
-- Name: IX_Contracts_EndDate; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Contracts_EndDate" ON public."Contracts" USING btree ("EndDate");


--
-- Name: IX_Contracts_Number; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Contracts_Number" ON public."Contracts" USING btree ("Number");


--
-- Name: IX_ConversionCoefficients_WorkloadModelId_ActivityType; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_ConversionCoefficients_WorkloadModelId_ActivityType" ON public."ConversionCoefficients" USING btree ("WorkloadModelId", "ActivityType");


--
-- Name: IX_CurriculumSubjects_CurriculumId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CurriculumSubjects_CurriculumId" ON public."CurriculumSubjects" USING btree ("CurriculumId");


--
-- Name: IX_CurriculumSubjects_LanguageId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CurriculumSubjects_LanguageId" ON public."CurriculumSubjects" USING btree ("LanguageId");


--
-- Name: IX_CurriculumSubjects_SubjectId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_CurriculumSubjects_SubjectId" ON public."CurriculumSubjects" USING btree ("SubjectId");


--
-- Name: IX_Curriculums_StudyProgramId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Curriculums_StudyProgramId" ON public."Curriculums" USING btree ("StudyProgramId");


--
-- Name: IX_DepartmentEmployments_DepartmentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_DepartmentEmployments_DepartmentId" ON public."DepartmentEmployments" USING btree ("DepartmentId");


--
-- Name: IX_DepartmentEmployments_PersonId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_DepartmentEmployments_PersonId" ON public."DepartmentEmployments" USING btree ("PersonId");


--
-- Name: IX_EmploymentDocuments_ContractId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_EmploymentDocuments_ContractId" ON public."EmploymentDocuments" USING btree ("ContractId");


--
-- Name: IX_EmploymentDocuments_PersonId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_EmploymentDocuments_PersonId" ON public."EmploymentDocuments" USING btree ("PersonId");


--
-- Name: IX_ExternalCommitments_ProfessorProfileId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ExternalCommitments_ProfessorProfileId" ON public."ExternalCommitments" USING btree ("ProfessorProfileId");


--
-- Name: IX_Groups_CurriculumId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Groups_CurriculumId" ON public."Groups" USING btree ("CurriculumId");


--
-- Name: IX_Groups_LanguageId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Groups_LanguageId" ON public."Groups" USING btree ("LanguageId");


--
-- Name: IX_Groups_StudyProgramId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Groups_StudyProgramId" ON public."Groups" USING btree ("StudyProgramId");


--
-- Name: IX_Languages_Code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Languages_Code" ON public."Languages" USING btree ("Code");


--
-- Name: IX_PositionNorms_WorkloadModelId_Position; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_PositionNorms_WorkloadModelId_Position" ON public."PositionNorms" USING btree ("WorkloadModelId", "Position");


--
-- Name: IX_ProfessorLanguages_LanguageId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ProfessorLanguages_LanguageId" ON public."ProfessorLanguages" USING btree ("LanguageId");


--
-- Name: IX_ProfessorProfiles_PersonId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_ProfessorProfiles_PersonId" ON public."ProfessorProfiles" USING btree ("PersonId");


--
-- Name: IX_ProfessorSubjects_SubjectId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ProfessorSubjects_SubjectId" ON public."ProfessorSubjects" USING btree ("SubjectId");


--
-- Name: IX_Rooms_Building_Code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Rooms_Building_Code" ON public."Rooms" USING btree ("Building", "Code");


--
-- Name: IX_StaffingPlans_DepartmentId_AcademicYear; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_StaffingPlans_DepartmentId_AcademicYear" ON public."StaffingPlans" USING btree ("DepartmentId", "AcademicYear");


--
-- Name: IX_Subgroups_GroupId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Subgroups_GroupId" ON public."Subgroups" USING btree ("GroupId");


--
-- Name: IX_TeachingAssignments_GroupId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TeachingAssignments_GroupId" ON public."TeachingAssignments" USING btree ("GroupId");


--
-- Name: IX_TeachingAssignments_LanguageId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TeachingAssignments_LanguageId" ON public."TeachingAssignments" USING btree ("LanguageId");


--
-- Name: IX_TeachingAssignments_ProfessorProfileId_AcademicYear_Semester; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TeachingAssignments_ProfessorProfileId_AcademicYear_Semester" ON public."TeachingAssignments" USING btree ("ProfessorProfileId", "AcademicYear", "Semester");


--
-- Name: IX_TeachingAssignments_SubgroupId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TeachingAssignments_SubgroupId" ON public."TeachingAssignments" USING btree ("SubgroupId");


--
-- Name: IX_TeachingAssignments_SubjectId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TeachingAssignments_SubjectId" ON public."TeachingAssignments" USING btree ("SubjectId");


--
-- Name: IX_TimetableEntries_RoomId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TimetableEntries_RoomId" ON public."TimetableEntries" USING btree ("RoomId");


--
-- Name: IX_TimetableEntries_TeachingAssignmentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TimetableEntries_TeachingAssignmentId" ON public."TimetableEntries" USING btree ("TeachingAssignmentId");


--
-- Name: IX_TimetableEntries_TimetableRunId_DayOfWeek_SlotNumber; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TimetableEntries_TimetableRunId_DayOfWeek_SlotNumber" ON public."TimetableEntries" USING btree ("TimetableRunId", "DayOfWeek", "SlotNumber");


--
-- Name: IX_TimetableRuns_AcademicYear_Semester; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TimetableRuns_AcademicYear_Semester" ON public."TimetableRuns" USING btree ("AcademicYear", "Semester");


--
-- Name: IX_TimetableUnplaced_TeachingAssignmentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TimetableUnplaced_TeachingAssignmentId" ON public."TimetableUnplaced" USING btree ("TeachingAssignmentId");


--
-- Name: IX_TimetableUnplaced_TimetableRunId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_TimetableUnplaced_TimetableRunId" ON public."TimetableUnplaced" USING btree ("TimetableRunId");


--
-- Name: IX_WorkloadModels_AcademicYear; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_WorkloadModels_AcademicYear" ON public."WorkloadModels" USING btree ("AcademicYear");


--
-- Name: ApprovalSteps FK_ApprovalSteps_StaffingPlans_StaffingPlanId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ApprovalSteps"
    ADD CONSTRAINT "FK_ApprovalSteps_StaffingPlans_StaffingPlanId" FOREIGN KEY ("StaffingPlanId") REFERENCES public."StaffingPlans"("Id") ON DELETE CASCADE;


--
-- Name: AvailabilityBlocks FK_AvailabilityBlocks_ProfessorProfiles_ProfessorProfileId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AvailabilityBlocks"
    ADD CONSTRAINT "FK_AvailabilityBlocks_ProfessorProfiles_ProfessorProfileId" FOREIGN KEY ("ProfessorProfileId") REFERENCES public."ProfessorProfiles"("Id") ON DELETE CASCADE;


--
-- Name: AvailabilityPreferences FK_AvailabilityPreferences_ProfessorProfiles_ProfessorProfileId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."AvailabilityPreferences"
    ADD CONSTRAINT "FK_AvailabilityPreferences_ProfessorProfiles_ProfessorProfileId" FOREIGN KEY ("ProfessorProfileId") REFERENCES public."ProfessorProfiles"("Id") ON DELETE CASCADE;


--
-- Name: Contracts FK_Contracts_DepartmentEmployments_DepartmentEmploymentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Contracts"
    ADD CONSTRAINT "FK_Contracts_DepartmentEmployments_DepartmentEmploymentId" FOREIGN KEY ("DepartmentEmploymentId") REFERENCES public."DepartmentEmployments"("Id") ON DELETE CASCADE;


--
-- Name: ConversionCoefficients FK_ConversionCoefficients_WorkloadModels_WorkloadModelId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ConversionCoefficients"
    ADD CONSTRAINT "FK_ConversionCoefficients_WorkloadModels_WorkloadModelId" FOREIGN KEY ("WorkloadModelId") REFERENCES public."WorkloadModels"("Id") ON DELETE CASCADE;


--
-- Name: CurriculumSubjects FK_CurriculumSubjects_Curriculums_CurriculumId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CurriculumSubjects"
    ADD CONSTRAINT "FK_CurriculumSubjects_Curriculums_CurriculumId" FOREIGN KEY ("CurriculumId") REFERENCES public."Curriculums"("Id") ON DELETE CASCADE;


--
-- Name: CurriculumSubjects FK_CurriculumSubjects_Languages_LanguageId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CurriculumSubjects"
    ADD CONSTRAINT "FK_CurriculumSubjects_Languages_LanguageId" FOREIGN KEY ("LanguageId") REFERENCES public."Languages"("Id") ON DELETE CASCADE;


--
-- Name: CurriculumSubjects FK_CurriculumSubjects_Subjects_SubjectId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."CurriculumSubjects"
    ADD CONSTRAINT "FK_CurriculumSubjects_Subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES public."Subjects"("Id") ON DELETE CASCADE;


--
-- Name: Curriculums FK_Curriculums_StudyPrograms_StudyProgramId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Curriculums"
    ADD CONSTRAINT "FK_Curriculums_StudyPrograms_StudyProgramId" FOREIGN KEY ("StudyProgramId") REFERENCES public."StudyPrograms"("Id") ON DELETE CASCADE;


--
-- Name: DepartmentEmployments FK_DepartmentEmployments_Departments_DepartmentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."DepartmentEmployments"
    ADD CONSTRAINT "FK_DepartmentEmployments_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES public."Departments"("Id") ON DELETE CASCADE;


--
-- Name: DepartmentEmployments FK_DepartmentEmployments_Persons_PersonId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."DepartmentEmployments"
    ADD CONSTRAINT "FK_DepartmentEmployments_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES public."Persons"("Id") ON DELETE CASCADE;


--
-- Name: EmploymentDocuments FK_EmploymentDocuments_Contracts_ContractId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."EmploymentDocuments"
    ADD CONSTRAINT "FK_EmploymentDocuments_Contracts_ContractId" FOREIGN KEY ("ContractId") REFERENCES public."Contracts"("Id") ON DELETE SET NULL;


--
-- Name: EmploymentDocuments FK_EmploymentDocuments_Persons_PersonId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."EmploymentDocuments"
    ADD CONSTRAINT "FK_EmploymentDocuments_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES public."Persons"("Id") ON DELETE CASCADE;


--
-- Name: ExternalCommitments FK_ExternalCommitments_ProfessorProfiles_ProfessorProfileId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ExternalCommitments"
    ADD CONSTRAINT "FK_ExternalCommitments_ProfessorProfiles_ProfessorProfileId" FOREIGN KEY ("ProfessorProfileId") REFERENCES public."ProfessorProfiles"("Id") ON DELETE CASCADE;


--
-- Name: Groups FK_Groups_Curriculums_CurriculumId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Groups"
    ADD CONSTRAINT "FK_Groups_Curriculums_CurriculumId" FOREIGN KEY ("CurriculumId") REFERENCES public."Curriculums"("Id") ON DELETE CASCADE;


--
-- Name: Groups FK_Groups_Languages_LanguageId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Groups"
    ADD CONSTRAINT "FK_Groups_Languages_LanguageId" FOREIGN KEY ("LanguageId") REFERENCES public."Languages"("Id") ON DELETE CASCADE;


--
-- Name: Groups FK_Groups_StudyPrograms_StudyProgramId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Groups"
    ADD CONSTRAINT "FK_Groups_StudyPrograms_StudyProgramId" FOREIGN KEY ("StudyProgramId") REFERENCES public."StudyPrograms"("Id") ON DELETE CASCADE;


--
-- Name: PositionNorms FK_PositionNorms_WorkloadModels_WorkloadModelId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."PositionNorms"
    ADD CONSTRAINT "FK_PositionNorms_WorkloadModels_WorkloadModelId" FOREIGN KEY ("WorkloadModelId") REFERENCES public."WorkloadModels"("Id") ON DELETE CASCADE;


--
-- Name: ProfessorLanguages FK_ProfessorLanguages_Languages_LanguageId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProfessorLanguages"
    ADD CONSTRAINT "FK_ProfessorLanguages_Languages_LanguageId" FOREIGN KEY ("LanguageId") REFERENCES public."Languages"("Id") ON DELETE CASCADE;


--
-- Name: ProfessorLanguages FK_ProfessorLanguages_ProfessorProfiles_ProfessorProfileId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProfessorLanguages"
    ADD CONSTRAINT "FK_ProfessorLanguages_ProfessorProfiles_ProfessorProfileId" FOREIGN KEY ("ProfessorProfileId") REFERENCES public."ProfessorProfiles"("Id") ON DELETE CASCADE;


--
-- Name: ProfessorProfiles FK_ProfessorProfiles_Persons_PersonId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProfessorProfiles"
    ADD CONSTRAINT "FK_ProfessorProfiles_Persons_PersonId" FOREIGN KEY ("PersonId") REFERENCES public."Persons"("Id") ON DELETE CASCADE;


--
-- Name: ProfessorSubjects FK_ProfessorSubjects_ProfessorProfiles_ProfessorProfileId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProfessorSubjects"
    ADD CONSTRAINT "FK_ProfessorSubjects_ProfessorProfiles_ProfessorProfileId" FOREIGN KEY ("ProfessorProfileId") REFERENCES public."ProfessorProfiles"("Id") ON DELETE CASCADE;


--
-- Name: ProfessorSubjects FK_ProfessorSubjects_Subjects_SubjectId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ProfessorSubjects"
    ADD CONSTRAINT "FK_ProfessorSubjects_Subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES public."Subjects"("Id") ON DELETE CASCADE;


--
-- Name: StaffingPlans FK_StaffingPlans_Departments_DepartmentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."StaffingPlans"
    ADD CONSTRAINT "FK_StaffingPlans_Departments_DepartmentId" FOREIGN KEY ("DepartmentId") REFERENCES public."Departments"("Id") ON DELETE RESTRICT;


--
-- Name: Subgroups FK_Subgroups_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Subgroups"
    ADD CONSTRAINT "FK_Subgroups_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- Name: TeachingAssignments FK_TeachingAssignments_Groups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TeachingAssignments"
    ADD CONSTRAINT "FK_TeachingAssignments_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."Groups"("Id") ON DELETE CASCADE;


--
-- Name: TeachingAssignments FK_TeachingAssignments_Languages_LanguageId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TeachingAssignments"
    ADD CONSTRAINT "FK_TeachingAssignments_Languages_LanguageId" FOREIGN KEY ("LanguageId") REFERENCES public."Languages"("Id") ON DELETE CASCADE;


--
-- Name: TeachingAssignments FK_TeachingAssignments_ProfessorProfiles_ProfessorProfileId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TeachingAssignments"
    ADD CONSTRAINT "FK_TeachingAssignments_ProfessorProfiles_ProfessorProfileId" FOREIGN KEY ("ProfessorProfileId") REFERENCES public."ProfessorProfiles"("Id") ON DELETE CASCADE;


--
-- Name: TeachingAssignments FK_TeachingAssignments_Subgroups_SubgroupId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TeachingAssignments"
    ADD CONSTRAINT "FK_TeachingAssignments_Subgroups_SubgroupId" FOREIGN KEY ("SubgroupId") REFERENCES public."Subgroups"("Id");


--
-- Name: TeachingAssignments FK_TeachingAssignments_Subjects_SubjectId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TeachingAssignments"
    ADD CONSTRAINT "FK_TeachingAssignments_Subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES public."Subjects"("Id") ON DELETE CASCADE;


--
-- Name: TimetableEntries FK_TimetableEntries_Rooms_RoomId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimetableEntries"
    ADD CONSTRAINT "FK_TimetableEntries_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES public."Rooms"("Id") ON DELETE RESTRICT;


--
-- Name: TimetableEntries FK_TimetableEntries_TeachingAssignments_TeachingAssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimetableEntries"
    ADD CONSTRAINT "FK_TimetableEntries_TeachingAssignments_TeachingAssignmentId" FOREIGN KEY ("TeachingAssignmentId") REFERENCES public."TeachingAssignments"("Id") ON DELETE CASCADE;


--
-- Name: TimetableEntries FK_TimetableEntries_TimetableRuns_TimetableRunId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimetableEntries"
    ADD CONSTRAINT "FK_TimetableEntries_TimetableRuns_TimetableRunId" FOREIGN KEY ("TimetableRunId") REFERENCES public."TimetableRuns"("Id") ON DELETE CASCADE;


--
-- Name: TimetableUnplaced FK_TimetableUnplaced_TeachingAssignments_TeachingAssignmentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimetableUnplaced"
    ADD CONSTRAINT "FK_TimetableUnplaced_TeachingAssignments_TeachingAssignmentId" FOREIGN KEY ("TeachingAssignmentId") REFERENCES public."TeachingAssignments"("Id") ON DELETE CASCADE;


--
-- Name: TimetableUnplaced FK_TimetableUnplaced_TimetableRuns_TimetableRunId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."TimetableUnplaced"
    ADD CONSTRAINT "FK_TimetableUnplaced_TimetableRuns_TimetableRunId" FOREIGN KEY ("TimetableRunId") REFERENCES public."TimetableRuns"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict Ocrucpvb3VM3iZ8zhmd4gkiNhUEuOLocRlhBTKep02UkxiVF47081Mh2ZAk7pkz

