using Microsoft.EntityFrameworkCore;
using NETmessenger.Contracts.Messages;
using NETmessenger.Domain;
using NETmessenger.Domain.Entities;
using NETmessenger.Web.Infrastructure;
using NETmessenger.Web.Services.Exceptions;

namespace NETmessenger.Web.Services.Messages;

public sealed class MessageService(AppDbContext dbContext) : IMessageService
{
    private readonly AppDbContext _dbContext = dbContext;
    public Task<IReadOnlyCollection<GetMessageDto>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var chat = _dbContext.Chats.FirstOrDefault(c => c.Id == chatId);
        if (chat == null)
        {
            throw new ResourceNotFoundException($"Chat '{chatId}' was not found.");
        }

        var messages = _dbContext.Messages
            .Where(m => m.ChatId == chatId)
            .ToList();
        messages ??= new List<Message>();

        var dto = messages
            .OrderBy(m => m.SentAt)
            .Select(MapToDto)
            .ToList();

        return Task.FromResult<IReadOnlyCollection<GetMessageDto>>(dto);
    }

    public Task<GetMessageDto> SendAsync(Guid chatId, SendMessageDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            throw new DomainValidationException("Message text is required.");
        }
        
        var chat = _dbContext.Chats.Include(chat => chat.Participants).FirstOrDefault(c => c.Id == chatId);
        
        if (chat == null)
        {
            throw new ResourceNotFoundException($"Chat '{chatId}' was not found.");
        }

        var user = _dbContext.Users.FirstOrDefault(c => c.Id == dto.SenderUserId);
        
        if (user == null)
        {
            throw new ResourceNotFoundException($"User '{dto.SenderUserId}' was not found.");
        }

        var senderInChat = chat.Participants.Any(p => p.Id == dto.SenderUserId);
        if (!senderInChat)
        {
            throw new DomainValidationException("Sender is not a participant of this chat.");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SenderId = dto.SenderUserId,
            Text = dto.Text.Trim(),
            SentAt = DateTime.UtcNow,
            Chat = chat,
            Sender = user
        };

        _dbContext.Messages.Add(message);
        _dbContext.SaveChanges();

        var list = _dbContext.Messages
            .Where(m => m.ChatId == chatId)
            .ToList();
        
        /* Я не знаю как это адаптировать под БД
        if (!store.MessagesByChat.TryGetValue(chatId, out var list))
        {
            list = new List<Message>();
            store.MessagesByChat[chatId] = list;
        }
        */
        
        if (list == null)
        {
            throw new DomainValidationException("MessagesByChat not found???");
        }

        list.Add(message);

        return Task.FromResult(MapToDto(message));
    }

    private static GetMessageDto MapToDto(Message message)
    {
        return new GetMessageDto(
            MessageId: message.Id,
            ChatId: message.ChatId,
            SenderUserId: message.SenderId,
            Text: message.Text,
            SentAt: message.SentAt);
    }
}
