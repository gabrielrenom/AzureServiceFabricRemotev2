using System;

namespace ECommerce.ProductCatalog.Model
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { set; get; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Availability { get; set; }
    }
}
