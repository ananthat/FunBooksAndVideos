// Represents a Customer
using System.Collections.Generic;

namespace FunBooksAndVideos.Core.Models;

public class Customer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public string? Address { get; set; } // Nullable for customers without physical orders yet
    public List<MembershipType> ActiveMemberships { get; set; } = new List<MembershipType>();
}

