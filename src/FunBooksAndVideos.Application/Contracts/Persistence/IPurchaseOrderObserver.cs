// Interface for Purchase Order Observers
using FunBooksAndVideos.Core.Models;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Application.Contracts.Persistence;

public interface IPurchaseOrderObserver
{
    Task UpdateAsync(PurchaseOrder order, ItemLine itemLine);
}

