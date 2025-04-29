// API Controller for Shipping Slips
using FunBooksAndVideos.Api.Models;
using FunBooksAndVideos.Application.Contracts.Persistence;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ShippingSlipsController : ControllerBase
{
    private readonly IShippingSlipRepository _shippingSlipRepository;

    public ShippingSlipsController(IShippingSlipRepository shippingSlipRepository)
    {
        _shippingSlipRepository = shippingSlipRepository;
    }

    [HttpGet("byPurchaseOrder/{purchaseOrderId}")]
    [ProducesResponseType(typeof(ShippingSlipResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetShippingSlipByPurchaseOrder(Guid purchaseOrderId)
    {
        var slip = await _shippingSlipRepository.GetByPurchaseOrderIdAsync(purchaseOrderId);
        if (slip == null)
        {
            return NotFound($"No shipping slip found for Purchase Order ID {purchaseOrderId}. It might contain only non-physical items.");
        }

        var responseDto = new ShippingSlipResponseDto
        {
            Id = slip.Id,
            PurchaseOrderId = slip.PurchaseOrderId,
            CustomerId = slip.CustomerId,
            CustomerAddress = slip.CustomerAddress,
            ItemsToShip = slip.ItemsToShip
        };

        return Ok(responseDto);
    }
}
