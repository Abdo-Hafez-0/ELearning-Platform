namespace ELearningPlatform.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string Title { get; set; }
        public List<QuizQuestion> Questions { get; set; }
    }

    public class QuizQuestion 
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Question { get; set; }
        public List<QuizOption> Options { get; set; }
        public int CorrectOptionId { get; set; }
    }

    public class QuizOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Text { get; set; }
    }
}

