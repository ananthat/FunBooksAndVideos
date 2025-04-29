// Interface for retrieving item details
using FunBooksAndVideos.Core.Models;
using System;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Application.Contracts.Persistence;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid itemId);
    
    //Todo: Here we can add other necessary methods if needed
}

