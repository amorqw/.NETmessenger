using Microsoft.EntityFrameworkCore;
using NETmessenger.Contracts.Chats;
using NETmessenger.Domain;
using NETmessenger.Domain.Entities;
using NETmessenger.Web.Infrastructure;
using NETmessenger.Web.Services.Exceptions;

namespace NETmessenger.Web.Services.Chats;

public sealed class ChatService(AppDbContext dbContext) : IChatService
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<GetChatDto> CreateAsync(CreateChatDto dto, CancellationToken cancellationToken)
    {
        var participantIds = (dto.ParticipantUserIds ?? Array.Empty<Guid>())
            .Distinct()
            .ToArray();

        if (participantIds.Length == 0)
        {
            throw new DomainValidationException("Chat must contain at least one participant.");
        }

        if (!dto.IsGroup && participantIds.Length != 2)
        {
            throw new DomainValidationException("Direct chat must contain exactly two participants.");
        }

        var participants = new List<User>(participantIds.Length);

        foreach (var participantId in participantIds)
        {
            var user = _dbContext.Users.Find(participantId);
            if (user == null)
            {
                throw new ResourceNotFoundException($"User '{participantId}' was not found.");
            }
            participants.Add(user);
        }

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            IsGroup = dto.IsGroup,
            Name = NormalizeName(dto.Name)
        };

        chat.Participants = participants;
        _dbContext.Chats.Add(chat);
        //store.MessagesByChat[chat.Id] = new List<Message>();
        
        foreach (var participant in participants)
        {
            participant.Chats.Add(chat);
        }
        _dbContext.SaveChanges();

        return Task.FromResult(MapToDto(chat));
    }

    public Task<GetChatDto?> GetByIdAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var chat = _dbContext.Chats.FirstOrDefault(c => c.Id == chatId);
        return Task.FromResult(chat != null 
            ? MapToDto(chat)
            : null);
    }

    public Task<IReadOnlyCollection<GetChatDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            throw new ResourceNotFoundException($"User '{userId}' was not found.");
        }

        var chats = _dbContext.Chats
            .Where(c => c.Participants.Any(p => p.Id == userId)).AsEnumerable()
            .Select(MapToDto)
            .OrderByDescending(c => c.CreatedAt)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<GetChatDto>>(chats);
    }

    private static string? NormalizeName(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? null
            : name.Trim();
    }

    private static GetChatDto MapToDto(Chat chat)
    {
        return new GetChatDto(
            ChatId: chat.Id,
            CreatedAt: chat.CreatedAt,
            IsGroup: chat.IsGroup,
            Name: chat.Name,
            ParticipantUserIds: chat.Participants.Select(p => p.Id).ToArray());
    }
}
