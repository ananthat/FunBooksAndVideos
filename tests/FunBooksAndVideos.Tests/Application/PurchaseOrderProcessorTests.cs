using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Application.Features.PurchaseOrders;
using FunBooksAndVideos.Application.Features.PurchaseOrders.Observers;
using FunBooksAndVideos.Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FunBooksAndVideos.Tests.Application;

public class PurchaseOrderProcessorTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IShippingSlipRepository> _mockShippingSlipRepository;
    private readonly Mock<ILogger<PurchaseOrderProcessor>> _mockLogger;
    private readonly Mock<ILogger<MembershipActivator>> _mockMembershipActivatorLogger;
    private readonly Mock<ILogger<ShippingSlipGenerator>> _mockShippingSlipGeneratorLogger;

    public PurchaseOrderProcessorTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockShippingSlipRepository = new Mock<IShippingSlipRepository>();
        _mockLogger = new Mock<ILogger<PurchaseOrderProcessor>>();
        _mockMembershipActivatorLogger = new Mock<ILogger<MembershipActivator>>();
        _mockShippingSlipGeneratorLogger = new Mock<ILogger<ShippingSlipGenerator>>();
    }

    [Fact]
    public async Task ProcessAsync_WithValidCustomer_ShouldCompleteSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Address = "123 Test St"
        };

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Name = "Test Book",
            Price = 10.99m,
            Author = "Test Author"
        };

        var itemLine = new ItemLine
        {
            Item = book,
            Quantity = 1
        };

        var purchaseOrder = new PurchaseOrder
        {
            CustomerId = customerId,
            ItemLines = new List<ItemLine> { itemLine }
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        var observers = new List<IPurchaseOrderObserver>
        {
            new MembershipActivator(_mockCustomerRepository.Object, _mockMembershipActivatorLogger.Object),
            new ShippingSlipGenerator(_mockCustomerRepository.Object, _mockShippingSlipRepository.Object, _mockShippingSlipGeneratorLogger.Object)
        };

        var processor = new PurchaseOrderProcessor(
            observers,
            _mockCustomerRepository.Object,
            _mockLogger.Object);

        // Act
        var result = await processor.ProcessAsync(purchaseOrder);

        // Assert
        Assert.Equal("Completed", result.Status);
        Assert.Equal(10.99m, result.TotalPrice);
        
        // Verify shipping slip was generated for the book
        _mockShippingSlipRepository.Verify(repo => repo.AddAsync(It.IsAny<ShippingSlip>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WithMembership_ShouldActivateMembership()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Address = "123 Test St",
            ActiveMemberships = new List<MembershipType>()
        };

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            Name = "Book Club Membership",
            Type = MembershipType.BookClub
        };

        var itemLine = new ItemLine
        {
            Item = membership,
            Quantity = 1
        };

        var purchaseOrder = new PurchaseOrder
        {
            CustomerId = customerId,
            ItemLines = new List<ItemLine> { itemLine }
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        var observers = new List<IPurchaseOrderObserver>
        {
            new MembershipActivator(_mockCustomerRepository.Object, _mockMembershipActivatorLogger.Object),
            new ShippingSlipGenerator(_mockCustomerRepository.Object, _mockShippingSlipRepository.Object, _mockShippingSlipGeneratorLogger.Object)
        };

        var processor = new PurchaseOrderProcessor(
            observers,
            _mockCustomerRepository.Object,
            _mockLogger.Object);

        // Act
        var result = await processor.ProcessAsync(purchaseOrder);

        // Assert
        Assert.Equal("Completed", result.Status);
        
        // Verify membership was activated
        _mockCustomerRepository.Verify(repo => repo.UpdateAsync(It.Is<Customer>(c => 
            c.ActiveMemberships.Contains(MembershipType.BookClub))), Times.Once);
        
        // Verify no shipping slip was generated (membership is not physical)
        _mockShippingSlipRepository.Verify(repo => repo.AddAsync(It.IsAny<ShippingSlip>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WithPremiumMembership_ShouldActivateAllMemberships()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Address = "123 Test St",
            ActiveMemberships = new List<MembershipType>()
        };

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            Name = "Premium Membership",
            Type = MembershipType.Premium
        };

        var itemLine = new ItemLine
        {
            Item = membership,
            Quantity = 1
        };

        var purchaseOrder = new PurchaseOrder
        {
            CustomerId = customerId,
            ItemLines = new List<ItemLine> { itemLine }
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        var observers = new List<IPurchaseOrderObserver>
        {
            new MembershipActivator(_mockCustomerRepository.Object, _mockMembershipActivatorLogger.Object),
            new ShippingSlipGenerator(_mockCustomerRepository.Object, _mockShippingSlipRepository.Object, _mockShippingSlipGeneratorLogger.Object)
        };

        var processor = new PurchaseOrderProcessor(
            observers,
            _mockCustomerRepository.Object,
            _mockLogger.Object);

        // Act
        var result = await processor.ProcessAsync(purchaseOrder);

        // Assert
        Assert.Equal("Completed", result.Status);
        
        // Verify all memberships were activated
        _mockCustomerRepository.Verify(repo => repo.UpdateAsync(It.Is<Customer>(c => 
            c.ActiveMemberships.Contains(MembershipType.Premium) && 
            c.ActiveMemberships.Contains(MembershipType.BookClub) && 
            c.ActiveMemberships.Contains(MembershipType.VideoClub))), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidCustomer_ShouldFailProcessing()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var purchaseOrder = new PurchaseOrder
        {
            CustomerId = customerId,
            ItemLines = new List<ItemLine>()
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync((Customer)null);

        var observers = new List<IPurchaseOrderObserver>
        {
            new MembershipActivator(_mockCustomerRepository.Object, _mockMembershipActivatorLogger.Object),
            new ShippingSlipGenerator(_mockCustomerRepository.Object, _mockShippingSlipRepository.Object, _mockShippingSlipGeneratorLogger.Object)
        };

        var processor = new PurchaseOrderProcessor(
            observers,
            _mockCustomerRepository.Object,
            _mockLogger.Object);

        // Act
        var result = await processor.ProcessAsync(purchaseOrder);

        // Assert
        Assert.Equal("Failed - Invalid Customer", result.Status);
        
        // Verify no observers were called
        _mockCustomerRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Customer>()), Times.Never);
        _mockShippingSlipRepository.Verify(repo => repo.AddAsync(It.IsAny<ShippingSlip>()), Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_WithMixedItems_ShouldProcessAllCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Address = "123 Test St",
            ActiveMemberships = new List<MembershipType>()
        };

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Name = "Test Book",
            Price = 10.99m,
            Author = "Test Author"
        };

        var video = new Video
        {
            Id = Guid.NewGuid(),
            Name = "Test Video",
            Price = 14.99m,
            Director = "Test Director"
        };

        var membership = new Membership
        {
            Id = Guid.NewGuid(),
            Name = "Book Club Membership",
            Type = MembershipType.BookClub
        };

        var itemLines = new List<ItemLine>
        {
            new ItemLine { Item = book, Quantity = 1 },
            new ItemLine { Item = video, Quantity = 1 },
            new ItemLine { Item = membership, Quantity = 1 }
        };

        var purchaseOrder = new PurchaseOrder
        {
            CustomerId = customerId,
            ItemLines = itemLines
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        var observers = new List<IPurchaseOrderObserver>
        {
            new MembershipActivator(_mockCustomerRepository.Object, _mockMembershipActivatorLogger.Object),
            new ShippingSlipGenerator(_mockCustomerRepository.Object, _mockShippingSlipRepository.Object, _mockShippingSlipGeneratorLogger.Object)
        };

        var processor = new PurchaseOrderProcessor(
            observers,
            _mockCustomerRepository.Object,
            _mockLogger.Object);

        // Act
        var result = await processor.ProcessAsync(purchaseOrder);

        // Assert
        Assert.Equal("Completed", result.Status);
        Assert.Equal(25.98m, result.TotalPrice); // 10.99 + 14.99
        
        // Verify membership was activated
        _mockCustomerRepository.Verify(repo => repo.UpdateAsync(It.Is<Customer>(c => 
            c.ActiveMemberships.Contains(MembershipType.BookClub))), Times.Once);
        
        // Verify shipping slip was generated for the book
        _mockShippingSlipRepository.Verify(repo => repo.AddAsync(It.IsAny<ShippingSlip>()), Times.Once);
    }
}
