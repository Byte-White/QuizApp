namespace QuizApp.Models
{
    public class UserData
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        
        public List<Question> CreatedQuestions { get; set; } = new List<Question>();
        public List<Question> CorrectlyAnsweredQuestions { get; set; } = new List<Question>();
    }
}
