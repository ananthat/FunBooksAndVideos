// Interface for Customer Repository
using FunBooksAndVideos.Core.Models;
using System;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Application.Contracts.Persistence;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid customerId);
    Task UpdateAsync(Customer customer);
    
    //Todo: Here we can add other necessary methods like AddAsync, DeleteAsync etc. if needed
}

