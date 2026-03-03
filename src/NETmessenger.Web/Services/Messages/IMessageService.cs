using NETmessenger.Contracts.Messages;

namespace NETmessenger.Web.Services.Messages;

public interface IMessageService
{
    Task<IReadOnlyCollection<GetMessageDto>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken);
    Task<GetMessageDto> SendAsync(Guid chatId, SendMessageDto dto, CancellationToken cancellationToken);
}
