using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELearningPlatform.Models
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }
        
        //[Range(0, int.MaxValue)]
        //public int? Age { get; set; }
        
        [Required]
        public UserRole Role { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First Name can only contain letters and spaces.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last Name can only contain letters and spaces.")]
        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Invalid phone number format.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Birth Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        //[MinimumAge(10, ErrorMessage = "You must be at least 10 years old.")]
        public DateTime? BirthDate { get; set; }

        //[Required(ErrorMessage = "Image is required")]
        [Display(Name = "Image")]
        public string ProfileImageUrl { get; set; }
        
        // Navigation properties
        public virtual ICollection<Course> InstructorCourses { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<UserAssignmentResult> AssignmentResults { get; set; }
        public virtual ICollection<Certification> Certifications { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        
        public User()
        {
            InstructorCourses = new HashSet<Course>();
            Enrollments = new HashSet<Enrollment>();
            AssignmentResults = new HashSet<UserAssignmentResult>();
            Certifications = new HashSet<Certification>();
            Reviews = new HashSet<Review>();
        }
    }
    
    public enum UserRole
    {
        Student,
        Instructor
    }
}

