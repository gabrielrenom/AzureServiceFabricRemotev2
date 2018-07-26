using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.API.ViewModels;
using ECommerce.CheckoutService.Model;
using ECommerce.ProductCatalog.Model.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    public class CheckoutController : Controller
    {
        private static readonly Random rnd = new Random(DateTime.UtcNow.Second);

        [Route("{userId}")]
        public async Task<CheckoutSummaryViewModel> Checkout(string userId)
        {

            var service = GetCheckoutService();

            CheckoutSummary summary = await service.Checkout(userId);

            return ToApiCheckoutSummary(summary);
        }

        [Route("history/{userId}")]
        public async Task<IEnumerable<CheckoutSummaryViewModel>> GetHistory(string userId)
        {
            IEnumerable<CheckoutSummary> history = await GetCheckoutService().GetOrderHistory(userId);

            return history.Select(ToApiCheckoutSummary);
        }


        private CheckoutSummaryViewModel ToApiCheckoutSummary(CheckoutSummary model)
        {
            return new CheckoutSummaryViewModel
            {
                Products = model.Products.Select(p => new CheckoutProductViewModel
                {
                    ProductId = p.Product.Id,
                    ProductName = p.Product.Name,
                    Price = p.Price,
                    Quantity = p.Quantity
                }).ToList(),
                Date = model.Date,
                TotalPrice = model.TotalPrice
            };
        }

        private ICheckoutService GetCheckoutService()
        {
            long key = LongRandom();

            var proxyFactory = new ServiceProxyFactory((c) => new FabricTransportServiceRemotingClientFactory(
               serializationProvider: new GenericDataProvider(new List<Type> { typeof(CheckoutSummary), typeof(List<CheckoutSummary>) })));

            var checkoutService = proxyFactory.CreateServiceProxy<ICheckoutService>(new Uri("fabric:/ECommerce/ECommerce.CheckoutService"), new ServicePartitionKey(key));

            return checkoutService;      
        }

        private long LongRandom()
        {
            byte[] buf = new byte[8];
            rnd.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);
            return longRand;
        }
    }
}
