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

public class CustomersControllerTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;

    public CustomersControllerTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
    }

    [Fact]
    public async Task GetCustomer_WithExistingCustomer_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Address = "123 Test St",
            ActiveMemberships = new List<MembershipType> { MembershipType.BookClub, MembershipType.VideoClub }
        };

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        var controller = new CustomersController(_mockCustomerRepository.Object);

        // Act
        var result = await controller.GetCustomer(customerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        
        var responseDto = Assert.IsType<CustomerResponseDto>(okResult.Value);
        Assert.Equal(customerId, responseDto.Id);
        Assert.Equal("Test Customer", responseDto.Name);
        Assert.Equal("123 Test St", responseDto.Address);
        Assert.Equal(2, responseDto.ActiveMemberships.Count);
        Assert.Contains("BookClub", responseDto.ActiveMemberships);
        Assert.Contains("VideoClub", responseDto.ActiveMemberships);
    }

    [Fact]
    public async Task GetCustomer_WithNonExistingCustomer_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingCustomerId = Guid.NewGuid();

        _mockCustomerRepository.Setup(repo => repo.GetByIdAsync(nonExistingCustomerId))
            .ReturnsAsync((Customer)null);

        var controller = new CustomersController(_mockCustomerRepository.Object);

        // Act
        var result = await controller.GetCustomer(nonExistingCustomerId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}
