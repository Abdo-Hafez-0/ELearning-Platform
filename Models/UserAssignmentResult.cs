using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class UserAssignmentResult
    {
        [Key]
        public Guid ResultID { get; set; }
        
        [ForeignKey("User")]
        public Guid UserID { get; set; }
        
        [ForeignKey("Assignment")]
        public Guid TestID { get; set; }
        
        [ForeignKey("SelectedOption")]
        public Guid SelectedOptionID { get; set; }
        public bool IsCorrect { get; set; } = false;
        
        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual Assignment Assignment { get; set; }
        public virtual AssignmentOption SelectedOption { get; set; }
    }
}

