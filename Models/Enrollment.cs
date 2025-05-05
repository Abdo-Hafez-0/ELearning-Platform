using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class Enrollment
    {
        [Key]
        public Guid EnrollmentID { get; set; }
        
        [ForeignKey("User")]
        public Guid UserID { get; set; }
        
        [ForeignKey("Course")]
        public Guid CourseID { get; set; }
        
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
        
        public int Progress { get; set; } = 0;
        
        public DateTime? CompletedDate { get; set; }
        
        public int LastLessonId { get; set; }
        
        public bool IsPurchased { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual Course Course { get; set; }
        
        public bool IsCompleted => Progress >= 100;
    }
    
    public enum EnrollmentStatus
    {
        Active,
        Completed,
        Dropped
    }
}

