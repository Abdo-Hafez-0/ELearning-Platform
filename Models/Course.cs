using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class Course
    {
        [Key]
        public Guid CourseID { get; set; }
        
        [Required]
        [StringLength(255)]
        public string CourseName { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public CourseLevel Level { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Language { get; set; }
        
        [ForeignKey("Instructor")]
        public Guid InstructorID { get; set; }
        
        public decimal Price { get; set; }
        
        public string ImageUrl { get; set; }
        
        public int DurationInHours { get; set; }
        
        public double AverageRating { get; set; }
        
        public int RatingCount { get; set; }
        
        public bool IsCertified { get; set; } = true;
        
        // Navigation properties
        public virtual User Instructor { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Certification> Certifications { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Lessons> Lessons { get; set; }
        
        public Course()
        {
            Enrollments = new HashSet<Enrollment>();
            Assignments = new HashSet<Assignment>();
            Certifications = new HashSet<Certification>();
            Reviews = new HashSet<Review>();
            Lessons = new HashSet<Lessons>();
        }
    }
    
    public enum CourseLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }
}

