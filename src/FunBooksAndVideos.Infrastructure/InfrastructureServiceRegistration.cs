// Infrastructure service registration
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Application.Contracts.Processor;
using FunBooksAndVideos.Application.Features.PurchaseOrders;
using FunBooksAndVideos.Application.Features.PurchaseOrders.Observers;
using FunBooksAndVideos.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FunBooksAndVideos.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
        services.AddSingleton<IShippingSlipRepository, InMemoryShippingSlipRepository>();
        services.AddSingleton<IItemRepository, InMemoryItemRepository>();
        
        // Register observers
        services.AddScoped<IPurchaseOrderObserver, MembershipActivator>();
        services.AddScoped<IPurchaseOrderObserver, ShippingSlipGenerator>();
        
        // Register processor
        services.AddScoped<IPurchaseOrderProcessor, PurchaseOrderProcessor>();
        
        return services;
    }
}
