using Microsoft.EntityFrameworkCore;
using SchedulIX.Models.Entities;

namespace SchedulIX.Data
{
    public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
    {
        public DbSet<RoomType> RoomTypes { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<RoomAvailability> RoomAvailabilities { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<TeacherPreference> TeacherPreferences { get; set; } = null!;
        public DbSet<Discipline> Disciplines { get; set; } = null!;
        public DbSet<TimeSlot> TimeSlots { get; set; } = null!;
        public DbSet<AcademicGroup> AcademicGroups { get; set; } = null!;
        public DbSet<Subgroup> Subgroups { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Series> Series { get; set; } = null!;
        public DbSet<EducationForm> EducationForms { get; set; } = null!;
        public DbSet<Schedule> Schedules { get; set; } = null!;

        // Sarcina didactica
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<AcademicPosition> AcademicPositions { get; set; } = null!;
        public DbSet<PositionNorm> PositionNorms { get; set; } = null!;
        public DbSet<ConversionCoefficient> ConversionCoefficients { get; set; } = null!;
        public DbSet<TeachingAssignment> TeachingAssignments { get; set; } = null!;
        public DbSet<StaffingPlan> StaffingPlans { get; set; } = null!;
        public DbSet<ApprovalStep> ApprovalSteps { get; set; } = null!;
        public DbSet<Contract> Contracts { get; set; } = null!;
        public DbSet<TeacherDocument> TeacherDocuments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys
            modelBuilder.Entity<TimeSlot>()
                .HasKey(x => x.SlotNumber);

            // Configure relationships
            modelBuilder.Entity<Room>()
                .HasOne(r => r.RoomType)
                .WithMany(rt => rt.Rooms)
                .HasForeignKey(r => r.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoomAvailability>()
                .HasOne(ra => ra.Room)
                .WithMany(r => r.Availabilities)
                .HasForeignKey(ra => ra.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherPreference>()
                .HasOne(tp => tp.Teacher)
                .WithMany(t => t.Preferences)
                .HasForeignKey(tp => tp.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherPreference>()
                .HasOne(tp => tp.Discipline)
                .WithMany(d => d.TeacherPreferences)
                .HasForeignKey(tp => tp.DisciplineId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TeacherPreference>()
                .HasOne(tp => tp.MandatoryRoom)
                .WithMany(r => r.TeacherPreferences)
                .HasForeignKey(tp => tp.MandatoryRoomId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AcademicGroup>()
                .HasOne(ag => ag.EducationForm)
                .WithMany(ef => ef.Groups)
                .HasForeignKey(ag => ag.EducationFormId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AcademicGroup>()
                .HasOne(ag => ag.Series)
                .WithMany(s => s.Groups)
                .HasForeignKey(ag => ag.SeriesId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Subgroup>()
                .HasOne(s => s.Group)
                .WithMany(ag => ag.Subgroups)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Students table relations
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Group)
                .WithMany(ag => ag.Students)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Subgroup)
                .WithMany(sg => sg.Students)
                .HasForeignKey(s => s.SubgroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Schedule>()
                .HasOne(sc => sc.Discipline)
                .WithMany(d => d.Schedules)
                .HasForeignKey(sc => sc.DisciplineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(sc => sc.Teacher)
                .WithMany(t => t.Schedules)
                .HasForeignKey(sc => sc.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(sc => sc.Room)
                .WithMany(r => r.Schedules)
                .HasForeignKey(sc => sc.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Schedule>()
                .HasOne(sc => sc.Group)
                .WithMany(ag => ag.Schedules)
                .HasForeignKey(sc => sc.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Schedule>()
                .HasOne(sc => sc.Subgroup)
                .WithMany(s => s.Schedules)
                .HasForeignKey(sc => sc.SubgroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Schedule>()
                .HasOne(sc => sc.Series)
                .WithMany()
                .HasForeignKey(sc => sc.SeriesId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Schedule>()
                .HasOne(sc => sc.TimeSlot)
                .WithMany(ts => ts.Schedules)
                .HasForeignKey(sc => sc.TimeSlotNumber)
                .HasPrincipalKey(ts => ts.SlotNumber)
                .OnDelete(DeleteBehavior.Restrict);

            ConfigureWorkload(modelBuilder);

            // Seed default timeslots
            SeedTimeSlots(modelBuilder);
        }

        private static void ConfigureWorkload(ModelBuilder mb)
        {
            mb.Entity<Department>().HasIndex(d => d.Code).IsUnique();
            mb.Entity<AcademicPosition>().HasIndex(p => p.Code).IsUnique();

            mb.Entity<Teacher>()
                .HasOne(t => t.Department).WithMany(d => d.Teachers)
                .HasForeignKey(t => t.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<Teacher>()
                .HasOne(t => t.AcademicPosition).WithMany(p => p.Teachers)
                .HasForeignKey(t => t.AcademicPositionId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<Teacher>().Property(t => t.EmploymentType).HasConversion<string>().HasMaxLength(30);

            mb.Entity<PositionNorm>()
                .HasOne(n => n.AcademicPosition).WithMany(p => p.Norms)
                .HasForeignKey(n => n.AcademicPositionId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<PositionNorm>()
                .HasIndex(n => new { n.AcademicPositionId, n.AcademicYear }).IsUnique();
            mb.Entity<PositionNorm>().Property(n => n.AcademicYear).HasMaxLength(9);
            mb.Entity<PositionNorm>().Property(n => n.BaseNormHours).HasPrecision(5, 2);
            mb.Entity<PositionNorm>().Property(n => n.MinHours).HasPrecision(5, 2);
            mb.Entity<PositionNorm>().Property(n => n.MaxHours).HasPrecision(5, 2);

            mb.Entity<ConversionCoefficient>()
                .HasOne(c => c.EducationForm).WithMany()
                .HasForeignKey(c => c.EducationFormId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ConversionCoefficient>().Property(c => c.AcademicYear).HasMaxLength(9);
            mb.Entity<ConversionCoefficient>().Property(c => c.ActivityType).HasConversion<string>().HasMaxLength(20);
            mb.Entity<ConversionCoefficient>().Property(c => c.Coefficient).HasPrecision(5, 3);
            mb.Entity<ConversionCoefficient>()
                .HasIndex(c => new { c.AcademicYear, c.ActivityType, c.EducationFormId }).IsUnique();

            mb.Entity<StaffingPlan>()
                .HasOne(p => p.Department).WithMany(d => d.StaffingPlans)
                .HasForeignKey(p => p.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<StaffingPlan>().Property(p => p.AcademicYear).HasMaxLength(9);
            mb.Entity<StaffingPlan>().Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
            mb.Entity<StaffingPlan>()
                .HasIndex(p => new { p.DepartmentId, p.AcademicYear }).IsUnique();

            mb.Entity<ApprovalStep>()
                .HasOne(s => s.StaffingPlan).WithMany(p => p.ApprovalSteps)
                .HasForeignKey(s => s.StaffingPlanId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ApprovalStep>().Property(s => s.FromStatus).HasConversion<string>().HasMaxLength(30);
            mb.Entity<ApprovalStep>().Property(s => s.ToStatus).HasConversion<string>().HasMaxLength(30);

            mb.Entity<TeachingAssignment>()
                .HasOne(a => a.Teacher).WithMany(t => t.Assignments)
                .HasForeignKey(a => a.TeacherId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TeachingAssignment>()
                .HasOne(a => a.Discipline).WithMany()
                .HasForeignKey(a => a.DisciplineId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TeachingAssignment>()
                .HasOne(a => a.StaffingPlan).WithMany(p => p.Assignments)
                .HasForeignKey(a => a.StaffingPlanId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<TeachingAssignment>()
                .HasOne(a => a.Series).WithMany()
                .HasForeignKey(a => a.SeriesId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<TeachingAssignment>()
                .HasOne(a => a.Group).WithMany()
                .HasForeignKey(a => a.GroupId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<TeachingAssignment>()
                .HasOne(a => a.Subgroup).WithMany()
                .HasForeignKey(a => a.SubgroupId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<TeachingAssignment>().Property(a => a.AcademicYear).HasMaxLength(9);
            mb.Entity<TeachingAssignment>().Property(a => a.ActivityType).HasConversion<string>().HasMaxLength(20);
            mb.Entity<TeachingAssignment>().Property(a => a.WeeklyHours).HasPrecision(5, 2);
            mb.Entity<TeachingAssignment>()
                .HasIndex(a => new { a.TeacherId, a.AcademicYear, a.Semester });
            mb.Entity<TeachingAssignment>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_TeachingAssignment_SingleFormation",
                    "((\"SeriesId\" IS NOT NULL)::int + (\"GroupId\" IS NOT NULL)::int + (\"SubgroupId\" IS NOT NULL)::int) <= 1");
                t.HasCheckConstraint("CK_TeachingAssignment_Hours", "\"WeeklyHours\" >= 0 AND \"Weeks\" > 0");
            });

            mb.Entity<Contract>()
                .HasOne(c => c.Teacher).WithMany(t => t.Contracts)
                .HasForeignKey(c => c.TeacherId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Contract>().Property(c => c.Type).HasConversion<string>().HasMaxLength(20);
            mb.Entity<Contract>().Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            mb.Entity<Contract>().Property(c => c.WeeklyHours).HasPrecision(5, 2);
            mb.Entity<Contract>().HasIndex(c => c.Number).IsUnique();
            mb.Entity<Contract>().HasIndex(c => c.EndDate);

            mb.Entity<TeacherDocument>()
                .HasOne(d => d.Teacher).WithMany(t => t.Documents)
                .HasForeignKey(d => d.TeacherId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<TeacherDocument>()
                .HasOne(d => d.Contract).WithMany(c => c.Documents)
                .HasForeignKey(d => d.ContractId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<TeacherDocument>().Property(d => d.Type).HasConversion<string>().HasMaxLength(30);
            mb.Entity<TeacherDocument>().Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
        }

        private static void SeedTimeSlots(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimeSlot>().HasData(
                new TimeSlot { SlotNumber = 1, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(9, 30, 0), IsLunchBreak = false },
                new TimeSlot { SlotNumber = 2, StartTime = new TimeSpan(9, 45, 0), EndTime = new TimeSpan(11, 15, 0), IsLunchBreak = false },
                new TimeSlot { SlotNumber = 3, StartTime = new TimeSpan(11, 30, 0), EndTime = new TimeSpan(13, 0, 0), IsLunchBreak = false },
                new TimeSlot { SlotNumber = 4, StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 30, 0), IsLunchBreak = true },
                new TimeSlot { SlotNumber = 5, StartTime = new TimeSpan(14, 30, 0), EndTime = new TimeSpan(16, 0, 0), IsLunchBreak = false }
            );
        }
    }
}
