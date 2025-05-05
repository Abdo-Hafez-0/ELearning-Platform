using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class AssignmentOption
    {
        [Key]
        public Guid OptionID { get; set; }
        
        [ForeignKey("Assignment")]
        public Guid TestID { get; set; }
        
        [Required]
        public string OptionText { get; set; }
        
        public bool IsCorrect { get; set; } = false;
        
        // Navigation properties
        public virtual Assignment Assignment { get; set; }
        public virtual ICollection<UserAssignmentResult> Results { get; set; }
        //public ICollection<UserAssignmentResult> UserAssignmentResults { get; set; }
        public AssignmentOption()
        {
            Results = new HashSet<UserAssignmentResult>();
        }
    }
}

