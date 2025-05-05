using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ELearningPlatform.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ELearningPlatform.Models
{
    public class InstructorCourseViewModel
    {
        public Guid CourseID { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public CourseLevel Level { get; set; }
        public string Language { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int DurationInHours { get; set; }
        public bool IsCertified { get; set; }
        
        public List<LessonViewModel> Lessons { get; set; }
        public List<AssignmentViewModel> Assignments { get; set; }
        public FinalAssignmentViewModel FinalAssignment { get; set; }
        
        public InstructorCourseViewModel()
        {
            Lessons = new List<LessonViewModel>();
            Assignments = new List<AssignmentViewModel>();
        }
    }
    
    public class LessonViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Video URL is required")]
        public string VideoUrl { get; set; }
        
        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 300, ErrorMessage = "Duration must be between 1 and 300 minutes")]
        public int DurationInMinutes { get; set; }
        
        public int Order { get; set; }
        
        public bool HasQuiz { get; set; }
        
        public QuizViewModel Quiz { get; set; }
    }
    
    public class QuizViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Question is required")]
        public string Question { get; set; }
        
        public List<QuizOptionViewModel> Options { get; set; } = new List<QuizOptionViewModel>();
        
        public int CorrectOptionIndex { get; set; }
    }
    
    public class QuizOptionViewModel
    {
        public Guid Id { get; set; }
        
        [Required(ErrorMessage = "Option text is required")]
        public string OptionText { get; set; }
        
        public bool IsCorrect { get; set; }
    }

    public class AssignmentViewModel
    {
        public Guid TestID { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Question is required")]
        public string Question { get; set; }

        public List<AssignmentOptionViewModel> Options { get; set; } = new List<AssignmentOptionViewModel>();

        public int CorrectOptionIndex { get; set; }
    }

    public class AssignmentOptionViewModel
    {
        public Guid OptionID { get; set; }

        [Required(ErrorMessage = "Option text is required")]
        public string OptionText { get; set; }

        public bool IsCorrect { get; set; }
    }

    //public class FinalAssignmentViewModel
    //{
    //    public Guid TestID { get; set; }

    //    [Required(ErrorMessage = "Title is required")]
    //    public string Title { get; set; }

    //    public List<AssignmentViewModel> Questions { get; set; } = new List<AssignmentViewModel>();
    //}
}
