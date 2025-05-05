using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Models;

namespace ELearningPlatform.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentOption> AssignmentOptions { get; set; }
        public DbSet<UserAssignmentResult> UserAssignmentResults { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Lessons> Lessons { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Ignore<Admin>();

            // Configure unique indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(u => u.InstructorCourses)
                .HasForeignKey(c => c.InstructorID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentOption>()
                .HasOne(o => o.Assignment)
                .WithMany(a => a.Options)
                .HasForeignKey(o => o.TestID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAssignmentResult>()
                .HasOne(r => r.User)
                .WithMany(u => u.AssignmentResults)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAssignmentResult>()
                .HasOne(r => r.Assignment)
                .WithMany(a => a.Results)
                .HasForeignKey(r => r.TestID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAssignmentResult>()
                .HasOne(r => r.SelectedOption)
                .WithMany(o => o.Results)
                .HasForeignKey(r => r.SelectedOptionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Certification>()
                .HasOne(c => c.User)
                .WithMany(u => u.Certifications)
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Certification>()
                .HasOne(c => c.Course)
                .WithMany(c => c.Certifications)
                .HasForeignKey(c => c.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lessons>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(e => e.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Lesson)
                .WithMany(e => e.Assignments)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.SetNull);
        }


    }
}

