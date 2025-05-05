using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELearningPlatform.Data;
using ELearningPlatform.Models;

namespace ELearningPlatform.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Login(string email, string password)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == email && a.Password == password);


            // Update last login time
            admin.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            // Store admin info in session
            HttpContext.Session.SetString("AdminId", admin.Id.ToString());
            HttpContext.Session.SetString("AdminUsername", admin.Username);

            return RedirectToAction("Dashboard");
        }

        public bool CheckSession()
        {
            return string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId"));
        }  

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login","Account");
            }

            ViewBag.TotalStudents = await _context.Users.CountAsync(u=> u.Role == UserRole.Student);
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
            ViewBag.TotalCertifications = await _context.Certifications.CountAsync();

            // Get recent courses
            ViewBag.RecentEnrollments = await _context.Enrollments
                .Include(c => c.User)
                .Include(c => c.Course)
                .OrderByDescending(c => c.EnrollmentDate)
                .Take(5)
                .ToListAsync();

            //Get popular courses
            ViewBag.PopularCourses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.Enrollments.Count)
                .Take(3)
                .ToListAsync();

            return View();
        }

        // GET: Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        #region Course Management

        // GET: Admin/Courses
        public async Task<IActionResult> Courses()
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            return View(courses);
        }

        // GET: Admin/CourseDetails/5
        public async Task<IActionResult> CourseDetails(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Admin/CreateCourse
        public async Task<IActionResult> CreateCourse()
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            // Get instructors for dropdown
            var instructors = await _context.Users
                .Where(u => u.Role == UserRole.Instructor)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            ViewBag.Instructors = instructors;

            return View();
        }

        // POST: Admin/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            ModelState.Remove("Instructor");
            if (ModelState.IsValid)
            {
                course.CourseID = Guid.NewGuid();
                _context.Add(course);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Course created successfully.";
                return RedirectToAction("Courses");
            }

            // Get instructors for dropdown
            var instructors = await _context.Users
                .Where(u => u.Role == UserRole.Instructor)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            ViewBag.Instructors = instructors;

            return View(course);
        }

        // GET: Admin/EditCourse/5
        public async Task<IActionResult> EditCourse(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            // Get instructors for dropdown
            var instructors = await _context.Users
                .Where(u => u.Role == UserRole.Instructor)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            ViewBag.Instructors = instructors;

            return View(course);
        }

        // POST: Admin/EditCourse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(Guid id, Course course)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != course.CourseID)
            {
                return NotFound();
            }


            ModelState.Remove("Instructor");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Course updated successfully.";
                    return RedirectToAction("Courses");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Get instructors for dropdown
            var instructors = await _context.Users
                .Where(u => u.Role == UserRole.Instructor)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            ViewBag.Instructors = instructors;

            return View(course);
        }

        // GET: Admin/DeleteCourse/5
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseID == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Admin/DeleteCourse/5
        [HttpPost, ActionName("DeleteCourse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourseConfirmed(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Course deleted successfully.";
            }

            return RedirectToAction("Courses");
        }

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }

        #endregion

        #region Student Management

        // GET: Admin/Students
        public async Task<IActionResult> Students()
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var students = await _context.Users
                .Where(u => u.Role == UserRole.Student)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Include(u => u.Certifications)
                .Include(u => u.Enrollments)
                .ToListAsync();
            
            
            return View(students);
        }

        // GET: Admin/StudentDetails/5
        public async Task<IActionResult> StudentDetails(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _context.Users
                .Include(u => u.Enrollments)
                .ThenInclude(e => e.Course)
                .Include(u => u.Certifications)
                .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(u => u.UserID == id && u.Role == UserRole.Student);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Admin/CreateStudent
        public IActionResult CreateStudent()
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Admin/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(User student)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            // Ensure the user is a student
            student.Role = UserRole.Student;

            if (ModelState.IsValid)
            {
                // Check if email is already in use
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == student.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already in use.");
                    return View(student);
                }

                student.UserID = Guid.NewGuid();
                student.Username = student.Email.Split('@')[0]; // Generate username from email

                _context.Add(student);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Student created successfully.";
                return RedirectToAction("Students");
            }

            return View(student);
        }

        // GET: Admin/EditStudent/5
        public async Task<IActionResult> EditStudent(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == id && u.Role == UserRole.Student);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Admin/EditStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(Guid id, User student)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != student.UserID)
            {
                return NotFound();
            }

            //DateTime minDate = DateTime.Today.AddYears(-10);

            //if (student.BirthDate > minDate)
            //{
            //    ModelState.AddModelError("BirthDate", "You must be at least 10 years old.");
            //}

            // Ensure the user is a student
            student.Role = UserRole.Student;
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email is already in use by another user
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == student.Email && u.UserID != student.UserID);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "This email is already in use by another user.");
                        return View(student);
                    }

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Student updated successfully.";
                    return RedirectToAction("Students");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.UserID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(student);
        }

        // GET: Admin/DeleteStudent/5
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.UserID == id && u.Role == UserRole.Student);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Admin/DeleteStudent/5
        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudentConfirmed(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var student = await _context.Users.FindAsync(id);
            if (student != null && student.Role == UserRole.Student)
            {
                _context.Users.Remove(student);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Student deleted successfully.";
            }

            return RedirectToAction("Students");
        }

        private bool StudentExists(Guid id)
        {
            return _context.Users.Any(e => e.UserID == id && e.Role == UserRole.Student);
        }

        #endregion

        #region Admin Management

        // GET: Admin/Admins
        public async Task<IActionResult> Admins()
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var admins = await _context.Admins
                .OrderBy(a => a.Username)
                .ToListAsync();

            return View(admins);
        }

        // GET: Admin/CreateAdmin
        public IActionResult CreateAdmin()
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(Admin admin)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Check if username is already in use
                var existingAdmin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Username == admin.Username);

                if (existingAdmin != null)
                {
                    ModelState.AddModelError("Username", "This username is already in use.");
                    return View(admin);
                }

                // Check if email is already in use
                existingAdmin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Email == admin.Email);

                if (existingAdmin != null)
                {
                    ModelState.AddModelError("Email", "This email is already in use.");
                    return View(admin);
                }

                admin.Id = Guid.NewGuid();
                admin.CreatedAt = DateTime.Now;

                _context.Add(admin);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Admin created successfully.";
                return RedirectToAction("Admins");
            }

            return View(admin);
        }

        // GET: Admin/EditAdmin/5
        public async Task<IActionResult> EditAdmin(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admin/EditAdmin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdmin(Guid id, Admin admin)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != admin.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if username is already in use by another admin
                    var existingAdmin = await _context.Admins
                        .FirstOrDefaultAsync(a => a.Username == admin.Username && a.Id != admin.Id);

                    if (existingAdmin != null)
                    {
                        ModelState.AddModelError("Username", "This username is already in use by another admin.");
                        return View(admin);
                    }

                    // Check if email is already in use by another admin
                    existingAdmin = await _context.Admins
                        .FirstOrDefaultAsync(a => a.Email == admin.Email && a.Id != admin.Id);

                    if (existingAdmin != null)
                    {
                        ModelState.AddModelError("Email", "This email is already in use by another admin.");
                        return View(admin);
                    }

                    // Preserve the CreatedAt date
                    var originalAdmin = await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    admin.CreatedAt = originalAdmin.CreatedAt;

                    _context.Update(admin);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Admin updated successfully.";
                    return RedirectToAction("Admins");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(admin.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(admin);
        }

        // GET: Admin/DeleteAdmin/5
        public async Task<IActionResult> DeleteAdmin(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            // Don't allow deleting the current admin
            if (HttpContext.Session.GetString("AdminId") == id.ToString())
            {
                TempData["ErrorMessage"] = "You cannot delete your own admin account.";
                return RedirectToAction("Admins");
            }

            return View(admin);
        }

        // POST: Admin/DeleteAdmin/5
        [HttpPost, ActionName("DeleteAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdminConfirmed(Guid id)
        {
            // Check if admin is logged in
            if (CheckSession())
            {
                return RedirectToAction("Login", "Account");
            }

            // Don't allow deleting the current admin
            if (HttpContext.Session.GetString("AdminId") == id.ToString())
            {
                TempData["ErrorMessage"] = "You cannot delete your own admin account.";
                return RedirectToAction("Admins");
            }

            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Admin deleted successfully.";
            }

            return RedirectToAction("Admins");
        }

        private bool AdminExists(Guid id)
        {
            return _context.Admins.Any(e => e.Id == id);
        }

        #endregion
    }
}
