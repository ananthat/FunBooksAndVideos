// Abstract base class for Products (inherits from Item)
namespace FunBooksAndVideos.Core.Models;

public abstract class Product : Item
{
    public decimal Price { get; init; }
}

