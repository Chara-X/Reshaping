namespace Reshaping.ConsoleApp.Models;

internal class Car
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public User? User { get; set; }
}