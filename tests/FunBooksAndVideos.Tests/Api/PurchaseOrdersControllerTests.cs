using FunBooksAndVideos.Api.Controllers;
using FunBooksAndVideos.Api.Models;
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Application.Contracts.Processor;
using FunBooksAndVideos.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FunBooksAndVideos.Tests.Api;

public class PurchaseOrdersControllerTests
{
    private readonly Mock<IPurchaseOrderProcessor> _mockProcessor;
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;

    public PurchaseOrdersControllerTests()
    {
        _mockProcessor = new Mock<IPurchaseOrderProcessor>();
        _mockItemRepository = new Mock<IItemRepository>();
        _mockCustomerRepository = new Mock<ICustomerRepository>();
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Address = "123 Test St"
        };

        var book = new Book
        {
            Id = bookId,
            Name = "Test Book",
            Price = 10.99m,
            Author = "Test Author"
        };

        var membership = new Membership
        {
            Id = membershipId,
            Name = "Book Club Membership",
            Type = MembershipType.BookClub
        };

        var requestDto = new PurchaseOrderRequestDto
        {
            CustomerId = customerId,
            Items = new List<ItemRequestDto>
            {
                new ItemRequestDto { ItemId = bookId, ItemType = "book", Quantity = 1 },
                new ItemRequestDto { ItemId = membershipId, ItemType = "membership", Quantity = 1 }
            }
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _mockItemRepository.Setup(repo => repo.GetByIdAsync(bookId))
            .ReturnsAsync(book);

        _mockItemRepository.Setup(repo => repo.GetByIdAsync(membershipId))
            .ReturnsAsync(membership);

        _mockProcessor.Setup(p => p.ProcessAsync(It.IsAny<PurchaseOrder>()))
            .ReturnsAsync((PurchaseOrder po) => 
            {
                // Create a new PO with the ID set during initialization
                var processedPo = new PurchaseOrder
                {
                    Id = Guid.NewGuid(), // Set ID here
                    CustomerId = po.CustomerId,
                    ItemLines = po.ItemLines,
                    Status = "Completed",
                    TotalPrice = 10.99m // Only the book has a price
                };
                return processedPo;
            });

        var controller = new PurchaseOrdersController(
            _mockProcessor.Object,
            _mockItemRepository.Object,
            _mockCustomerRepository.Object);

        // Act
        var result = await controller.CreatePurchaseOrder(requestDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        
        var responseDto = Assert.IsType<PurchaseOrderResponseDto>(createdResult.Value);
        Assert.Equal("Completed", responseDto.Status);
        Assert.Equal(10.99m, responseDto.TotalPrice);
        Assert.Equal(2, responseDto.ItemLines.Count);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithInvalidCustomer_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var requestDto = new PurchaseOrderRequestDto
        {
            CustomerId = customerId,
            Items = new List<ItemRequestDto>
            {
                new ItemRequestDto { ItemId = Guid.NewGuid(), ItemType = "book", Quantity = 1 }
            }
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync((Customer)null);

        var controller = new PurchaseOrdersController(
            _mockProcessor.Object,
            _mockItemRepository.Object,
            _mockCustomerRepository.Object);

        // Act
        var result = await controller.CreatePurchaseOrder(requestDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task CreatePurchaseOrder_WithInvalidItem_ShouldReturnBadRequest()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var invalidItemId = Guid.NewGuid();

        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Address = "123 Test St"
        };

        var requestDto = new PurchaseOrderRequestDto
        {
            CustomerId = customerId,
            Items = new List<ItemRequestDto>
            {
                new ItemRequestDto { ItemId = invalidItemId, ItemType = "book", Quantity = 1 }
            }
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _mockItemRepository.Setup(repo => repo.GetByIdAsync(invalidItemId))
            .ReturnsAsync((Item)null);

        var controller = new PurchaseOrdersController(
            _mockProcessor.Object,
            _mockItemRepository.Object,
            _mockCustomerRepository.Object);

        // Act
        var result = await controller.CreatePurchaseOrder(requestDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void GetPurchaseOrder_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Name = "Test Book",
            Price = 10.99m,
            Author = "Test Author"
        };

        var itemLine = new ItemLine
        {
            Id = Guid.NewGuid(),
            Item = book,
            Quantity = 1
        };

        var order = new PurchaseOrder
        {
            Id = orderId,
            CustomerId = customerId,
            ItemLines = new List<ItemLine> { itemLine },
            TotalPrice = 10.99m,
            Status = "Completed"
        };

        var controller = new PurchaseOrdersController(
            _mockProcessor.Object,
            _mockItemRepository.Object,
            _mockCustomerRepository.Object);

        // Use reflection to set the private _processedOrders dictionary
        var processedOrdersField = typeof(PurchaseOrdersController)
            .GetField("_processedOrders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var processedOrders = new Dictionary<Guid, PurchaseOrder>();
        processedOrders[orderId] = order;
        processedOrdersField.SetValue(controller, processedOrders);

        // Act
        var result = controller.GetPurchaseOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        
        var responseDto = Assert.IsType<PurchaseOrderResponseDto>(okResult.Value);
        Assert.Equal(orderId, responseDto.Id);
        Assert.Equal(customerId, responseDto.CustomerId);
        Assert.Equal(10.99m, responseDto.TotalPrice);
        Assert.Equal("Completed", responseDto.Status);
        Assert.Single(responseDto.ItemLines);
        Assert.Equal("Test Book", responseDto.ItemLines[0].ItemName);
    }

    [Fact]
    public void GetPurchaseOrder_WithNonExistingOrder_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingOrderId = Guid.NewGuid();

        var controller = new PurchaseOrdersController(
            _mockProcessor.Object,
            _mockItemRepository.Object,
            _mockCustomerRepository.Object);

        // Act
        var result = controller.GetPurchaseOrder(nonExistingOrderId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}
