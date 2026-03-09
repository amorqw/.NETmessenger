using Microsoft.EntityFrameworkCore;
using NETmessenger.Application.Abstractions.Messages;
using NETmessenger.Application.Exceptions;
using NETmessenger.Contracts.Messages;
using NETmessenger.Domain.Entities;
using NETmessenger.Infrastructure.Persistence;

namespace NETmessenger.Infrastructure.Services.Messages;

public sealed class MessageService(AppDbContext dbContext) : IMessageService
{
    public async Task<IReadOnlyCollection<GetMessageDto>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var chatExists = await dbContext.Chats.AsNoTracking().AnyAsync(c => c.Id == chatId, cancellationToken);
        if (!chatExists)
        {
            throw new ResourceNotFoundException($"Chat '{chatId}' was not found.");
        }

        return await dbContext.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.SentAt)
            .Select(m => new GetMessageDto(m.Id, m.ChatId, m.SenderId, m.Text, m.SentAt))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<GetMessageDto> SendAsync(Guid chatId, SendMessageDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            throw new DomainValidationException("Message text is required.");
        }

        var chatExists = await dbContext.Chats.AsNoTracking().AnyAsync(c => c.Id == chatId, cancellationToken);
        if (!chatExists)
        {
            throw new ResourceNotFoundException($"Chat '{chatId}' was not found.");
        }

        var senderExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == dto.SenderUserId, cancellationToken);

        if (!senderExists)
        {
            throw new ResourceNotFoundException($"User '{dto.SenderUserId}' was not found.");
        }

        var senderInChat = await dbContext.Chats
            .AsNoTracking()
            .Where(c => c.Id == chatId)
            .SelectMany(c => c.Participants)
            .AnyAsync(p => p.Id == dto.SenderUserId, cancellationToken);

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
            SentAt = DateTime.UtcNow
        };

        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new GetMessageDto(
            message.Id,
            message.ChatId,
            message.SenderId,
            message.Text,
            message.SentAt);
    }
}
