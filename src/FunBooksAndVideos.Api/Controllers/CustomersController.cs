// API Controller for Customers
using FunBooksAndVideos.Api.Models;
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCustomer(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return NotFound($"Customer with ID {id} not found.");
        }

        var responseDto = new CustomerResponseDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address,
            ActiveMemberships = customer.ActiveMemberships.Select(m => m.ToString()).ToList()
        };

        return Ok(responseDto);
    }
}
