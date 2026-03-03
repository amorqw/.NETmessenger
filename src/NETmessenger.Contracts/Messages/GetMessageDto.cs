namespace NETmessenger.Contracts.Messages;

public record GetMessageDto(Guid MessageId, Guid ChatId, Guid SenderUserId, string Text, DateTime SentAt);
