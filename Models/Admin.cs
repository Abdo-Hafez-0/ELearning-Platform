using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models
{
    public class Admin
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; }
        
        // Additional properties
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
    }
}
