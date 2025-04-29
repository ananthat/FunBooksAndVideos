using FunBooksAndVideos.Api.Controllers;
using FunBooksAndVideos.Api.Models;
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FunBooksAndVideos.Tests.Api;

public class ShippingSlipsControllerTests
{
    private readonly Mock<IShippingSlipRepository> _mockShippingSlipRepository;

    public ShippingSlipsControllerTests()
    {
        _mockShippingSlipRepository = new Mock<IShippingSlipRepository>();
    }

    [Fact]
    public async Task GetShippingSlipByPurchaseOrder_WithExistingSlip_ShouldReturnSlip()
    {
        // Arrange
        var purchaseOrderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var slipId = Guid.NewGuid();
        
        var shippingSlip = new ShippingSlip
        {
            Id = slipId,
            PurchaseOrderId = purchaseOrderId,
            CustomerId = customerId,
            CustomerAddress = "123 Test St",
            ItemsToShip = new List<string> { "Test Book (x1)" }
        };

        _mockShippingSlipRepository.Setup(repo => repo.GetByPurchaseOrderIdAsync(purchaseOrderId))
            .ReturnsAsync(shippingSlip);

        var controller = new ShippingSlipsController(_mockShippingSlipRepository.Object);

        // Act
        var result = await controller.GetShippingSlipByPurchaseOrder(purchaseOrderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        
        var responseDto = Assert.IsType<ShippingSlipResponseDto>(okResult.Value);
        Assert.Equal(slipId, responseDto.Id);
        Assert.Equal(purchaseOrderId, responseDto.PurchaseOrderId);
        Assert.Equal(customerId, responseDto.CustomerId);
        Assert.Equal("123 Test St", responseDto.CustomerAddress);
        Assert.Single(responseDto.ItemsToShip);
        Assert.Equal("Test Book (x1)", responseDto.ItemsToShip[0]);
    }

    [Fact]
    public async Task GetShippingSlipByPurchaseOrder_WithNonExistingSlip_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingPurchaseOrderId = Guid.NewGuid();

        _mockShippingSlipRepository.Setup(repo => repo.GetByPurchaseOrderIdAsync(nonExistingPurchaseOrderId))
            .ReturnsAsync((ShippingSlip)null);

        var controller = new ShippingSlipsController(_mockShippingSlipRepository.Object);

        // Act
        var result = await controller.GetShippingSlipByPurchaseOrder(nonExistingPurchaseOrderId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Contains("No shipping slip found", notFoundResult.Value.ToString());
    }
}
