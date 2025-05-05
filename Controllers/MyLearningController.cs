using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;

namespace ELearningPlatform.Controllers
{
    public class MyLearningController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MyLearningController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MyLearning
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login","Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .Where(e => e.UserID == user.UserID)
                .ToListAsync();

            // Add context to ViewBag for use in the view
            ViewBag.Context = _context;

            // Or better approach: prepare certificate data for the view
            var userId = user.UserID;
            var enrollmentCourseIds = enrollments.Select(e => e.CourseID).ToList();

            var certificateDict = await _context.Certifications
                .Where(c => c.UserID == userId && enrollmentCourseIds.Contains(c.CourseID))
                .ToDictionaryAsync(c => c.CourseID, c => true);

            ViewBag.HasCertificate = certificateDict;

            return View(enrollments);
        }

        // GET: MyLearning/AddCourse/5
        public async Task<IActionResult> AddCourse(Guid id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login","Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Get the course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            // Check if the user is already enrolled in the course
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == id);

            if (existingEnrollment != null)
            {
                TempData["InfoMessage"] = "You are already enrolled in this course.";
                return RedirectToAction("Index");
            }

            // Create a new enrollment
            var enrollment = new Enrollment
            {
                EnrollmentID = Guid.NewGuid(),
                UserID = user.UserID,
                CourseID = id,
                EnrollmentDate = DateTime.Now,
                Status = EnrollmentStatus.Active,
                Progress = 0,
                LastLessonId = 0,
                IsPurchased = false
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{course.CourseName} has been added to your learning list.";

            return RedirectToAction("Index");
        }

        // GET: MyLearning/RemoveCourse/5
        public async Task<IActionResult> RemoveCourse(Guid id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login","Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));

            // Get the enrollment
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentID == id && e.UserID == user.UserID);

            if (enrollment == null)
            {
                return NotFound();
            }

            // Remove the enrollment
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Course removed from your learning list.";

            return RedirectToAction("Index");
        }

        // GET: MyLearning/Purchase/5
        public async Task<IActionResult> Purchase(Guid id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login","Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Get the enrollment
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentID == id && e.UserID == user.UserID);

            if (enrollment == null)
            {
                return NotFound();
            }

            // Store the course ID in TempData for the payment page
            TempData["EnrollmentId"] = enrollment.EnrollmentID.ToString();
            TempData["CourseName"] = enrollment.Course.CourseName.ToString();
            TempData["CoursePrice"] = enrollment.Course.Price.ToString();

            return RedirectToAction("Index", "Payment");
        }

        // GET: MyLearning/PurchaseConfirm/5
        public async Task<IActionResult> PurchaseConfirm(Guid id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login","Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));

            // Get the enrollment
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentID == id && e.UserID == user.UserID);

            if (enrollment == null)
            {
                return NotFound();
            }

            // Mark the course as purchased
            enrollment.IsPurchased = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"You have successfully purchased {enrollment.Course.CourseName}. You can now access all course materials and earn a certificate upon completion.";

            return RedirectToAction("Index");
        }

        // GET: MyLearning/UpdateProgress/5?progress=75&LessonId=4
        public async Task<IActionResult> UpdateProgress(Guid id, int progress, int LessonId)
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
                .FirstOrDefaultAsync(e => e.CourseID == id && e.UserID == user.UserID);

            if (enrollment == null)
            {
                return NotFound();
            }

            // Update the progress
            enrollment.Progress = progress;
            enrollment.LastLessonId = LessonId;

            if (progress >= 100)
            {
                enrollment.Status = EnrollmentStatus.Completed;
                enrollment.CompletedDate = DateTime.Now;

                // If the course is purchased and certified, create a certificate
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseID == id);

                if (course != null && enrollment.IsPurchased && course.IsCertified)
                {
                    var existingCertificate = await _context.Certifications
                        .FirstOrDefaultAsync(c => c.UserID == user.UserID && c.CourseID == id);

                    if (existingCertificate == null)
                    {
                        var certificate = new Certification
                        {
                            CertificationID = Guid.NewGuid(),
                            UserID = user.UserID,
                            CourseID = id,
                            CertificationDate = DateTime.Now,
                            CertificateNumber = $"CERT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                        };

                        await _context.Certifications.AddAsync(certificate);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Congratulations! You have completed this course.";
            }
            else
            {
                TempData["SuccessMessage"] = "Your progress has been updated.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        // GET: MyLearning/RateCourse/5
        public async Task<IActionResult> RateCourse(Guid id)
        {
            // Redirect to the Reviews controller's Rate action
            return RedirectToAction("Rate", "Reviews", new { id });
        }
    }
}

