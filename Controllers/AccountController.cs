using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ELearningPlatform.Models.ViewModel;

namespace ELearningPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login");
            }
            if(HttpContext.Session.GetString("UserRole") == UserRole.Instructor.ToString())
            {
                return RedirectToAction("Dashboard", "Instructor");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .Include(u => u.Certifications)
                .Include(u => u.Enrollments)
                .ThenInclude(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));

            return View(user);
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            var viewModel = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate ?? DateTime.Now,
                ProfileImageUrl = user.ProfileImageUrl
            };

            // Add additional user information to ViewBag
            // In a real application, these would be properties of the User model
            ViewBag.Bio = ""; // Placeholder for user bio
            ViewBag.Interests = ""; // Placeholder for user interests
            ViewBag.LinkedIn = ""; // Placeholder for LinkedIn URL
            ViewBag.GitHub = ""; // Placeholder for GitHub URL
            ViewBag.Website = ""; // Placeholder for personal website URL

            return View(viewModel);
        }

        // POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model, IFormFile ProfileImage)
        {
            var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

            // Update the user's profile
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.BirthDate = model.BirthDate;
            //user.Password = model.Password;


            ModelState.Remove("ProfileImage");
            // Handle profile image upload
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                user.ProfileImageUrl = ProfileImage.FileName;
            }
            else
            {
                // Keep the existing profile image if no new one is uploaded
                user.ProfileImageUrl = model.ProfileImageUrl;
            }
            if (ModelState.IsValid)
            {
                // Get the current user (in a real app, this would come from authentication)


                // In a real application, you would also update additional user information
                // user.Bio = Request.Form["Bio"];
                // user.Interests = Request.Form["Interests"];
                // etc.

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                return RedirectToAction("Profile");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // GET: Account/Reviews
        public async Task<IActionResult> Reviews()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));

            // Get the user's reviews
            var reviews = await _context.Reviews
                .Include(r => r.Course)
                .Where(r => r.UserID == user.UserID)
                .ToListAsync();

            return View(reviews);
        }

        // GET: Account/Certificates
        public async Task<IActionResult> Certificates()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Get the user's certificates
            var certificates = await _context.Certifications
                .Include(c => c.Course)
                .Where(c => c.UserID == user.UserID)
                .ToListAsync();

            return View(certificates);
        }

        // GET: Account/AddReview/5
        public async Task<IActionResult> AddReview(Guid id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login");
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

            // Check if the user has completed the course
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == id);

            if (enrollment == null || enrollment.Status != EnrollmentStatus.Completed)
            {
                TempData["ErrorMessage"] = "You must complete the course before you can review it.";
                return RedirectToAction("Index", "MyLearning");
            }

            // Check if the user has already reviewed the course
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserID == user.UserID && r.CourseID == id);

            if (existingReview != null)
            {
                TempData["InfoMessage"] = "You have already reviewed this course.";
                return RedirectToAction("Reviews");
            }

            var viewModel = new ReviewViewModel
            {
                CourseID = id,
                CourseName = course.CourseName
            };

            return View(viewModel);
        }
        // GET: Account/ViewCertificate/5
        public async Task<IActionResult> ViewCertificate(Guid id)
        {
            // Get the certification
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
        // GET: Account/PrintCertificate/5
        public async Task<IActionResult> PrintCertificate(Guid id)
        {
            // Get the certification
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
        // POST: Account/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(ReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
                {
                    return RedirectToAction("Login");
                }
                // Get the current user (in a real app, this would come from authentication)
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


                // Create a new review
                var review = new Review
                {
                    ReviewID = Guid.NewGuid(),
                    CourseID = model.CourseID,
                    UserID = user.UserID,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now
                };

                await _context.Reviews.AddAsync(review);

                // Update the course's average rating
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseID == model.CourseID);

                if (course != null)
                {
                    // Get all reviews for the course
                    var reviews = await _context.Reviews
                        .Where(r => r.CourseID == model.CourseID)
                        .ToListAsync();

                    // Add the new review
                    reviews.Add(review);

                    // Calculate the new average rating
                    course.AverageRating = reviews.Average(r => r.Rating);
                    course.RatingCount = reviews.Count;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your review has been submitted successfully.";
                return RedirectToAction("Reviews");
            }

            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            DateTime minDate = DateTime.Today.AddYears(-10);

            if (model.BirthDate > minDate)
            {
                ModelState.AddModelError("BirthDate", "You must be at least 10 years old.");
            }
            
            if (ModelState.IsValid)
            {
                // Check if the email is already in use
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already in use.");
                    return View(model);
                }

                // Create a new user
                var user = new User
                {
                    UserID = Guid.NewGuid(),
                    Username = model.Email.Split('@')[0], // Generate a username from the email
                    Email = model.Email,
                    Password = model.Password, // In a real app, this would be hashed
                    Role = UserRole.Student,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    BirthDate = model.BirthDate,
                    ProfileImageUrl = model.ProfileImageUrl
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            // Display success message if registration was successful
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Validate the user's credentials
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            
            var admin = await _context.Admins
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            
            if (admin != null)
            {
                return RedirectToAction("Login", "Admin", new { email = admin.Email, password = admin.Password });
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            


            // Store user information in session
            HttpContext.Session.SetString("UserName", user.FirstName + " " + user.LastName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserID", user.UserID.ToString());
            HttpContext.Session.SetString("UserRole", user.Role.ToString());
            HttpContext.Session.SetString("UserImg", user.ProfileImageUrl.ToString());

            // In a real app, you would:
            // 1. Sign the user in using ASP.NET Core Identity
            // 2. Redirect to the home page or dashboard

            if(user.Role == UserRole.Instructor)
            {
                return RedirectToAction("Dashboard", "Instructor");
            }
            return RedirectToAction("Dashboard");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}

