using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningPlatform.Models.ViewModel
{
    public class PaymentViewModel
    {
        public Guid EnrollmentId { get; set; }
        public string CourseName { get; set; }
        public decimal Amount { get; set; }
        
        [Required(ErrorMessage = "Card holder name is required")]
        [Display(Name = "Card Holder Name")]
        public string CardHolderName { get; set; }
        
        [Required(ErrorMessage = "Card number is required")]
        [Display(Name = "Card Number")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be 16 digits")]
        public string CardNumber { get; set; }
        
        [Required(ErrorMessage = "Expiration month is required")]
        [Display(Name = "Expiration Month")]
        [Range(1, 12, ErrorMessage = "Expiration month must be between 1 and 12")]
        public int ExpirationMonth { get; set; }
        
        [Required(ErrorMessage = "Expiration year is required")]
        [Display(Name = "Expiration Year")]
        [Range(2023, 2035, ErrorMessage = "Expiration year must be between 2023 and 2035")]
        public int ExpirationYear { get; set; }
        
        [Required(ErrorMessage = "CVV is required")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
        public string CVV { get; set; }
        
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "CreditCard";
    }
}

