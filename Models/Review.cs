using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class Review
    {
        [Key]
        public Guid ReviewID { get; set; }
        
        [ForeignKey("Course")]
        public Guid CourseID { get; set; }
        
        [ForeignKey("User")]
        public Guid UserID { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [StringLength(500)]
        public string Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual Course Course { get; set; }
        public virtual User User { get; set; }
    }
}

