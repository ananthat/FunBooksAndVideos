// Concrete implementation of the Purchase Order Processor
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Application.Contracts.Processor;
using FunBooksAndVideos.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Application.Features.PurchaseOrders;

public class PurchaseOrderProcessor : IPurchaseOrderProcessor
{
    private readonly IEnumerable<IPurchaseOrderObserver> _observers;
    private readonly ICustomerRepository _customerRepository; 
    // Assuming IItemRepository is needed if Item details aren't fully populated in the input order
    // private readonly IItemRepository _itemRepository; 
    private readonly ILogger<PurchaseOrderProcessor> _logger;

    // Inject all registered observers and necessary repositories
    public PurchaseOrderProcessor(
        IEnumerable<IPurchaseOrderObserver> observers, 
        ICustomerRepository customerRepository,
        // IItemRepository itemRepository, // Uncomment if needed
        ILogger<PurchaseOrderProcessor> logger)
    {
        _observers = observers;
        _customerRepository = customerRepository;
        // _itemRepository = itemRepository; // Uncomment if needed
        _logger = logger;
    }

    public async Task<PurchaseOrder> ProcessAsync(PurchaseOrder order)
    {
        _logger.LogInformation("Processing Purchase Order {PurchaseOrderId} for Customer {CustomerId}", order.Id, order.CustomerId);
        order.Status = "Processing";

        // 1. Validate Customer
        var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        if (customer == null)
        {
            _logger.LogError("Customer {CustomerId} not found. Aborting processing for PO {PurchaseOrderId}.", order.CustomerId, order.Id);
            order.Status = "Failed - Invalid Customer";
            return order;
        }

        // 2. Calculate Total Price (assuming item details including price are already populated in order.ItemLines)
        // If not, fetch items using _itemRepository
        decimal totalPrice = 0;
        foreach (var line in order.ItemLines)
        {
            if (line.Item is Product product) 
            {
                totalPrice += product.Price * line.Quantity;
            }
            if (line.Item is Membership membership) 
            {
                totalPrice += membership.Price * line.Quantity;
            }
        }
        order.TotalPrice = totalPrice;
        _logger.LogInformation("Calculated Total Price {TotalPrice} for PO {PurchaseOrderId}", order.TotalPrice, order.Id);


        // 3. Notify Observers for each item line
        foreach (var line in order.ItemLines)
        {
            _logger.LogDebug("Processing item line {ItemLineId} ({ItemName}) for PO {PurchaseOrderId}", line.Id, line.Item.Name, order.Id);
            // Use Task.WhenAll to run observer updates concurrently if they are independent
            var observerTasks = _observers.Select(observer => observer.UpdateAsync(order, line));
            await Task.WhenAll(observerTasks);
        }

        order.Status = "Completed";
        _logger.LogInformation("Finished processing Purchase Order {PurchaseOrderId}", order.Id);
        return order;
    }
}

