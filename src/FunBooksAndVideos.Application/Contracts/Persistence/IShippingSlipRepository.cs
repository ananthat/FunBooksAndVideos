// Interface for Shipping Slip Repository
using FunBooksAndVideos.Core.Models;
using System;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Application.Contracts.Persistence;

public interface IShippingSlipRepository
{
    Task AddAsync(ShippingSlip shippingSlip);
    Task<ShippingSlip?> GetByPurchaseOrderIdAsync(Guid purchaseOrderId);
    // Todo: Here we can add other necessary methods if needed
}

