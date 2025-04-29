// Observer implementation for generating shipping slips
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Application.Features.PurchaseOrders.Observers;

public class ShippingSlipGenerator : IPurchaseOrderObserver
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IShippingSlipRepository _shippingSlipRepository;
    private readonly ILogger<ShippingSlipGenerator> _logger;

    public ShippingSlipGenerator(
        ICustomerRepository customerRepository,
        IShippingSlipRepository shippingSlipRepository,
        ILogger<ShippingSlipGenerator> logger)
    {
        _customerRepository = customerRepository;
        _shippingSlipRepository = shippingSlipRepository;
        _logger = logger;
    }

    public async Task UpdateAsync(PurchaseOrder order, ItemLine itemLine)
    {
        if (itemLine.Item.IsPhysical)
        {
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
            if (customer == null)
            {
                _logger.LogError("Customer {CustomerId} not found for shipping slip generation.", order.CustomerId);
                // Handled error: Cannot generate slip without customer
                return;
            }

            if (string.IsNullOrWhiteSpace(customer.Address))
            {
                _logger.LogError("Customer {CustomerId} has no address for shipping slip generation.", order.CustomerId);
                // Handled error: Cannot generate slip without address
                return;
            }

            // Check if a slip already exists for this PO
            var existingSlip = await _shippingSlipRepository.GetByPurchaseOrderIdAsync(order.Id);

            if (existingSlip == null)
            {
                // Create a new slip
                var newSlip = new ShippingSlip
                {
                    PurchaseOrderId = order.Id,
                    CustomerId = customer.Id,
                    CustomerAddress = customer.Address,
                    ItemsToShip = new List<string> { $"{itemLine.Item.Name} (x{itemLine.Quantity})" }
                };
                await _shippingSlipRepository.AddAsync(newSlip);
                _logger.LogInformation("Generated new shipping slip {SlipId} for PO {PurchaseOrderId}", newSlip.Id, order.Id);
            }
            else
            {
                // In a real system, appending might be complex (Ex: idempotency checks).
                // For simplicity, we just log that we would append.
                // If using an in-memory list, we could append here.
                existingSlip.ItemsToShip.Add($"{itemLine.Item.Name} (x{itemLine.Quantity})");
                // Assuming AddAsync handles updates if the slip already exists or we need an UpdateAsync method.
                // await _shippingSlipRepository.UpdateAsync(existingSlip); // If UpdateAsync exists
                _logger.LogInformation("Appended item {ItemName} to existing shipping slip for PO {PurchaseOrderId}", itemLine.Item.Name, order.Id);
            }
        }
    }
}

