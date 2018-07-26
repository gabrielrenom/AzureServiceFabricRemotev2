using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.ProductCatalog.Classes;
using ECommerce.ProductCatalog.Interfaces;
using ECommerce.ProductCatalog.Model;
using ECommerce.ProductCatalog.Model.Classes;
using ECommerce.ProductCatalog.Model.Interfaces;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;


namespace ECommerce.ProductCatalog
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProductCatalog : StatefulService, IProductCatalogService
    {
        private IProductRepository _repo;

        public ProductCatalog(StatefulServiceContext context)
            : base(context){ }

       
        public async Task AddProduct(Product product)
        {
            await _repo.AddProduct(product);
        }
        public async Task<Product> GetProduct(Guid productId)
        {
            return await _repo.GetProduct(productId);
        }
        public async Task<List<Product>> GetAllProducts()
        {
            List<Product> result = (await _repo.GetAllProducts()).ToList();

            return result;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {           
            return new[]
            {
                new ServiceReplicaListener((context) =>
                {
                    return new FabricTransportServiceRemotingListener(context, this, serializationProvider: new GenericDataProvider(new List<Type>{ typeof(Product), typeof(List<Product>)}));                       
                })
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            _repo = new ServiceFabricProductRepository(this.StateManager);
            IEnumerable<Product> all = await _repo.GetAllProducts();
            await _repo.AddProduct(new Product { Id = Guid.NewGuid(), Name = "Policy", Description = "A keyboard", Availability = 1, Price = 12.12 });
            await _repo.AddProduct(new Product { Id = Guid.NewGuid(), Name = "Customer", Description = "Gaming laptop", Availability = 90, Price = 1312.12 });
            await _repo.AddProduct(new Product { Id = Guid.NewGuid(), Name = "Policies", Description = "Beautiful Screen", Availability = 13, Price = 12.12 });

            all = await _repo.GetAllProducts();
        }
    }
}
