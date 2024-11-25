using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Interfaces;

namespace TechChallenge.Data.Repository;
public class ContactRepository(DBContext context) : IContactRepository
{
    private readonly DBContext _context = context;

    public DBContext Context => _context;

    public async Task<Contact> Create(Contact request, CancellationToken cancellationToken)
    {
        var entity = await _context.Contacts.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Entity;
    }
}
