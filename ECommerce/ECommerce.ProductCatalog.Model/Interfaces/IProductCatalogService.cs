using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.ProductCatalog.Model.Interfaces
{
    public interface IProductCatalogService:IService
    {
        Task<List<Product>> GetAllProducts();
        Task AddProduct(Product product);
        Task<Product> GetProduct(Guid productId);
    }
}
