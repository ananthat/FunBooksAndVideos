// Observer implementation for activating memberships
using FunBooksAndVideos.Application.Contracts.Persistence;
using FunBooksAndVideos.Core.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FunBooksAndVideos.Application.Features.PurchaseOrders.Observers;

public class MembershipActivator : IPurchaseOrderObserver
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<MembershipActivator> _logger;

    public MembershipActivator(ICustomerRepository customerRepository, ILogger<MembershipActivator> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task UpdateAsync(PurchaseOrder order, ItemLine itemLine)
    {
        if (itemLine.Item is Membership membership)
        {
            var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
            if (customer != null)
            {
                if (!customer.ActiveMemberships.Contains(membership.Type))
                {
                    customer.ActiveMemberships.Add(membership.Type);
                    _logger.LogInformation("Activated {MembershipType} for customer {CustomerId}", membership.Type, customer.Id);

                    // Handle premium membership upgrade logic
                    if (membership.Type == MembershipType.Premium)
                    {
                        if (!customer.ActiveMemberships.Contains(MembershipType.BookClub))
                        {
                            customer.ActiveMemberships.Add(MembershipType.BookClub);
                        }
                        if (!customer.ActiveMemberships.Contains(MembershipType.VideoClub))
                        {
                            customer.ActiveMemberships.Add(MembershipType.VideoClub);
                        }
                        _logger.LogInformation("Premium membership automatically activated Book and Video clubs for customer {CustomerId}", customer.Id);
                    }
                    
                    await _customerRepository.UpdateAsync(customer);
                }
            }
            else
            {
                _logger.LogError("Customer {CustomerId} not found for membership activation.", order.CustomerId);
            }
        }
    }
}

