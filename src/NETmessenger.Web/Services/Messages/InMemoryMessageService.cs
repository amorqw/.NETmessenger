using NETmessenger.Contracts.Messages;
using NETmessenger.Domain.Entities;
using NETmessenger.Web.Infrastructure;
using NETmessenger.Web.Services.Exceptions;

namespace NETmessenger.Web.Services.Messages;

public sealed class InMemoryMessageService(InMemoryMessengerStore store) : IMessageService
{
    public Task<IReadOnlyCollection<GetMessageDto>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken)
    {
        lock (store.SyncRoot)
        {
            if (!store.Chats.ContainsKey(chatId))
            {
                throw new ResourceNotFoundException($"Chat '{chatId}' was not found.");
            }

            store.MessagesByChat.TryGetValue(chatId, out var messages);
            messages ??= new List<Message>();

            var dto = messages
                .OrderBy(m => m.SentAt)
                .Select(MapToDto)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<GetMessageDto>>(dto);
        }
    }

    public Task<GetMessageDto> SendAsync(Guid chatId, SendMessageDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
        {
            throw new DomainValidationException("Message text is required.");
        }

        lock (store.SyncRoot)
        {
            if (!store.Chats.TryGetValue(chatId, out var chat))
            {
                throw new ResourceNotFoundException($"Chat '{chatId}' was not found.");
            }

            if (!store.Users.ContainsKey(dto.SenderUserId))
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
                Sender = store.Users[dto.SenderUserId]
            };

            chat.Messages.Add(message);

            if (!store.MessagesByChat.TryGetValue(chatId, out var list))
            {
                list = new List<Message>();
                store.MessagesByChat[chatId] = list;
            }

            list.Add(message);

            return Task.FromResult(MapToDto(message));
        }
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
