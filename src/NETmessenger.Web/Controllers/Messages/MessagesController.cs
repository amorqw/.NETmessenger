using Microsoft.AspNetCore.Mvc;
using NETmessenger.Application.Abstractions.Messages;
using NETmessenger.Application.Exceptions;
using NETmessenger.Contracts.Messages;

namespace NETmessenger.Web.Controllers.Messages;

[ApiController]
[Route("api/chats/{chatId:guid}/messages")]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<GetMessageDto>>> GetByChatId(Guid chatId, CancellationToken cancellationToken)
    {
        try
        {
            var messages = await messageService.GetByChatIdAsync(chatId, cancellationToken);
            return Ok(messages);
        }
        catch (ResourceNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<GetMessageDto>> Send(Guid chatId, [FromBody] SendMessageDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var message = await messageService.SendAsync(chatId, dto, cancellationToken);
            return Ok(message);
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
