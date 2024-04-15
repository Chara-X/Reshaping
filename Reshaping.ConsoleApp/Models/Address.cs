namespace Reshaping.ConsoleApp.Models;

internal class Address
{
    public int Id { get; set; }
    public string? Province { get; set; }
    public string City { get; set; } = null!;
    public IEnumerable<User> Users { get; set; } = null!;
}