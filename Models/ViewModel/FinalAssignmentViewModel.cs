using System;
using System.Collections.Generic;

namespace ELearningPlatform.Models.ViewModel
{
    public class FinalAssignmentViewModel
    {
        public Guid CourseID { get; set; }
        public string CourseName { get; set; }
        public List<AssignmentWithOptions> Assignments { get; set; }
        public bool HasCompletedCourse { get; set; }
        public bool HasPurchasedCourse { get; set; }
        public bool CanTakeFinalAssignment => HasCompletedCourse && HasPurchasedCourse;
        public string ErrorMessage { get; set; }
    }

    public class AssignmentWithOptions
    {
        public Guid AssignmentID { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public List<AssignmentOptionViewModel> Options { get; set; }
        public int? SelectedOptionId { get; set; }
    }

    public class AssignmentOptionViewModel
    {
        public Guid OptionID { get; set; }
        public string OptionText { get; set; }

        public bool IsCorrect { get; set; }
    }
}

