namespace ELearningPlatform.Models.ViewModel
{
    public class QuizResultViewModel
    {
        public int QuizId { get; set; }
        public Guid CourseId { get; set; }
        public int LessonId { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int Percentage { get; set; }
        public bool Passed { get; set; }
        public bool IsFinalAssignment { get; set; } = false;

    }
}

