using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class Certification
    {
        [Key]
        public Guid CertificationID { get; set; }
        
        [ForeignKey("User")]
        public Guid UserID { get; set; }
        
        [ForeignKey("Course")]
        public Guid CourseID { get; set; }
        
        public DateTime CertificationDate { get; set; } = DateTime.Now;
        
        public string CertificateNumber { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
        public virtual Course Course { get; set; }
    }
}

