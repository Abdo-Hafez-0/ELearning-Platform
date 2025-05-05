using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class Assignment
    {
        [Key]
        public Guid TestID { get; set; }
        
        [ForeignKey("Course")]
        public Guid CourseID { get; set; }
        
        [Required]
        public string Question { get; set; }
        
        public string Title { get; set; }
        
        [ForeignKey("Lessons")]
        public int? LessonId { get; set; }
        
        // Navigation properties
        public virtual Course Course { get; set; }
        public virtual Lessons Lesson { get; set; }
        public virtual ICollection<AssignmentOption> Options { get; set; }
        public virtual ICollection<UserAssignmentResult> Results { get; set; }
        
        public Assignment()
        {
            Options = new HashSet<AssignmentOption>();
            Results = new HashSet<UserAssignmentResult>();
        }
    }
}

