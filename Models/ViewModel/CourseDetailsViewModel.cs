namespace ELearningPlatform.Models.ViewModel
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; }
        public List<Lessons> Lessons { get; set; }
        public Lessons CurrentLesson { get; set; }
        public int? PreviousLessonId { get; set; }
        public int? NextLessonId { get; set; }
        public Quiz LessonQuiz { get; set; }
        public List<Review> Reviews { get; set; }

        // Added properties to track enrollment status
        public bool IsEnrolled { get; set; }
        public bool IsPurchased { get; set; }
        public bool IsCompleted { get; set; }

        // Helper property to check if current Lesson is the last one
        public bool IsLastLesson => !NextLessonId.HasValue;
    }
}

