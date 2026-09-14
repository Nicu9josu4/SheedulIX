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

            // Seed default timeslots
            SeedTimeSlots(modelBuilder);
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
