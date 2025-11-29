namespace ShulteTestAutomation.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public string SubjectId { get; set; } // Для испытуемых - ссылка на профиль
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
