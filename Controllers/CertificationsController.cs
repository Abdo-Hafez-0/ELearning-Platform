using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Models.ViewModel;

namespace ELearningPlatform.Controllers
{
    public class CertificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CertificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Certifications/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var certification = await _context.Certifications
                .Include(c => c.User)
                .Include(c => c.Course)
                .ThenInclude(c => c.Instructor)
                .Include(c => c.Course.Lessons)
                .FirstOrDefaultAsync(c => c.CertificationID == id);

            if (certification == null)
            {
                return NotFound();
            }

            // Get enrollment information
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserID == certification.UserID && e.CourseID == certification.CourseID);

            // Get final assignment results
            var finalAssignmentResults = await _context.UserAssignmentResults
                .Include(r => r.Assignment)
                .Where(r => r.UserID == certification.UserID && 
                           r.Assignment.CourseID == certification.CourseID && 
                           r.Assignment.LessonId == null) // Final assignments have null LessonId
                .ToListAsync();

            int finalScore = 100;
            if (finalAssignmentResults.Any())
            {
                int correctAnswers = finalAssignmentResults.Count(r => r.IsCorrect);
                int totalQuestions = finalAssignmentResults.Count;
                finalScore = totalQuestions > 0 ? (correctAnswers * 100) / totalQuestions : 0;
            }

            // Create the view model
            var viewModel = new CertificationDetailsViewModel
            {
                CertificationID = certification.CertificationID,
                CertificateNumber = certification.CertificateNumber,
                CertificationDate = certification.CertificationDate,
                
                // User details
                UserID = certification.UserID,
                UserFullName = $"{certification.User.FirstName} {certification.User.LastName}",
                UserEmail = certification.User.Email,
                UserProfileImage = certification.User.ProfileImageUrl,
                
                // Course details
                CourseID = certification.CourseID,
                CourseName = certification.Course.CourseName,
                CourseDescription = certification.Course.Description,
                CourseImageUrl = certification.Course.ImageUrl,
                InstructorName = certification.Course.Instructor != null 
                    ? $"{certification.Course.Instructor.FirstName} {certification.Course.Instructor.LastName}" 
                    : "Unknown Instructor",
                DurationInHours = certification.Course.DurationInHours,
                CourseLevel = certification.Course.Level.ToString(),
                CourseLanguage = certification.Course.Language,
                
                // Achievement details
                TotalLessons = certification.Course.Lessons.Count,
                EnrollmentDate = enrollment?.EnrollmentDate ?? certification.CertificationDate.AddMonths(-1),
                CompletionDate = enrollment?.CompletedDate ?? certification.CertificationDate,
                FinalScore = finalScore
            };

            return View(viewModel);
        }

        // GET: Certifications/View/5
        public async Task<IActionResult> View(Guid id)
        {
            var certification = await _context.Certifications
                .Include(c => c.User)
                .Include(c => c.Course)
                .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CertificationID == id);

            if (certification == null)
            {
                return NotFound();
            }

            // Create the view model
            var viewModel = new CertificationViewModel
            {
                CertificationID = certification.CertificationID,
                CertificateNumber = certification.CertificateNumber,
                CertificationDate = certification.CertificationDate,
                
                // User details
                UserFullName = $"{certification.User.FirstName} {certification.User.LastName}",
                UserEmail = certification.User.Email,
                
                // Course details
                CourseName = certification.Course.CourseName,
                CourseDescription = certification.Course.Description,
                InstructorName = certification.Course.Instructor != null 
                    ? $"{certification.Course.Instructor.FirstName} {certification.Course.Instructor.LastName}" 
                    : "Unknown Instructor",
                DurationInHours = certification.Course.DurationInHours
            };

            return View(viewModel);
        }

        // GET: Certifications/Download/5
        public async Task<IActionResult> Download(Guid id)
        {
            var certification = await _context.Certifications
                .Include(c => c.User)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CertificationID == id);

            if (certification == null)
            {
                return NotFound();
            }

            // In a real application, you would generate a PDF certificate here
            // For now, we'll just redirect to the view
            TempData["DownloadMessage"] = "Your certificate is being downloaded...";
            return RedirectToAction("View", new { id = certification.CertificationID });
        }

        // GET: Certifications/List
        public async Task<IActionResult> List()
        {
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == UserRole.Student);
                
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Get all certifications for the user
            var certifications = await _context.Certifications
                .Include(c => c.Course)
                .Where(c => c.UserID == user.UserID)
                .ToListAsync();
                
            return View(certifications);
        }
    }
}

