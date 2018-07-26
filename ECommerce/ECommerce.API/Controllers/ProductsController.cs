using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.API.ViewModels;
using ECommerce.ProductCatalog.Model;
using ECommerce.ProductCatalog.Model.Classes;
using ECommerce.ProductCatalog.Model.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {

        private readonly IProductCatalogService _catalogService;

        public ProductsController()
        {
            var proxyFactory = new ServiceProxyFactory((c) => new FabricTransportServiceRemotingClientFactory(
                serializationProvider: new GenericDataProvider(new List<Type> { typeof(Product), typeof(List<Product>) })));

            _catalogService = proxyFactory.CreateServiceProxy<IProductCatalogService>(new Uri("fabric:/ECommerce/ECommerce.ProductCatalog"), new ServicePartitionKey(0));           

            ///_catalogService = ServiceProxy.Create<IProductCatalogService>(new Uri("fabric:/ECommerce/ECommerce.ProductCatalog"), new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(0));
        }
        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<ProductViewModel>> Get()
        {
            try
            {
                var result = await _catalogService.GetAllProducts();

                return result.Select(x => new ProductViewModel
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price
                });
            }
            catch (Exception ex)
            {
                string er = ex.ToString();
            }
            return null;
        }


        // POST api/values
        [HttpPost]
        public async Task Post(ProductViewModel product)
        {
            await _catalogService.AddProduct(new ProductCatalog.Model.Product { Id = Guid.NewGuid(), Description = product.Description,  Name = product.Name, Price = product.Price });
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
