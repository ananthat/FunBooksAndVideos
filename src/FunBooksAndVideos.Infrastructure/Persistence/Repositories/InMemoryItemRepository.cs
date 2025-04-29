// In-memory implementation of the Item Repository
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Infrastructure.Persistence.Repositories;

public class InMemoryItemRepository : IItemRepository
{
    // Store all items (Books, Videos, Memberships) in one dictionary for simplicity
    private static readonly ConcurrentDictionary<Guid, Item> _items = new ConcurrentDictionary<Guid, Item>();

    // Static constructor to pre-populate data
    static InMemoryItemRepository()
    {
        var book = new Book { Id = Guid.Parse("a1b2c3d4-e5f6-7788-99a0-b1c2d3e4f5a6"), Name = "The Girl on the Train", Price = 12.99m, Author = "Paula Hawkins" };
        var video = new Video { Id = Guid.Parse("b2c3d4e5-f6a7-8899-a0b1-c2d3e4f5a6b7"), Name = "Comprehensive First Aid Training", Price = 9.99m, Director = "Jane Doe" };
        var bookClub = new Membership { Id = Guid.Parse("c3d4e5f6-a7b8-99a0-b1c2-d3e4f5a6b7c8"), Name = "Book Club Membership", Type = MembershipType.BookClub, Price = 25.00m };
        var videoClub = new Membership { Id = Guid.Parse("d4e5f6a7-b8c9-a0b1-c2d3-e4f5a6b7c8d9"), Name = "Video Club Membership", Type = MembershipType.VideoClub, Price = 35.00m };
        var premium = new Membership { Id = Guid.Parse("e5f6a7b8-c9d0-b1c2-d3e4-f5a6b7c8d9e0"), Name = "Premium Membership", Type = MembershipType.Premium, Price = 55.00m };

        _items.TryAdd(book.Id, book);
        _items.TryAdd(video.Id, video);
        _items.TryAdd(bookClub.Id, bookClub);
        _items.TryAdd(videoClub.Id, videoClub);
        _items.TryAdd(premium.Id, premium);
    }

    public Task<Item?> GetByIdAsync(Guid itemId)
    {
        _items.TryGetValue(itemId, out var item);
        // Cloning is less critical here if items are treated as immutable reference data,
        // but good practice if they could be modified.
        // For simplicity, returning the direct reference.
        return Task.FromResult(item);
    }

    // Optional: Method to add items (useful for testing/setup)
    public Task AddAsync(Item item)
    {
        _items.TryAdd(item.Id, item);
        return Task.CompletedTask;
    }
}

