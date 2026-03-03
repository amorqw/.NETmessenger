using Microsoft.AspNetCore.Mvc;
using NETmessenger.Contracts.Users;
using NETmessenger.Web.Services.Exceptions;
using NETmessenger.Web.Services.Users;

namespace NETmessenger.Web.Controllers.Users;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<GetUserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<GetUserDto>> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<GetUserDto>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { userId = user.UserId }, user);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<GetUserDto>> Update(Guid userId, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.UpdateAsync(userId, dto, cancellationToken);
            return Ok(user);
        }
        catch (ResourceNotFoundException)
        {
            return NotFound();
        }
        catch (ConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
