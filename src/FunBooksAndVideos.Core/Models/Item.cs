// Base class for items that can be purchased
namespace FunBooksAndVideos.Core.Models;

public abstract class Item
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public virtual bool IsPhysical { get; } = false; // Default to non-physical
}

