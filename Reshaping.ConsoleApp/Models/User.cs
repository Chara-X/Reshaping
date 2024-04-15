using Reshaping.ConsoleApp.Tables;

namespace Reshaping.ConsoleApp.Models;

internal class User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public Addresses Address { get; set; } = null!;
    public Car? Car { get; set; }
    public IEnumerable<Roles> Roles { get; set; } = null!;
}