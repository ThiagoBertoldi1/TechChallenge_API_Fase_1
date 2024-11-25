using MediatR;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure;

namespace TechChallenge.Domain.Commands.ContactHandle.Create;
public class CreateContactCommand : IRequest<ResponseBase<Contact>>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PhoneNumber { get; set; } = 0;
}
