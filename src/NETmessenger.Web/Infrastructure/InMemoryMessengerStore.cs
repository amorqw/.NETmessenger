using NETmessenger.Domain.Entities;

namespace NETmessenger.Web.Infrastructure;

public sealed class InMemoryMessengerStore
{
    public object SyncRoot { get; } = new();
    public Dictionary<Guid, User> Users { get; } = new();
    public Dictionary<Guid, Chat> Chats { get; } = new();
    public Dictionary<Guid, List<Message>> MessagesByChat { get; } = new();
}
