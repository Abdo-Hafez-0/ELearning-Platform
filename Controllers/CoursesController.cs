using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;
using System.Numerics;
using ELearningPlatform.Models.ViewModel;

namespace ELearningPlatform.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .ToListAsync();

            return View(courses);
        }

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(Guid id, int? LessonId = null)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            var Lessons = course.Lessons.OrderBy(e => e.Order).ToList();

            if (Lessons == null || !Lessons.Any())
            {
                return NotFound("No Lessons found for this course.");
            }

            // Determine which Lesson to display
            var currentLesson = LessonId.HasValue
                ? Lessons.FirstOrDefault(e => e.Id == LessonId.Value)
                : Lessons.OrderBy(e => e.Order).FirstOrDefault();

            if (currentLesson == null)
            {
                currentLesson = Lessons.OrderBy(e => e.Order).FirstOrDefault();
            }

            // Determine previous and next Lesson IDs
            int? previousLessonId = null;
            int? nextLessonId = null;

            if (currentLesson != null)
            {
                var orderedLessons = Lessons.OrderBy(e => e.Order).ToList();
                var currentIndex = orderedLessons.FindIndex(e => e.Id == currentLesson.Id);

                if (currentIndex > 0)
                {
                    previousLessonId = orderedLessons[currentIndex - 1].Id;
                }

                if (currentIndex < orderedLessons.Count - 1)
                {
                    nextLessonId = orderedLessons[currentIndex + 1].Id;
                }
            }

            // Get quiz for the current Lesson
            var quiz = await _context.Assignments
                .Include(a => a.Options)
                .FirstOrDefaultAsync(a => a.LessonId == currentLesson.Id);

            // Check if the current user is already enrolled in this course
            bool isEnrolled = false;
            bool isPurchased = false;
            bool isCompleted = false;

            if (user != null)
            {
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == id);

                isEnrolled = enrollment != null;
                isPurchased = enrollment?.IsPurchased ?? false;
                isCompleted = enrollment?.Status == EnrollmentStatus.Completed;
            }

            

            // Create the view model
            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                Lessons = Lessons,
                CurrentLesson = currentLesson,
                PreviousLessonId = previousLessonId,
                NextLessonId = nextLessonId,
                LessonQuiz = quiz != null ? new Quiz
                {
                    Id = (int)quiz.TestID.GetHashCode(),
                    LessonId = currentLesson.Id,
                    Title = quiz.Title ?? "Quiz",
                    Questions = new List<QuizQuestion>
                  {
                      new QuizQuestion
                      {
                          Id = (int)quiz.TestID.GetHashCode(),
                          QuizId = (int)quiz.TestID.GetHashCode(),
                          Question = quiz.Question,
                          Options = quiz.Options != null ? quiz.Options.Select(opt => new QuizOption
                          {
                              Id = (int)opt.OptionID.GetHashCode(),
                              QuestionId = (int)quiz.TestID.GetHashCode(),
                              Text = opt.OptionText
                          }).ToList() : new List<QuizOption>(),
                          CorrectOptionId = quiz.Options != null && quiz.Options.Any(opt => opt.IsCorrect)
                              ? (int)quiz.Options.FirstOrDefault(opt => opt.IsCorrect).OptionID.GetHashCode()
                              : 0
                      }
                  }
                } : null,
                IsEnrolled = isEnrolled,
                IsPurchased = isPurchased,
                IsCompleted = isCompleted
            };

            // Get reviews for this course
            viewModel.Reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.CourseID == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(viewModel);
        }

        //// GET: Courses/CompleteCourse/5
        //public async Task<IActionResult> CompleteCourse(Guid id)
        //{
        //    // Get the current user (in a real app, this would come from authentication)
        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Role == UserRole.Student);

        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    // Get the course
        //    var course = await _context.Courses
        //        .FirstOrDefaultAsync(c => c.CourseID == id);

        //    if (course == null)
        //    {
        //        return NotFound();
        //    }

        //    // Get the enrollment
        //    var enrollment = await _context.Enrollments
        //        .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == id);

        //    if (enrollment == null)
        //    {
        //        return NotFound("You are not enrolled in this course.");
        //    }

        //    if (!enrollment.IsPurchased)
        //    {
        //        return BadRequest("You must purchase this course before completing it.");
        //    }

        //    if (enrollment.Status == EnrollmentStatus.Completed)
        //    {
        //        TempData["InfoMessage"] = "This course is already marked as completed.";
        //        return RedirectToAction("Index", "MyLearning");
        //    }

        //    // Mark the course as completed
        //    enrollment.Status = EnrollmentStatus.Completed;
        //    enrollment.Progress = 100;
        //    enrollment.CompletedDate = DateTime.Now;

        //    // If the course is certified, create a certificate
        //    if (course.IsCertified)
        //    {
        //        var existingCertificate = await _context.Certifications
        //            .FirstOrDefaultAsync(c => c.UserID == user.UserID && c.CourseID == id);

        //        if (existingCertificate == null)
        //        {
        //            var certificate = new Certification
        //            {
        //                CertificationID = Guid.NewGuid(),
        //                UserID = user.UserID,
        //                CourseID = id,
        //                CertificationDate = DateTime.Now,
        //                CertificateNumber = $"CERT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
        //            };

        //            await _context.Certifications.AddAsync(certificate);
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] = $"Congratulations! You have completed the course '{course.CourseName}'.";

        //    if (course.IsCertified)
        //    {
        //        TempData["SuccessMessage"] += " A certificate has been issued to you.";
        //    }

        //    return RedirectToAction("Index", "MyLearning");
        //}

        // GET: Courses/CompleteAndRedirect/5
        public async Task<IActionResult> CompleteAndRedirect(Guid id)
        {
            // Get the course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound("Course not found.");
            }


            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Check if the user has purchased the course
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == id);

            if (enrollment == null)
            {
                TempData["ErrorMessage"] = "You must be enrolled in this course to complete it.";
                return RedirectToAction("Details", new { id });
            }

            if (!enrollment.IsPurchased)
            {
                TempData["ErrorMessage"] = "You must purchase this course before completing it.";
                return RedirectToAction("Details", new { id });
            }

            // Mark the course as complete
            enrollment.Status = EnrollmentStatus.Completed;
            enrollment.Progress = 100;
            enrollment.CompletedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            // Set success message
            TempData["SuccessMessage"] = "Congratulations! You have completed this course.";

            // Redirect to the Final Assignment page
            return RedirectToAction("FinalAssignment", new { id });
        }

        // GET: Courses/Quiz/5
        public async Task<IActionResult> Quiz(Guid id, int LessonId)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            var Lesson = await _context.Lessons
                .FirstOrDefaultAsync(e => e.Id == LessonId && e.CourseID == id);

            if (Lesson == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(a => a.Options)
                .FirstOrDefaultAsync(a => a.LessonId == LessonId);

            if (assignment == null)
            {
                return NotFound("No quiz found for this Lesson.");
            }

            // Check if options exist
            if (assignment.Options == null || !assignment.Options.Any())
            {
                return NotFound("No options found for this quiz.");
            }

            var quiz = new Quiz
            {
                Id = (int)assignment.TestID.GetHashCode(),
                LessonId = LessonId,
                Title = assignment.Title ?? "Quiz",
                Questions = new List<QuizQuestion>
              {
                  new QuizQuestion
                  {
                      Id = 1,
                      QuizId = (int)assignment.TestID.GetHashCode(),
                      Question = assignment.Question,
                      Options = assignment.Options.Select((o, index) => new QuizOption
                      {
                          Id = index + 1,
                          QuestionId = 1,
                          Text = o.OptionText
                      }).ToList(),
                      CorrectOptionId = assignment.Options.ToList().FindIndex(o => o.IsCorrect) + 1
                  }
              }
            };

            ViewBag.Course = course;
            ViewBag.Lesson = Lesson;

            return View(quiz);
        }

        // POST: Courses/SubmitQuiz
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int quizId, Guid courseId, int LessonId, Dictionary<int, int> answers)
        {
            // Get the assignment
            var assignment = await _context.Assignments
                .Include(a => a.Options)
                .FirstOrDefaultAsync(a => a.LessonId == LessonId);

            if (assignment == null)
            {
                return NotFound();
            }

            // Check if options exist
            if (assignment.Options == null || !assignment.Options.Any())
            {
                return NotFound("No options found for this quiz.");
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Calculate the score
            int correctAnswers = 0;
            int totalQuestions = 1; // We only have one question per quiz in this example

            foreach (var answer in answers)
            {
                var questionId = answer.Key;
                var selectedOptionId = answer.Value;

                // Find the correct option
                var options = assignment.Options.ToList();
                var correctOptionIndex = options.FindIndex(o => o.IsCorrect) + 1;

                if (selectedOptionId == correctOptionIndex)
                {
                    correctAnswers++;

                    // Record the result
                    var quizResult = new UserAssignmentResult
                    {
                        ResultID = Guid.NewGuid(),
                        UserID = user.UserID,
                        TestID = assignment.TestID,
                        SelectedOptionID = options[selectedOptionId - 1].OptionID,
                        IsCorrect = true,
                        SubmissionDate = DateTime.Now
                    };

                    await _context.UserAssignmentResults.AddAsync(quizResult);
                }
                else if (selectedOptionId > 0 && selectedOptionId <= options.Count)
                {
                    // Record the result
                    var quizResult = new UserAssignmentResult
                    {
                        ResultID = Guid.NewGuid(),
                        UserID = user.UserID,
                        TestID = assignment.TestID,
                        SelectedOptionID = options[selectedOptionId - 1].OptionID,
                        IsCorrect = false,
                        SubmissionDate = DateTime.Now
                    };

                    await _context.UserAssignmentResults.AddAsync(quizResult);
                }
                else
                {
                    // Invalid option selected
                    return BadRequest("Invalid option selected.");
                }
            }

            await _context.SaveChangesAsync();

            // Calculate the percentage
            int percentage = totalQuestions > 0 ? (correctAnswers * 100) / totalQuestions : 0;

            // Determine if the user passed (70% or higher)
            bool passed = percentage >= 70;

            // Update the user's progress if they passed
            if (passed)
            {
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == courseId);

                if (enrollment != null)
                {
                    // Calculate new progress based on completed Lessons
                    var course = await _context.Courses
                        .Include(c => c.Lessons)
                        .FirstOrDefaultAsync(c => c.CourseID == courseId);

                    if (course != null)
                    {
                        var totalLessons = course.Lessons.Count;
                        var completedLessons = await _context.UserAssignmentResults
                            .Include(r => r.Assignment)
                            .Where(r => r.UserID == user.UserID && r.IsCorrect && r.Assignment.CourseID == courseId)
                            .Select(r => r.Assignment.LessonId)
                            .Distinct()
                            .CountAsync();

                        var newProgress = totalLessons > 0 ? (completedLessons * 100) / totalLessons : 0;

                        enrollment.Progress = Math.Max(enrollment.Progress, newProgress);
                        enrollment.LastLessonId = LessonId;

                        if (enrollment.Progress >= 100)
                        {
                            enrollment.Status = EnrollmentStatus.Completed;
                            enrollment.CompletedDate = DateTime.Now;

                            // If the course is purchased and certified, create a certificate
                            if (enrollment.IsPurchased && course.IsCertified)
                            {
                                var existingCertificate = await _context.Certifications
                                    .FirstOrDefaultAsync(c => c.UserID == user.UserID && c.CourseID == courseId);

                                if (existingCertificate == null)
                                {
                                    var certificate = new Certification
                                    {
                                        CertificationID = Guid.NewGuid(),
                                        UserID = user.UserID,
                                        CourseID = courseId,
                                        CertificationDate = DateTime.Now,
                                        CertificateNumber = $"CERT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                                    };

                                    await _context.Certifications.AddAsync(certificate);
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }
            }

            // Create the result view model
            var resultViewModel = new QuizResultViewModel
            {
                QuizId = quizId,
                CourseId = courseId,
                LessonId = LessonId,
                CorrectAnswers = correctAnswers,
                TotalQuestions = totalQuestions,
                Percentage = percentage,
                Passed = passed
            };
            ViewData["CoursId"] = courseId;
            ViewData["EpId"] = LessonId;
            return View("QuizResult", resultViewModel);
        }

        // GET: Courses/FinalAssignment/5
        public async Task<IActionResult> FinalAssignment(Guid id)
        {
            // Get the course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Check if the user has completed and purchased the course
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == id);

            bool hasCompletedCourse = enrollment?.Status == EnrollmentStatus.Completed;
            bool hasPurchasedCourse = enrollment?.IsPurchased ?? false;

            // Get all assignments for the course
            var assignments = await _context.Assignments
                .Include(a => a.Options)
                .Where(a => a.CourseID == id && a.LessonId == null) // Get only course-level assignments
                .ToListAsync();

            if (assignments == null || !assignments.Any())
            {
                // If no course-level assignments, get all Lesson assignments
                assignments = await _context.Assignments
                    .Include(a => a.Options)
                    .Where(a => a.CourseID == id)
                    .ToListAsync();
            }

            // Create the view model
            var viewModel = new FinalAssignmentViewModel
            {
                CourseID = id,
                CourseName = course.CourseName,
                HasCompletedCourse = hasCompletedCourse,
                HasPurchasedCourse = hasPurchasedCourse,
                ErrorMessage = !hasCompletedCourse ? "You must complete the course before taking the final assignment." :
                              !hasPurchasedCourse ? "You must purchase the course before taking the final assignment." : null,
                Assignments = assignments.Select(a => new AssignmentWithOptions
                {
                    AssignmentID = a.TestID,
                    Title = a.Title,
                    Question = a.Question,
                    Options = a.Options.Select(o => new Models.ViewModel.AssignmentOptionViewModel
                    {
                        OptionID = o.OptionID,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Courses/SubmitFinalAssignment
        [HttpPost]
        public async Task<IActionResult> SubmitFinalAssignment(Guid courseId, Dictionary<string, string> answers)
        {
            // Get the course
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Check if the user has completed and purchased the course
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == courseId);

            if (enrollment == null || enrollment.Status != EnrollmentStatus.Completed || !enrollment.IsPurchased)
            {
                return BadRequest("You must complete and purchase the course before submitting the final assignment.");
            }

            // Calculate the score
            int correctAnswers = 0;
            int totalQuestions = answers.Count - 1;

            int counter = 0;

            foreach (var answer in answers)
            {
                counter++;
                if (counter == 7)
                    break;
                var assignmentId = Guid.Parse(answer.Key);
                var selectedOptionId = Guid.Parse(answer.Value);

                // Get the assignment and its options
                var assignment = await _context.Assignments
                    .Include(a => a.Options)
                    .FirstOrDefaultAsync(a => a.TestID == assignmentId);

                if (assignment == null)
                {
                    continue;
                }

                // Check if the selected option is correct
                var selectedOption = assignment.Options.FirstOrDefault(o => o.OptionID == selectedOptionId);

                if (selectedOption != null)
                {
                    // Record the result
                    var result = new UserAssignmentResult
                    {
                        ResultID = Guid.NewGuid(),
                        UserID = user.UserID,
                        TestID = assignmentId,
                        SelectedOptionID = selectedOptionId,
                        IsCorrect = selectedOption.IsCorrect,
                        SubmissionDate = DateTime.Now
                    };

                    await _context.UserAssignmentResults.AddAsync(result);

                    if (selectedOption.IsCorrect)
                    {
                        correctAnswers++;
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Calculate the percentage
            int percentage = totalQuestions > 0 ? (correctAnswers * 100) / totalQuestions : 0;
            ViewBag.FinalScore = percentage;
            // Determine if the user passed (70% or higher)
            bool passed = percentage >= 70;

            // If the user passed, create a certificate
            if (passed && course.IsCertified)
            {
                var existingCertificate = await _context.Certifications
                    .FirstOrDefaultAsync(c => c.UserID == user.UserID && c.CourseID == courseId);

                if (existingCertificate == null)
                {
                    var certificate = new Certification
                    {
                        CertificationID = Guid.NewGuid(),
                        UserID = user.UserID,
                        CourseID = courseId,
                        CertificationDate = DateTime.Now,
                        CertificateNumber = $"CERT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
                    };

                    await _context.Certifications.AddAsync(certificate);
                    await _context.SaveChangesAsync();

                    ViewBag.CertificationId = certificate.CertificationID;
                }
                else
                    ViewBag.CertificationId = existingCertificate.CertificationID;

            }


            // Create the result view model
            var resultViewModel = new QuizResultViewModel
            {
                QuizId = 0, // Not applicable for final assignment
                CourseId = courseId,
                LessonId = 0, // Not applicable for final assignment
                CorrectAnswers = correctAnswers,
                TotalQuestions = totalQuestions,
                Percentage = percentage,
                Passed = passed,
                IsFinalAssignment = true
            };

            ViewData["CourseGuid"] = courseId;

            return View("FinalAssignmentResult", resultViewModel);
        }
    }
}

