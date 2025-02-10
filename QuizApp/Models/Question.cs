namespace QuizApp.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }        
        public QuestionState State { get; set; }
        public Category Category { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();    
    }
}
