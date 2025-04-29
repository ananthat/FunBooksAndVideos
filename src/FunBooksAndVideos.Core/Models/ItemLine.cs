// Represents a line item in a Purchase Order
namespace FunBooksAndVideos.Core.Models;

public class ItemLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Item Item { get; init; } // Reference to the actual Book, Video, or Membership
    public int Quantity { get; init; } = 1; // Default to 1, can be adjusted if needed
}

