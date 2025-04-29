// In-memory implementation of the Shipping Slip Repository
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Infrastructure.Persistence.Repositories;

public class InMemoryShippingSlipRepository : IShippingSlipRepository
{
    private static readonly ConcurrentDictionary<Guid, ShippingSlip> _shippingSlips = new ConcurrentDictionary<Guid, ShippingSlip>();

    public Task AddAsync(ShippingSlip shippingSlip)
    {
        // In a real DB, this might handle updates if the PO ID already exists.
        // For in-memory, we assume Add means add or update based on PO ID for simplicity in the observer.
        // Using the Slip ID as the key here.
        _shippingSlips.AddOrUpdate(shippingSlip.Id, CloneSlip(shippingSlip), (key, existingVal) => CloneSlip(shippingSlip));
        return Task.CompletedTask;
    }

    public Task<ShippingSlip?> GetByPurchaseOrderIdAsync(Guid purchaseOrderId)
    {
        // This assumes one slip per PO. If multiple slips were possible, this would return a list.
        var slip = _shippingSlips.Values.FirstOrDefault(s => s.PurchaseOrderId == purchaseOrderId);
        return Task.FromResult(slip != null ? CloneSlip(slip) : null);
    }
    
    // Optional: Get by Slip ID
    public Task<ShippingSlip?> GetByIdAsync(Guid slipId)
    {
        _shippingSlips.TryGetValue(slipId, out var slip);
        return Task.FromResult(slip != null ? CloneSlip(slip) : null);
    }

    // Helper to clone slip
    private ShippingSlip CloneSlip(ShippingSlip original)
    {
        return new ShippingSlip
        {
            Id = original.Id,
            PurchaseOrderId = original.PurchaseOrderId,
            CustomerId = original.CustomerId,
            CustomerAddress = original.CustomerAddress,
            ItemsToShip = new List<string>(original.ItemsToShip) // Clone the list
        };
    }
}

