using Intellegens.Commons.DemoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Intellegens.Commons.DemoApi.DbContext
{
    public class DemoDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Data Source=Demo.db").EnableSensitiveDataLogging().UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())).ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StudentCourse>().HasAlternateKey(x => new { x.CourseId, x.StudentId });
        }
    }
}