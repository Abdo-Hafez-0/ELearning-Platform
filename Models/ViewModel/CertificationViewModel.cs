using System;

namespace ELearningPlatform.Models.ViewModel
{
    public class CertificationViewModel
    {
        // Certificate details
        public Guid CertificationID { get; set; }
        public string CertificateNumber { get; set; }
        public DateTime CertificationDate { get; set; }

        // User details
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }

        // Course details
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string InstructorName { get; set; }
        public int DurationInHours { get; set; }

        // Added missing property for issuer name
        public string IssuerName { get; set; } = "E-Learning Platform";
    }
}

