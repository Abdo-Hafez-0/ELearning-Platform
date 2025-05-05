using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class Lessons
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Course")]
        public Guid CourseID { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public string VideoUrl { get; set; }
        
        public int DurationInMinutes { get; set; }
        
        public int Order { get; set; }
        
        // Navigation properties
        public virtual Course Course { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        
        public Lessons()
        {
            Assignments = new HashSet<Assignment>();
        }
    }
}

