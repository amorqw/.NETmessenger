using System;
using System.Runtime.ConstrainedExecution;
using Microsoft.EntityFrameworkCore;

namespace NETmessenger.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? Password { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Chat> Chats { get; set; } = new List<Chat>();
}







