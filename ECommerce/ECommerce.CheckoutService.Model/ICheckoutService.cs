using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.CheckoutService.Model
{
    public interface ICheckoutService:IService
    {
        Task<CheckoutSummary> Checkout(string userId);
        Task<List<CheckoutSummary>> GetOrderHistory(string userId);
    }
}
