using System;

namespace ELearningPlatform.Models.ViewModel
{
    public class CertificationDetailsViewModel
    {
        // Certificate details
        public Guid CertificationID { get; set; }
        public string CertificateNumber { get; set; }
        public DateTime CertificationDate { get; set; }
        
        // User details
        public Guid UserID { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string UserProfileImage { get; set; }
        
        // Course details
        public Guid CourseID { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string CourseImageUrl { get; set; }
        public string InstructorName { get; set; }
        public int DurationInHours { get; set; }
        public string CourseLevel { get; set; }
        public string CourseLanguage { get; set; }
        
        // Achievement details
        public int TotalLessons { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime CompletionDate { get; set; }
        public int FinalScore { get; set; }
    }
}

