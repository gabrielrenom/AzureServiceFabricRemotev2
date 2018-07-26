using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.CheckoutService.Model;
using ECommerce.ProductCatalog.Model;
using ECommerce.ProductCatalog.Model.Classes;
using ECommerce.ProductCatalog.Model.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using UserActor.Interfaces;

namespace ECommerce.CheckoutService
{
    internal sealed class CheckoutService : StatefulService, ICheckoutService
    {
        public CheckoutService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<CheckoutSummary> Checkout(string userId)
        {
            var result = new CheckoutSummary();
            result.Date = DateTime.UtcNow;
            result.Products = new List<CheckoutProduct>();

            //call user actor to get the basket
            IUserActor userActor = GetUserActor(userId);
            Dictionary<Guid, int> basket = await userActor.GetBasket();

            //get catalog client
            IProductCatalogService catalogService = GetProductCatalogService();

            //constuct CheckoutProduct items by calling to the catalog
            foreach (KeyValuePair<Guid, int> basketLine in basket)
            {
                Product product = await catalogService.GetProduct(basketLine.Key);
                var checkoutProduct = new CheckoutProduct
                {
                    Product = product,
                    Price = product.Price,
                    Quantity = basketLine.Value
                };
                result.Products.Add(checkoutProduct);
            }

            //generate total price
            result.TotalPrice = result.Products.Sum(p => p.Price);

            //clear user basket
            await userActor.ClearBasket();

            await AddToHistory(result);

            return result;
        }

        public async Task<List<CheckoutSummary>> GetOrderHistory(string userId)
        {
            var result = new List<CheckoutSummary>();
            var history = await StateManager.GetOrAddAsync<IReliableDictionary<DateTime, CheckoutSummary>>("history");

            using (var tx = StateManager.CreateTransaction())
            {
                var allProducts = await history.CreateEnumerableAsync(tx, EnumerationMode.Unordered);
                using (var enumerator = allProducts.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        KeyValuePair<DateTime, CheckoutSummary> current = enumerator.Current;

                        result.Add(current.Value);
                    }
                }
            }

            return result;
        }

        private async Task AddToHistory(CheckoutSummary checkout)
        {
            var history = await StateManager.GetOrAddAsync<IReliableDictionary<DateTime, CheckoutSummary>>("history");

            using (var tx = StateManager.CreateTransaction())
            {
                await history.AddAsync(tx, checkout.Date, checkout);

                await tx.CommitAsync();
            }
        }

        private IUserActor GetUserActor(string userId)
        {
            return ActorProxy.Create<IUserActor>(new ActorId(userId), new Uri("fabric:/ECommerce/UserActorService"));
        }

        private IProductCatalogService GetProductCatalogService()
        {

            var proxyFactory = new ServiceProxyFactory((c) => new FabricTransportServiceRemotingClientFactory(
               serializationProvider: new GenericDataProvider(new List<Type> { typeof(Product), typeof(List<Product>) })));

            var catalogService = proxyFactory.CreateServiceProxy<IProductCatalogService>(new Uri("fabric:/ECommerce/ECommerce.ProductCatalog"), new ServicePartitionKey(0));


            return (catalogService);
            //return ServiceProxy.Create<IProductCatalogService>(
            //   new Uri("fabric:/ECommerce/ProductCatalog"),
            //   new ServicePartitionKey(0));
        }


        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener((context) =>
                {
                    return new FabricTransportServiceRemotingListener(context, this, serializationProvider: new GenericDataProvider(new List<Type>{ typeof(CheckoutSummary), typeof(List<CheckoutSummary>)}));
                })
            };
        }
    }

}
