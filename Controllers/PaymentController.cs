using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Models.ViewModel;

namespace ELearningPlatform.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payment
        public IActionResult Index()
        {
            // Check if we have enrollment information in TempData
            if (TempData["EnrollmentId"] == null || TempData["CourseName"] == null || TempData["CoursePrice"] == null)
            {
                return RedirectToAction("Index", "MyLearning");
            }
            
            // Create a payment view model
            var viewModel = new PaymentViewModel
            {
                EnrollmentId = (Guid)TempData["EnrollmentId"],
                CourseName = TempData["CourseName"].ToString(),
                Amount = decimal.Parse(TempData["CoursePrice"].ToString())
            };
            
            // Keep the values in TempData for the post action
            TempData.Keep("EnrollmentId");
            TempData.Keep("CourseName");
            TempData.Keep("CoursePrice");
            
            return View(viewModel);
        }

        // POST: Payment/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
                {
                    return RedirectToAction("Login", "Account");
                }
                // Get the current user (in a real app, this would come from authentication)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


                // Get the enrollment
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.EnrollmentID == model.EnrollmentId && e.UserID == user.UserID);
                    
                if (enrollment == null)
                {
                    return NotFound();
                }
                
                // In a real application, you would:
                // 1. Process the payment with a payment gateway
                // 2. Record the transaction in your database
                // 3. Update the user's course status
                
                // For now, we'll just mark the course as purchased
                enrollment.IsPurchased = true;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"You have successfully purchased {model.CourseName}. You can now access all course materials and earn a certificate upon completion.";
                
                return RedirectToAction("Index", "MyLearning");
            }
            
            return View("Index", model);
        }
    }
}

