using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;
using System.Collections.Specialized;
using ELearningPlatform.Models.ViewModel;

namespace ELearningPlatform.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reviews/Rate/5
        public async Task<IActionResult> Rate(Guid id)
        {
            // Get the course
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Check if the user has completed the course
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserID == user.UserID && e.CourseID == id);

            if (enrollment == null)
            {
                TempData["ErrorMessage"] = "You must be enrolled in this course to rate it.";
                return RedirectToAction("Index", "MyLearning");
            }

            if (enrollment.Status != EnrollmentStatus.Completed)
            {
                TempData["ErrorMessage"] = "You must complete the course before you can rate it.";
                return RedirectToAction("Index", "MyLearning");
            }

            // Check if the user has already reviewed the course
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserID == user.UserID && r.CourseID == id);

            // Create the view model
            var viewModel = new RateViewModel
            {
                CourseID = course.CourseID,
                CourseName = course.CourseName,
                CourseImageUrl = course.ImageUrl,
                InstructorName = course.Instructor != null
                    ? $"{course.Instructor.FirstName} {course.Instructor.LastName}"
                    : "Unknown Instructor",
                IsEdit = existingReview != null,
                Rating = existingReview?.Rating ?? 0,
                Comment = existingReview?.Comment ?? string.Empty
            };

            return View(viewModel);
        }

        // POST: Reviews/Rate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(RateViewModel model)
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


                // Check if the user has already reviewed the course
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserID == user.UserID && r.CourseID == model.CourseID);

                if (existingReview != null)
                {
                    // Update existing review
                    existingReview.Rating = model.Rating;
                    existingReview.Comment = model.Comment;
                    _context.Reviews.Update(existingReview);
                }
                else
                {
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
                }


                await _context.SaveChangesAsync();

                // Update the course's average rating and rating count
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseID == model.CourseID);

                if (course != null)
                {
                    // Get all reviews for the course
                    var reviews = await _context.Reviews
                        .Where(r => r.CourseID == model.CourseID)
                        .ToListAsync();

                    // Calculate the new average rating
                    if (reviews.Any())
                    {
                        course.AverageRating = reviews.Average(r => r.Rating);
                        course.RatingCount = reviews.Count;
                    }
                    else
                    {
                        // If there are no reviews (which shouldn't happen at this point)
                        course.AverageRating = model.Rating;
                        course.RatingCount = 1;
                    }

                    _context.Courses.Update(course);
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = existingReview != null
                    ? "Your review has been updated successfully."
                    : "Your review has been submitted successfully.";

                return RedirectToAction("MyReviews");
            }

            return View(model);
        }

        // GET: Reviews/MyReviews
        public async Task<IActionResult> MyReviews()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            // Get the user's reviews
            var reviews = await _context.Reviews
                .Include(r => r.Course)
                .Where(r => r.UserID == user.UserID)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            var review = await _context.Reviews
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.ReviewID == id);

            if (review == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            if (user == null || review.UserID != user.UserID)
            {
                return Forbid();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewID == id);

            if (review == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
            {
                return RedirectToAction("Login", "Account");
            }
            // Get the current user (in a real app, this would come from authentication)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == HttpContext.Session.GetString("UserEmail"));


            if (user == null || review.UserID != user.UserID)
            {
                return Forbid();
            }

            _context.Reviews.Remove(review);

            // Update the course's average rating and rating count
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == review.CourseID);

            if (course != null)
            {
                // Get all remaining reviews for the course
                var reviews = await _context.Reviews
                    .Where(r => r.CourseID == review.CourseID && r.ReviewID != id)
                    .ToListAsync();

                // Calculate the new average rating
                if (reviews.Any())
                {
                    course.AverageRating = reviews.Average(r => r.Rating);
                    course.RatingCount = reviews.Count;
                }

                else
                {
                    // If there are no reviews left
                    course.AverageRating = 0;
                    course.RatingCount = 0;
                }

                _context.Courses.Update(course);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your review has been deleted successfully.";

            return RedirectToAction("MyReviews");
        }
    }
}
