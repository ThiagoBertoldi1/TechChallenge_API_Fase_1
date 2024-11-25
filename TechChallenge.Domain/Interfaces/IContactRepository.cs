using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Interfaces;
public interface IContactRepository
{
    Task<Contact> Create(Contact request, CancellationToken cancellationToken);
}
