using Microsoft.EntityFrameworkCore;
using NETmessenger.Domain.Entities;

namespace NETmessenger.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options): base(options) {}
    
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Chat> Chats { get; set; }
}