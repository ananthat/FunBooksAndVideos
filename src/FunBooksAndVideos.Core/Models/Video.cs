// Represents a Video product
namespace FunBooksAndVideos.Core.Models;

public class Video : Product
{
    public required string Director { get; init; }
    // IsPhysical remains false (default from Item)
}

