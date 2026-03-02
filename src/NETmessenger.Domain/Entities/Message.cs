namespace NETmessenger.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    // Внешние ключи
    public int SenderId { get; set; }
    public Guid ChatId { get; set; }     

    public User? Sender { get; set; }
    public Chat? Chat { get; set; }       
}