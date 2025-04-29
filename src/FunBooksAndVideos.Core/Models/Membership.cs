// Represents a Membership
namespace FunBooksAndVideos.Core.Models;

public class Membership : Item
{
    public MembershipType Type { get; init; }    // IsPhysical remains false (default from Item)
    public decimal Price { get; init; }
}
