using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models.ViewModel
{
    public class ReviewViewModel
    {
        public Guid ReviewID { get; set; }

        public Guid CourseID { get; set; }
        public string CourseName { get; set; }
        public string UserName { get; set; }
        public string UserProfileImage { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
        
        [Required(ErrorMessage = "Comment is required")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }

        //public virtual ICollection<User> user { get; set; }

    }
}

