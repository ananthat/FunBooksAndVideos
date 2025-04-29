// Represents a Shipping Slip generated for physical products
using System.Collections.Generic;

namespace FunBooksAndVideos.Core.Models;

public class ShippingSlip
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid PurchaseOrderId { get; init; }
    public required Guid CustomerId { get; init; }
    public required string CustomerAddress { get; init; }
    public required List<string> ItemsToShip { get; init; } // List of physical item names/descriptions
}

