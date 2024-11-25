using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Domain.Commands.ContactHandle.Create;
using TechChallenge.Infrastructure;

namespace TechChallenge.API.Controllers;

public class ContactController(IMediator mediatr) : ControllerBaseAPI
{
    private readonly IMediator _mediatr = mediatr;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactCommand request, CancellationToken cancellationToken)
        => ReturnResponse(await _mediatr.Send(request, cancellationToken));
}
