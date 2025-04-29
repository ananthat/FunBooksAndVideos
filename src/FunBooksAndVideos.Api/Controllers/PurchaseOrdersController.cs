// API Controller for Purchase Orders
using FunBooksAndVideos.Api.Models;
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Application.Contracts.Processor;
using FunBooksAndVideos.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderProcessor _processor;
    private readonly IItemRepository _itemRepository;
    private readonly ICustomerRepository _customerRepository; // Needed to get customer details for response
    // In a real app, you might have a dedicated query service instead of repositories in controller
    private readonly Dictionary<Guid, PurchaseOrder> _processedOrders = new Dictionary<Guid, PurchaseOrder>(); // Simple in-memory store for processed orders

    public PurchaseOrdersController(
        IPurchaseOrderProcessor processor,
        IItemRepository itemRepository,
        ICustomerRepository customerRepository)
    {
        _processor = processor;
        _itemRepository = itemRepository;
        _customerRepository = customerRepository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PurchaseOrderResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrderRequestDto requestDto)
    {
        if (requestDto == null || !requestDto.Items.Any())
        {
            return BadRequest("Request body is invalid or contains no items.");
        }

        // Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(requestDto.CustomerId);
        if (customer == null)
        {
            return NotFound($"Customer with ID {requestDto.CustomerId} not found.");
        }

        var itemLines = new List<ItemLine>();
        foreach (var itemReq in requestDto.Items)
        {
            var item = await _itemRepository.GetByIdAsync(itemReq.ItemId);
            if (item == null)
            {
                // Consider specific item type validation if needed
                return BadRequest($"Item with ID {itemReq.ItemId} not found.");
            }
            itemLines.Add(new ItemLine { Item = item, Quantity = itemReq.Quantity });
        }

        var newOrder = new PurchaseOrder
        {
            CustomerId = requestDto.CustomerId,
            ItemLines = itemLines
        };

        // Process the order
        var processedOrder = await _processor.ProcessAsync(newOrder);

        // Store processed order (in-memory)
        _processedOrders[processedOrder.Id] = processedOrder;

        // Map to Response DTO
        var responseDto = MapToPurchaseOrderResponseDto(processedOrder);

        // Return 201 Created with the processed order details
        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = processedOrder.Id }, responseDto);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PurchaseOrderResponseDto), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetPurchaseOrder(Guid id)
    {
        if (_processedOrders.TryGetValue(id, out var order))
        {
            var responseDto = MapToPurchaseOrderResponseDto(order);
            return Ok(responseDto);
        }
        return NotFound($"Purchase Order with ID {id} not found.");
    }

    // Mapping Function 
    private PurchaseOrderResponseDto MapToPurchaseOrderResponseDto(PurchaseOrder order)
    {
        return new PurchaseOrderResponseDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            ItemLines = order.ItemLines.Select(line => new ItemLineResponseDto
            {
                Id = line.Id,
                ItemName = line.Item.Name,
                ItemType = line.Item switch
                {
                    Book => "Book",
                    Video => "Video",
                    Membership => "Membership",
                    _ => "Unknown"
                },
                Quantity = line.Quantity
            }).ToList()
        };
    }
}

