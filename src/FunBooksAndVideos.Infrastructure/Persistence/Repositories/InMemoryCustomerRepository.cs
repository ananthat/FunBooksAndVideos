// In-memory implementation of the Customer Repository
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Infrastructure.Persistence.Repositories;

public class InMemoryCustomerRepository : ICustomerRepository
{
    // Using ConcurrentDictionary for basic thread safety in a web context
    private static readonly ConcurrentDictionary<Guid, Customer> _customers = new ConcurrentDictionary<Guid, Customer>();

    // Static constructor or method to pre-populate data if needed for testing
    static InMemoryCustomerRepository()
    {
        // Example: Add a default customer
        var defaultCustomer = new Customer { Id = Guid.Parse("f2e1a0a3-1b9c-4a7e-8c3d-9b8e1a0f2b9c"), Name = "Default Customer", Address = "123 Default St" };
        _customers.TryAdd(defaultCustomer.Id, defaultCustomer);
    }

    public Task<Customer?> GetByIdAsync(Guid customerId)
    {
        _customers.TryGetValue(customerId, out var customer);
        // Return a copy to prevent external modification of the in-memory object
        return Task.FromResult(customer != null ? CloneCustomer(customer) : null);
    }

    public Task UpdateAsync(Customer customer)
    {
        if (_customers.ContainsKey(customer.Id))
        {
            // Update the existing customer (replace with a copy)
            _customers[customer.Id] = CloneCustomer(customer);
        }
        // Handle case where customer doesn't exist if necessary (e.g., throw exception)
        return Task.CompletedTask;
    }

    // Helper to clone customer to avoid direct modification of stored object
    private Customer CloneCustomer(Customer original)
    {
        return new Customer
        {
            Id = original.Id,
            Name = original.Name,
            Address = original.Address,
            ActiveMemberships = new List<MembershipType>(original.ActiveMemberships) // Clone the list
        };
    }

    // Optional: Method to add a customer (useful for testing/setup)
    public Task AddAsync(Customer customer)
    {
        _customers.TryAdd(customer.Id, CloneCustomer(customer));
        return Task.CompletedTask;
    }
}

