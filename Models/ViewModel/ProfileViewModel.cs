using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models.ViewModel
{
    public class ProfileViewModel
    {
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First Name can only contain letters and spaces.")]
        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last Name can only contain letters and spaces.")]
        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        //[Required(ErrorMessage = "Email is required")]
        //[EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        //[Required(ErrorMessage = "Birth Date is required")]
        //[DataType(DataType.Date)]
        //[Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Profile Image")]
        public string ProfileImageUrl { get; set; }

        //[Required(ErrorMessage = "Password is required")]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        //public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm Password")]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        //public string ConfirmPassword { get; set; }
    }
}

