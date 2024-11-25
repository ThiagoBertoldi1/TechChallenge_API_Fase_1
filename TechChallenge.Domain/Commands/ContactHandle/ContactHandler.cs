using MediatR;
using System.Net;
using TechChallenge.Domain.Commands.ContactHandle.Create;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;
using TechChallenge.Infrastructure;

namespace TechChallenge.Domain.Commands.ContactHandle;
public class ContactHandler(IContactRepository repository) :
    IRequestHandler<CreateContactCommand, ResponseBase<Contact>>
{
    private readonly IContactRepository _repository = repository;

    public async Task<ResponseBase<Contact>> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Email))
            return ResponseBase<Contact>.Fault(HttpStatusCode.BadRequest, "Email inválido");

        var entity = new Contact()
        {
            Email = request.Email,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber
        };

        var inserted = await _repository.Create(entity, cancellationToken);

        return ResponseBase<Contact>.Create(inserted);
    }
}