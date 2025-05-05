using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models.ViewModel
{
    public class RateViewModel
    {
        public Guid CourseID { get; set; }
        
        public string? CourseName { get; set; }
        
        public string? CourseImageUrl { get; set; }
        
        public string? InstructorName { get; set; }
        
        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        
        [Required(ErrorMessage = "Please provide a comment")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }
        
        public bool IsEdit { get; set; }
    }
}
