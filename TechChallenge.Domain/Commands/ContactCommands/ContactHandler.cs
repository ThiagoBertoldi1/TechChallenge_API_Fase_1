﻿using MediatR;
using System.Net;
using TechChallenge.Domain.Commands.ContactCommands.Create;
using TechChallenge.Domain.Commands.ContactCommands.Delete;
using TechChallenge.Domain.Commands.ContactCommands.Select;
using TechChallenge.Domain.Commands.ContactCommands.Update;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interface;
using TechChallenge.Domain.Validations;
using TechChallenge.Infra.Helpers.DDD;
using TechChallenge.Infra.Responses;

namespace TechChallenge.Domain.Commands.ContactCommands;
public class ContactHandler(IContactRepository contactRepository) :
    IRequestHandler<GetContactListCommand, ResponseBase<List<Contact>>>,
    IRequestHandler<CreateContactCommand, ResponseBase<string>>,
    IRequestHandler<UpdateContactCommand, ResponseBase<string>>,
    IRequestHandler<DeleteContactCommand, ResponseBase<string>>
{
    private readonly IContactRepository _contactRepository = contactRepository;

    public async Task<ResponseBase<string>> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var entity = new Contact
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };

        new ContactValidation().ContactValidator(entity);

        var exists = await _contactRepository.IsPhoneNumberRegistered(entity.PhoneNumber, cancellationToken);
        if (exists is not null)
            return ResponseBase<string>.Error(HttpStatusCode.BadRequest, ["Número de telefone já cadastrado"]);

        var (region, district) = await new GetDDDHelper().GetDDDInfo(int.Parse(request.PhoneNumber.ToString()[..2]));

        entity.District = district;
        entity.Region = region;

        var inserted = await _contactRepository.InsertAsync(entity, cancellationToken);
        if (!inserted)
            return ResponseBase<string>.Error(HttpStatusCode.InternalServerError, ["Usuário não inserido"]);

        return ResponseBase<string>.Success("Contato criado com sucesso.");
    }

    public async Task<ResponseBase<List<Contact>>> Handle(GetContactListCommand filters, CancellationToken cancellationToken)
    {
        var response = await _contactRepository.GetContactList(filters, cancellationToken);

        return ResponseBase<List<Contact>>.Success(response);
    }

    public async Task<ResponseBase<string>> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contato = await _contactRepository.GetById(request.Id, cancellationToken);
        if (contato is null)
            return ResponseBase<string>.Error(HttpStatusCode.NotFound, ["Contato não encontrado"]);

        var exists = await _contactRepository.IsPhoneNumberRegistered(request.PhoneNumber, cancellationToken);
        if (exists is not null && request.Id != exists.Id)
            return ResponseBase<string>.Error(HttpStatusCode.BadRequest, ["Número de telefone já cadastrado"]);

        var dataToUpdate = new Contact
        {
            Id = request.Id,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            District = contato.District,
            Region = contato.Region
        };

        new ContactValidation().ContactValidator(dataToUpdate);

        if (contato.PhoneNumber != request.PhoneNumber)
        {
            var (region, district) = await new GetDDDHelper().GetDDDInfo(int.Parse(request.PhoneNumber.ToString()[..2]));

            dataToUpdate.District = district;
            dataToUpdate.Region = region;
        }

        var updated = await _contactRepository.UpdateAsync(dataToUpdate, cancellationToken);
        if (!updated)
            return ResponseBase<string>.Error(HttpStatusCode.InternalServerError, ["Usuário não atualizado"]);

        return ResponseBase<string>.Success("Contato atualizado com sucesso");
    }

    public async Task<ResponseBase<string>> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetById(request.Id, cancellationToken);
        if (contact is null)
            return ResponseBase<string>.Error(HttpStatusCode.NotFound, ["Contato não encontrado"]);

        var deleted = await _contactRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
            return ResponseBase<string>.Error(HttpStatusCode.InternalServerError, ["Não foi possível deletar o contato"]);

        return ResponseBase<string>.Success("Contato deletado com sucesso");
    }
}
