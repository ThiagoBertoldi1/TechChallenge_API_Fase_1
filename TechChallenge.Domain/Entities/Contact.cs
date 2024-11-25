namespace TechChallenge.Domain.Entities;
public class Contact
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PhoneNumber { get; set; } = 0;
    public string Region { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
}
