// Represents a Purchase Order
using System.Collections.Generic;

namespace FunBooksAndVideos.Core.Models;

public class PurchaseOrder
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid CustomerId { get; init; }
    public required List<ItemLine> ItemLines { get; init; }
    public decimal TotalPrice { get; set; } // Calculated during processing
    public string Status { get; set; } = "Pending"; // e.g., Pending, Processing, Completed, Failed
}

