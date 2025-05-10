using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using TechChallenge.Domain.Commands.ContactCommands.Create;
using TechChallenge.Domain.Commands.ContactCommands.Delete;
using TechChallenge.Domain.Commands.ContactCommands.Select;
using TechChallenge.Domain.Commands.ContactCommands.Update;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interface;
using TechChallenge.Domain.Validations;
using TechChallenge.Infra.Helpers.Cache;
using TechChallenge.Infra.Helpers.DDD;
using TechChallenge.Infra.RabbitMQ;
using TechChallenge.Infra.Responses;

namespace TechChallenge.Domain.Commands.ContactCommands;
public class ContactHandler(
    IMemoryCache memoryCache,
    IContactRepository contactRepository,
    IRabbitMQ rabbitMQ) :
    IRequestHandler<GetContactListCommand, ResponseBase<List<Contact>>>,
    IRequestHandler<CreateContactCommand, ResponseBase<string>>,
    IRequestHandler<UpdateContactCommand, ResponseBase<string>>,
    IRequestHandler<DeleteContactCommand, ResponseBase<string>>
{
    private readonly IContactRepository _contactRepository = contactRepository;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IRabbitMQ _rabbitMQ = rabbitMQ;

    private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

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

        await _rabbitMQ.Publish("Contact.Queue.Insert", entity);

        return ResponseBase<string>.Success("Contato criado com sucesso.");
    }

    public async Task<ResponseBase<List<Contact>>> Handle(GetContactListCommand filters, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(CacheHelper.CreateCacheKey(filters), out ResponseBase<List<Contact>>? cachedResponse)) return cachedResponse!;

        var listContacts = await _contactRepository.GetContactList(filters, cancellationToken);

        var response = ResponseBase<List<Contact>>.Success(listContacts);

        _memoryCache.Set(CacheHelper.CreateCacheKey(filters), response, _cacheOptions);

        return response;
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

        await _rabbitMQ.Publish("Contact.Queue.Update", dataToUpdate);

        return ResponseBase<string>.Success("Contato atualizado com sucesso");
    }

    public async Task<ResponseBase<string>> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetById(request.Id, cancellationToken);
        if (contact is null)
            return ResponseBase<string>.Error(HttpStatusCode.NotFound, ["Contato não encontrado"]);

        await _rabbitMQ.Publish("Contact.Queue.Delete", request.Id);

        return ResponseBase<string>.Success("Contato deletado com sucesso");
    }
}
