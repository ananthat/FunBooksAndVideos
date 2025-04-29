// Interface for the Purchase Order Processor
using FunBooksAndVideos.Core.Models;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Application.Contracts.Processor;

public interface IPurchaseOrderProcessor
{
    Task<PurchaseOrder> ProcessAsync(PurchaseOrder order);
}

