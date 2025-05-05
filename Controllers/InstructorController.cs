using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;
using ELearningPlatform.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ELearningPlatform.Models.ViewModel;
using Microsoft.AspNetCore.Routing.Matching;

namespace ELearningPlatform.Controllers
{
    //[Authorize(Roles = "Instructor,Admin")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Instructor/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }

            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            var courses = await _context.Courses
                .Where(c => c.InstructorID == instructorId)
                .ToListAsync();

            // Get total students enrolled in instructor's courses
            var totalStudents = await _context.Enrollments
                .Where(uc => courses.Select(c => c.CourseID).Contains(uc.CourseID))
                .Select(uc => uc.UserID)
                .Distinct()
                .CountAsync();

            // Get total lessons across all instructor's courses
            var totalLessons = await _context.Lessons
                .Where(e => courses.Select(c => c.CourseID).Contains(e.CourseID))
                .CountAsync();

            // Get average rating across all instructor's courses
            double averageRating = 0;
            if (courses.Any())
            {
                averageRating = courses.Where(c => c.AverageRating > 0).Select(c => c.AverageRating).DefaultIfEmpty(0).Average();
            }

            // Get student count per course
            var courseStudents = await _context.Enrollments
                .Where(uc => courses.Select(c => c.CourseID).Contains(uc.CourseID))
                .GroupBy(uc => uc.CourseID)
                .Select(g => new { CourseID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseID, x => x.Count);

            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalLessons = totalLessons;
            ViewBag.AverageRating = averageRating.ToString("0.0");
            ViewBag.CourseStudents = courseStudents;

            return View(courses);
        }

        // GET: Instructor/ManageCourse/5
        public async Task<IActionResult> ManageCourse(Guid id)
        {

            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));

            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.CourseID == id && c.InstructorID == instructorId);

            if (course == null)
            {
                return NotFound();
            }

            var viewModel = new InstructorCourseViewModel
            {
                CourseID = course.CourseID,
                CourseName = course.CourseName,
                Description = course.Description,
                Level = course.Level,
                Language = course.Language,
                Price = course.Price,
                ImageUrl = course.ImageUrl,
                DurationInHours = course.DurationInHours,
                IsCertified = course.IsCertified,
                Lessons = course.Lessons
                    .OrderBy(e => e.Order)
                    .Select(e => new LessonViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        VideoUrl = e.VideoUrl,
                        DurationInMinutes = e.DurationInMinutes,
                        Order = e.Order,
                        HasQuiz = _context.Assignments.Any(a => a.LessonId == e.Id)
                    }).ToList()
            };

            // Get assignments (quizzes) for each lesson
            foreach (var Lesson in viewModel.Lessons)
            {
                var quiz = await _context.Assignments
                    .Include(a => a.Options)
                    .FirstOrDefaultAsync(a => a.LessonId == Lesson.Id);

                if (quiz != null)
                {
                    Lesson.Quiz = new QuizViewModel
                    {
                        Id = quiz.TestID,
                        Title = quiz.Title,
                        Question = quiz.Question,
                        Options = quiz.Options.Select(o => new QuizOptionViewModel
                        {
                            Id = o.OptionID,
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect
                        }).ToList(),
                        CorrectOptionIndex = quiz.Options.ToList().FindIndex(o => o.IsCorrect)
                    };
                }
            }

            // Get final assignment
            var finalAssignments = await _context.Assignments
                .Include(a => a.Options)
                .Where(a => a.CourseID == id && a.LessonId == null)
                .ToListAsync();

            // did mapping between two calss
            viewModel.Assignments = finalAssignments.Select(a => new AssignmentViewModel
            {
                TestID = a.TestID,
                Title = a.Title,
                Question = a.Question,
                Options = a.Options.Select(o => new Models.AssignmentOptionViewModel
                {
                    OptionID = o.OptionID,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList(),
                CorrectOptionIndex = a.Options.ToList().FindIndex(o => o.IsCorrect)
            }).ToList();

            return View(viewModel);
        }

        // GET: Instructor/AddLesson/5
        public async Task<IActionResult> AddLesson(Guid id)
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id && c.InstructorID == instructorId);

            if (course == null)
            {
                return NotFound();
            }

            ViewBag.CourseID = id;
            ViewBag.CourseName = course.CourseName;

            // Determine the next order number
            int nextOrder = 1;
            var lastLesson = await _context.Lessons
                .Where(e => e.CourseID == id)
                .OrderByDescending(e => e.Order)
                .FirstOrDefaultAsync();

            if (lastLesson != null)
            {
                nextOrder = lastLesson.Order + 1;
            }

            var viewModel = new LessonViewModel
            {
                Order = nextOrder,
                Quiz = new QuizViewModel
                {
                    Options = new List<QuizOptionViewModel>
            {
                new QuizOptionViewModel(),
                new QuizOptionViewModel(),
                new QuizOptionViewModel(),
                new QuizOptionViewModel()
            }
                }
            };

            return View(viewModel);
        }

        // POST: Instructor/AddLesson/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLesson(Guid id, LessonViewModel model)
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id && c.InstructorID == instructorId);

            if (course == null)
            {
                return NotFound();
            }

            ModelState.Remove("id");

            if (ModelState.IsValid)
            {
                // Create new lesson
                var lesson = new Lessons
                {
                    CourseID = id,
                    Title = model.Title,
                    Description = model.Description,
                    VideoUrl = model.VideoUrl,
                    DurationInMinutes = model.DurationInMinutes,
                    Order = model.Order
                };

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                if (model.Quiz != null)
                {
                    var quiz = new Assignment
                    {
                        TestID = Guid.NewGuid(),
                        CourseID = id,
                        LessonId = lesson.Id,
                        Title = model.Quiz.Title,
                        Question = model.Quiz.Question,
                        Options = new List<AssignmentOption>()
                    };

                    // Add options
                    for (int i = 0; i < model.Quiz.Options.Count; i++)
                    {
                        var option = model.Quiz.Options[i];
                        quiz.Options.Add(new AssignmentOption
                        {
                            OptionID = Guid.NewGuid(),
                            TestID = quiz.TestID,
                            OptionText = option.OptionText,
                            IsCorrect = i == model.Quiz.CorrectOptionIndex
                        });
                    }

                    _context.Assignments.Add(quiz);
                    await _context.SaveChangesAsync();
                }

                // Update course duration
                course.DurationInHours = await _context.Lessons
                    .Where(e => e.CourseID == id)
                    .SumAsync(e => e.DurationInMinutes) / 60;

                _context.Update(course);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ManageCourse), new { id });
            }

            ViewBag.CourseID = id;
            ViewBag.CourseName = course.CourseName;

            return View(model);
        }


        // POST: Instructor/DeleteQuiz/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuiz(Guid id)
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            var quiz = await _context.Assignments
                .Include(a => a.Options)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.TestID == id && a.Course.InstructorID == instructorId);

            if (quiz == null)
            {
                return NotFound();
            }

            // First, find and delete any user assignment results that reference these options
            var optionIds = quiz.Options.Select(o => o.OptionID).ToList();
            var userResults = await _context.UserAssignmentResults
                .Where(r => r.TestID == quiz.TestID || optionIds.Contains(r.SelectedOptionID))
                .ToListAsync();

            if (userResults.Any())
            {
                _context.UserAssignmentResults.RemoveRange(userResults);
                await _context.SaveChangesAsync();
            }

            var courseId = quiz.CourseID;

            // Delete options
            _context.AssignmentOptions.RemoveRange(quiz.Options);

            // Delete quiz1
            _context.Assignments.Remove(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageCourse), new { id = courseId });
        }



        // POST: Instructor/DeleteLesson/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            var lesson = await _context.Lessons
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id && e.Course.InstructorID == instructorId);

            if (lesson == null)
            {
                return NotFound();
            }

            var courseId = lesson.CourseID;

            // Delete associated quiz1 if exists
            var quiz = await _context.Assignments
                .Include(a => a.Options)
                .FirstOrDefaultAsync(a => a.LessonId == id);

            // First, find and delete any user assignment results that reference these options
            var optionIds = quiz?.Options.Select(o => o.OptionID).ToList();

            if (quiz == null)
                goto jump;

            var userResults = await _context.UserAssignmentResults
                .Where(r => r.TestID == quiz.TestID || optionIds.Contains(r.SelectedOptionID))
                .ToListAsync();

            if (userResults.Any())
            {
                _context.UserAssignmentResults.RemoveRange(userResults);
                await _context.SaveChangesAsync();
            }

            if (quiz != null)
            {
                // Delete quiz1 options first
                _context.AssignmentOptions.RemoveRange(quiz.Options);

                // Then delete the quiz1
                _context.Assignments.Remove(quiz);
            }
        jump:
            // Delete the lesson
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            // Update course duration
            var course = await _context.Courses.FindAsync(courseId);
            if (course != null)
            {
                course.DurationInHours = await _context.Lessons
                    .Where(e => e.CourseID == courseId)
                    .SumAsync(e => e.DurationInMinutes) / 60;

                _context.Update(course);
                await _context.SaveChangesAsync();
            }

            // Reorder remaining episodes if needed
            var remainingLessons = await _context.Lessons
                .Where(e => e.CourseID == courseId)
                .OrderBy(e => e.Order)
                .ToListAsync();

            for (int i = 0; i < remainingLessons.Count; i++)
            {
                if (remainingLessons[i].Order != i + 1)
                {
                    remainingLessons[i].Order = i + 1;
                    _context.Update(remainingLessons[i]);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Lesson '{lesson.Title}' has been deleted successfully.";

            return RedirectToAction(nameof(ManageCourse), new { id = courseId });
        }

        // GET: Instructor/EditLessonAndQuiz/5
        public async Task<IActionResult> EditLesson(int id)
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            var lesson = await _context.Lessons
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id && e.Course.InstructorID == instructorId);

            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.CourseID = lesson.CourseID;
            ViewBag.CourseName = lesson.Course.CourseName;

            var viewModel = new LessonViewModel
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                VideoUrl = lesson.VideoUrl,
                DurationInMinutes = lesson.DurationInMinutes,
                Order = lesson.Order
            };

            // Get quiz1 if exists
            var quiz = await _context.Assignments
                .Include(a => a.Options)
                .FirstOrDefaultAsync(a => a.LessonId == id);

            if (quiz != null)
            {
                viewModel.HasQuiz = true;
                viewModel.Quiz = new QuizViewModel
                {
                    Id = quiz.TestID,
                    Title = quiz.Title,
                    Question = quiz.Question,
                    Options = quiz.Options.Select(o => new QuizOptionViewModel
                    {
                        Id = o.OptionID,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList(),
                    CorrectOptionIndex = quiz.Options.ToList().FindIndex(o => o.IsCorrect)
                };
            }
            else
            {
                viewModel.Quiz = new QuizViewModel
                {
                    Options = new List<QuizOptionViewModel>
            {
                new QuizOptionViewModel(),
                new QuizOptionViewModel(),
                new QuizOptionViewModel(),
                new QuizOptionViewModel()
            }
                };
            }

            return View(viewModel);
        }

        // POST: Instructor/EditLessonAndQuiz/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLesson(int id, LessonViewModel model)
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            var lesson = await _context.Lessons
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id && e.Course.InstructorID == instructorId);

            if (lesson == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update Lesson
                lesson.Title = model.Title;
                lesson.Description = model.Description;
                lesson.VideoUrl = model.VideoUrl;
                lesson.DurationInMinutes = model.DurationInMinutes;
                lesson.Order = model.Order;

                _context.Update(lesson);
                await _context.SaveChangesAsync();

                // Handle quiz1
                var existingQuiz = await _context.Assignments
                    .Include(a => a.Options)
                    .FirstOrDefaultAsync(a => a.LessonId == id);

                if (model.HasQuiz && model.Quiz != null)
                {

                    if (existingQuiz == null)
                    {
                        goto jump;
                    }

                    var quiz = await _context.Assignments
                .Include(a => a.Options)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.TestID == existingQuiz.TestID && a.Course.InstructorID == instructorId);

                    

                    // First, find and delete any user assignment results that reference these options
                    var optionIds = quiz.Options.Select(o => o.OptionID).ToList();
                    var userResults = await _context.UserAssignmentResults
                        .Where(r => r.TestID == quiz.TestID || optionIds.Contains(r.SelectedOptionID))
                        .ToListAsync();

                    if (userResults.Any())
                    {
                        _context.UserAssignmentResults.RemoveRange(userResults);
                        await _context.SaveChangesAsync();
                    }

                    var courseId = quiz.CourseID;

                    // Delete options
                    _context.AssignmentOptions.RemoveRange(quiz.Options);

                    // Delete quiz1
                    _context.Assignments.Remove(quiz);
                    await _context.SaveChangesAsync();
                jump:
                    if (existingQuiz != null)
                    {
                        var quiz1 = new Assignment
                        {
                            TestID = Guid.NewGuid(),
                            CourseID = lesson.CourseID,
                            LessonId = lesson.Id,
                            Title = model.Quiz.Title,
                            Question = model.Quiz.Question,
                            Options = new List<AssignmentOption>()
                        };

                        // Add options
                        for (int i = 0; i < model.Quiz.Options.Count; i++)
                        {
                            var option = model.Quiz.Options[i];
                            quiz1.Options.Add(new AssignmentOption
                            {
                                OptionID = Guid.NewGuid(),
                                TestID = quiz1.TestID,
                                OptionText = option.OptionText,
                                IsCorrect = i == model.Quiz.CorrectOptionIndex
                            });
                        }

                        _context.Assignments.Add(quiz1);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // Create new quiz1
                        var quiz1 = new Assignment
                        {
                            TestID = Guid.NewGuid(),
                            CourseID = lesson.CourseID,
                            LessonId = lesson.Id,
                            Title = model.Quiz.Title,
                            Question = model.Quiz.Question,
                            Options = new List<AssignmentOption>()
                        };

                        // Add options
                        for (int i = 0; i < model.Quiz.Options.Count; i++)
                        {
                            var option = model.Quiz.Options[i];
                            quiz1.Options.Add(new AssignmentOption
                            {
                                OptionID = Guid.NewGuid(),
                                TestID = quiz1.TestID,
                                OptionText = option.OptionText,
                                IsCorrect = i == model.Quiz.CorrectOptionIndex
                            });
                        }

                        _context.Assignments.Add(quiz1);
                    }
                }
                else if (existingQuiz != null)
                {
                    // Remove quiz1 if it exists but is no longer needed
                    _context.AssignmentOptions.RemoveRange(existingQuiz.Options);
                    _context.Assignments.Remove(existingQuiz);
                }

                await _context.SaveChangesAsync();

                // Update course duration
                var course = lesson.Course;
                course.DurationInHours = await _context.Lessons
                    .Where(e => e.CourseID == course.CourseID)
                    .SumAsync(e => e.DurationInMinutes) / 60;

                _context.Update(course);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ManageCourse), new { id = lesson.CourseID });
            }

            ViewBag.CourseID = lesson.CourseID;
            ViewBag.CourseName = lesson.Course.CourseName;

            return View(model);
        }

        // GET: Instructor/Students
        public async Task<IActionResult> Students()
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            // Get courses taught by this instructor
            var courses = await _context.Courses
                .Where(c => c.InstructorID == instructorId)
                .Select(c => c.CourseID)
                .ToListAsync();

            // Get students enrolled in these courses
            var students = await _context.Enrollments
                .Where(uc => courses.Contains(uc.CourseID))
                .Include(uc => uc.User)
                .Include(uc => uc.Course)
                .OrderByDescending(uc => uc.EnrollmentDate)
                .ToListAsync();

            return View(students);
        }

        // GET: Instructor/Analytics
        public async Task<IActionResult> Analytics()
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            // Get courses taught by this instructor
            var courses = await _context.Courses
                .Where(c => c.InstructorID == instructorId)
                .ToListAsync();

            // Get enrollment data by course
            var enrollmentData = await _context.Enrollments
                .Where(uc => courses.Select(c => c.CourseID).Contains(uc.CourseID))
                .GroupBy(uc => uc.CourseID)
                .Select(g => new { CourseID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseID, x => x.Count);

            var sumEn = enrollmentData.Values.Sum();

            // Get completion data by course
            var completionData = await _context.Enrollments
                .Where(uc => courses.Select(c => c.CourseID).Contains(uc.CourseID) && uc.CompletedDate != null)
                .GroupBy(uc => uc.CourseID)
                .Select(g => new { CourseID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseID, x => x.Count);

            var sumCom = completionData.Values.Sum();

            // Get rating data by course
            var ratingData = courses.ToDictionary(c => c.CourseID, c => c.AverageRating);

            ViewBag.EnrollmentData = enrollmentData;
            ViewBag.CompletionData = completionData;
            ViewBag.RatingData = ratingData;
            ViewBag.SumEnrollments = sumEn;
            ViewBag.SumCompletionData = sumCom;


            return View(courses);
        }

        // GET: Instructor/Reviews
        public async Task<IActionResult> Reviews()
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            // Get courses taught by this instructor
            var courses = await _context.Courses
                .Where(c => c.InstructorID == instructorId)
                .Select(c => c.CourseID)
                .ToListAsync();

            // Get reviews for these courses
            var reviews = await _context.Reviews
                .Where(r => courses.Contains(r.CourseID))
                .Include(r => r.User)
                .Include(r => r.Course)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // GET: Instructor/Courses
        public async Task<IActionResult> Courses()
        {
            if (CheckSessoin())
            {
                return RedirectToAction("Login", "Account");
            }
            var instructorId = Guid.Parse(HttpContext.Session.GetString("UserID"));


            var courses = await _context.Courses
                .Where(c => c.InstructorID == instructorId)
                .OrderByDescending(c => c.Price)
                .ToListAsync();

            // Get student count for each course
            var courseStudents = await _context.Enrollments
                .Where(uc => courses.Select(c => c.CourseID).Contains(uc.CourseID))
                .GroupBy(uc => uc.CourseID)
                .Select(g => new { CourseID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CourseID, x => x.Count);

            ViewBag.CourseStudents = courseStudents;

            return View(courses);
        }
        public bool CheckSessoin()
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == HttpContext.Session.GetString("UserEmail") && u.Role == UserRole.Instructor);
            return user == null;

        }
    }
}
