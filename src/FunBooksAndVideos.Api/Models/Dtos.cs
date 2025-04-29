// DTOs for API requests and responses
using FunBooksAndVideos.Core.Models;
using System;
using System.Collections.Generic;

namespace FunBooksAndVideos.Api.Models;

public class ItemRequestDto
{
    public Guid ItemId { get; set; }
    public string ItemType { get; set; } = string.Empty; // "book", "video", or "membership"
    public int Quantity { get; set; } = 1;
}

public class PurchaseOrderRequestDto
{
    public Guid CustomerId { get; set; }
    public List<ItemRequestDto> Items { get; set; } = new List<ItemRequestDto>();
}

public class CustomerResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public List<string> ActiveMemberships { get; set; } = new List<string>();
}

public class ItemLineResponseDto
{
    public Guid Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class PurchaseOrderResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<ItemLineResponseDto> ItemLines { get; set; } = new List<ItemLineResponseDto>();
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ShippingSlipResponseDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerAddress { get; set; } = string.Empty;
    public List<string> ItemsToShip { get; set; } = new List<string>();
}
