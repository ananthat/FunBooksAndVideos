// Represents a Book product
namespace FunBooksAndVideos.Core.Models;

public class Book : Product
{
    public required string Author { get; init; }
    public override bool IsPhysical => true; // Books are physical
}

