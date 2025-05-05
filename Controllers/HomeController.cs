using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Models.ViewModel;

namespace ELearningPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredCourses = await _context.Courses
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.AverageRating)
                .Take(3)
                .ToListAsync();


            // Get featured reviews (highest rated)
            var featuredReviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Course)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedAt)
                .Take(6)
                .Select(r => new ReviewViewModel
                {
                    ReviewID = r.ReviewID,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    UserProfileImage = "/images/"+r.User.ProfileImageUrl,
                    CourseName = r.Course.CourseName,
                    Rating = r.Rating,
                    Comment = r.Comment
                })
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedCourses = featuredCourses,
                FeaturedReviews = featuredReviews
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
/*
        private async Task SeedSampleData()
        {
            // Create sample users
            var instructor1 = new User
            {
                UserID = Guid.NewGuid(),
                Username = "johnsmith",
                Email = "john.smith@example.com",
                Password = "hashed_password_here", // In a real app, this would be hashed
                Age = 35,
                Role = UserRole.Instructor,
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "123-456-7890",
                BirthDate = new DateTime(1988, 5, 15),
                ProfileImageUrl = "/images/instructors/john-smith.jpg"
            };

            var instructor2 = new User
            {
                UserID = Guid.NewGuid(),
                Username = "sarahjohnson",
                Email = "sarah.johnson@example.com",
                Password = "hashed_password_here", // In a real app, this would be hashed
                Age = 32,
                Role = UserRole.Instructor,
                FirstName = "Sarah",
                LastName = "Johnson",
                PhoneNumber = "123-456-7891",
                BirthDate = new DateTime(1991, 8, 22),
                ProfileImageUrl = "/images/instructors/sarah-johnson.jpg"
            };

            var instructor3 = new User
            {
                UserID = Guid.NewGuid(),
                Username = "michaelbrown",
                Email = "michael.brown@example.com",
                Password = "hashed_password_here", // In a real app, this would be hashed
                Age = 40,
                Role = UserRole.Instructor,
                FirstName = "Michael",
                LastName = "Brown",
                PhoneNumber = "123-456-7892",
                BirthDate = new DateTime(1983, 3, 10),
                ProfileImageUrl = "/images/instructors/michael-brown.jpg"
            };

            var student1 = new User
            {
                UserID = Guid.NewGuid(),
                Username = "johndoe",
                Email = "john.doe@example.com",
                Password = "hashed_password_here", // In a real app, this would be hashed
                Age = 28,
                Role = UserRole.Student,
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "123-456-7893",
                BirthDate = new DateTime(1995, 1, 1),
                ProfileImageUrl = "/images/profile.jpg"
            };

            await _context.Users.AddRangeAsync(instructor1, instructor2, instructor3, student1);
            await _context.SaveChangesAsync();

            // Create sample courses
            var course1 = new Course
            {
                CourseID = Guid.NewGuid(),
                CourseName = "Complete Web Development Bootcamp",
                Description = "Learn HTML, CSS, JavaScript, React, Node.js and more to become a full-stack web developer.",
                Level = CourseLevel.Beginner,
                Language = "English",
                InstructorID = instructor1.UserID,
                Price = 49.99m,
                ImageUrl = "/images/courses/web-dev.jpg",
                DurationInHours = 42,
                AverageRating = 4.7,
                RatingCount = 1250,
                IsCertified = true
            };

            var course2 = new Course
            {
                CourseID = Guid.NewGuid(),
                CourseName = "Python for Data Science and Machine Learning",
                Description = "Master Python for data analysis, visualization, and machine learning with real-world projects.",
                Level = CourseLevel.Intermediate,
                Language = "English",
                InstructorID = instructor2.UserID,
                Price = 59.99m,
                ImageUrl = "/images/courses/python-ds.jpg",
                DurationInHours = 38,
                AverageRating = 4.8,
                RatingCount = 980,
                IsCertified = true
            };

            var course3 = new Course
            {
                CourseID = Guid.NewGuid(),
                CourseName = "ASP.NET Core MVC for Beginners",
                Description = "Build modern web applications with ASP.NET Core MVC, Entity Framework Core, and C#.",
                Level = CourseLevel.Beginner,
                Language = "English",
                InstructorID = instructor3.UserID,
                Price = 44.99m,
                ImageUrl = "/images/courses/aspnet-mvc.jpg",
                DurationInHours = 35,
                AverageRating = 4.5,
                RatingCount = 750,
                IsCertified = true
            };

            var course4 = new Course
            {
                CourseID = Guid.NewGuid(),
                CourseName = "Mobile App Development with Flutter",
                Description = "Create beautiful cross-platform mobile apps for iOS and Android using Flutter and Dart.",
                Level = CourseLevel.Intermediate,
                Language = "English",
                InstructorID = instructor1.UserID,
                Price = 54.99m,
                ImageUrl = "/images/courses/flutter.jpg",
                DurationInHours = 40,
                AverageRating = 4.6,
                RatingCount = 820,
                IsCertified = true
            };

            var course5 = new Course
            {
                CourseID = Guid.NewGuid(),
                CourseName = "UI/UX Design Fundamentals",
                Description = "Learn the principles of user interface and user experience design to create engaging digital products.",
                Level = CourseLevel.Beginner,
                Language = "English",
                InstructorID = instructor2.UserID,
                Price = 39.99m,
                ImageUrl = "/images/courses/uiux.jpg",
                DurationInHours = 28,
                AverageRating = 4.9,
                RatingCount = 1100,
                IsCertified = true
            };

            var course6 = new Course
            {
                CourseID = Guid.NewGuid(),
                CourseName = "DevOps Engineering with Docker and Kubernetes",
                Description = "Master containerization and orchestration for modern application deployment and scaling.",
                Level = CourseLevel.Advanced,
                Language = "English",
                InstructorID = instructor3.UserID,
                Price = 64.99m,
                ImageUrl = "/images/courses/devops.jpg",
                DurationInHours = 45,
                AverageRating = 4.7,
                RatingCount = 650,
                IsCertified = true
            };

            await _context.Courses.AddRangeAsync(course1, course2, course3, course4, course5, course6);
            await _context.SaveChangesAsync();

            // Create sample Lessons for course1
            var Lessons = new[]
            {
                new Lessons
                {
                    CourseID = course1.CourseID,
                    Title = "Introduction to Web Development",
                    Description = "Learn about the basics of web development and what you'll build in this course.",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", // Sample video URL
                    DurationInMinutes = 15,
                    Order = 1
                },
                new Lessons
                {
                    CourseID = course1.CourseID,
                    Title = "HTML Fundamentals",
                    Description = "Learn the building blocks of web pages with HTML5.",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", // Sample video URL
                    DurationInMinutes = 45,
                    Order = 2
                },
                new Lessons
                {
                    CourseID = course1.CourseID,
                    Title = "CSS Styling Basics",
                    Description = "Learn how to style your HTML with CSS to create beautiful web pages.",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", // Sample video URL
                    DurationInMinutes = 60,
                    Order = 3
                },
                new Lessons
                {
                    CourseID = course1.CourseID,
                    Title = "JavaScript Fundamentals",
                    Description = "Learn the basics of JavaScript programming for web development.",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", // Sample video URL
                    DurationInMinutes = 75,
                    Order = 4
                },
                new Lessons
                {
                    CourseID = course1.CourseID,
                    Title = "Responsive Web Design",
                    Description = "Learn how to make your websites look great on all devices.",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", // Sample video URL
                    DurationInMinutes = 60,
                    Order = 5
                }
            };

            await _context.Lessons.AddRangeAsync(Lessons);
            await _context.SaveChangesAsync();

            // Create sample assignments
            var assignment1 = new Assignment
            {
                TestID = Guid.NewGuid(),
                CourseID = course1.CourseID,
                LessonId = Lessons[0].Id,
                Title = "Introduction to Web Development Quiz",
                Question = "What does HTML stand for?"
            };

            await _context.Assignments.AddAsync(assignment1);
            await _context.SaveChangesAsync();

            // Create sample assignment options
            var options = new[]
            {
                new AssignmentOption
                {
                    OptionID = Guid.NewGuid(),
                    TestID = assignment1.TestID,
                    OptionText = "Hyper Text Markup Language",
                    IsCorrect = true
                },
                new AssignmentOption
                {
                    OptionID = Guid.NewGuid(),
                    TestID = assignment1.TestID,
                    OptionText = "High Tech Modern Language",
                    IsCorrect = false
                },
                new AssignmentOption
                {
                    OptionID = Guid.NewGuid(),
                    TestID = assignment1.TestID,
                    OptionText = "Hyperlinks and Text Markup Language",
                    IsCorrect = false
                },
                new AssignmentOption
                {
                    OptionID = Guid.NewGuid(),
                    TestID = assignment1.TestID,
                    OptionText = "Home Tool Markup Language",
                    IsCorrect = false
                }
            };

            await _context.AssignmentOptions.AddRangeAsync(options);
            await _context.SaveChangesAsync();

            // Create sample enrollments
            var enrollment1 = new Enrollment
            {
                EnrollmentID = Guid.NewGuid(),
                UserID = student1.UserID,
                CourseID = course1.CourseID,
                EnrollmentDate = DateTime.Now.AddDays(-30),
                Status = EnrollmentStatus.Active,
                Progress = 60,
                LastLessonId = 3,
                IsPurchased = true
            };

            var enrollment2 = new Enrollment
            {
                EnrollmentID = Guid.NewGuid(),
                UserID = student1.UserID,
                CourseID = course2.CourseID,
                EnrollmentDate = DateTime.Now.AddDays(-15),
                Status = EnrollmentStatus.Active,
                Progress = 0,
                LastLessonId = 0,
                IsPurchased = false
            };

            var enrollment3 = new Enrollment
            {
                EnrollmentID = Guid.NewGuid(),
                UserID = student1.UserID,
                CourseID = course3.CourseID,
                EnrollmentDate = DateTime.Now.AddDays(-60),
                Status = EnrollmentStatus.Completed,
                Progress = 100,
                LastLessonId = 5,
                CompletedDate = DateTime.Now.AddDays(-5),
                IsPurchased = true
            };

            await _context.Enrollments.AddRangeAsync(enrollment1, enrollment2, enrollment3);
            await _context.SaveChangesAsync();

            // Create sample certification
            var certification = new Certification
            {
                CertificationID = Guid.NewGuid(),
                UserID = student1.UserID,
                CourseID = course3.CourseID,
                CertificationDate = DateTime.Now.AddDays(-5),
                CertificateNumber = "CERT-12345"
            };

            await _context.Certifications.AddAsync(certification);
            await _context.SaveChangesAsync();

            // Create sample reviews
            var review1 = new Review
            {
                ReviewID = Guid.NewGuid(),
                CourseID = course1.CourseID,
                UserID = student1.UserID,
                Rating = 5,
                Comment = "This course was excellent! I learned so much about web development.",
                CreatedAt = DateTime.Now.AddDays(-10)
            };

            var review2 = new Review
            {
                ReviewID = Guid.NewGuid(),
                CourseID = course3.CourseID,
                UserID = student1.UserID,
                Rating = 4,
                Comment = "Great course on ASP.NET Core MVC. The instructor was very knowledgeable.",
                CreatedAt = DateTime.Now.AddDays(-5)
            };

            await _context.Reviews.AddRangeAsync(review1, review2);
            await _context.SaveChangesAsync();
           
        } */
    }
}

