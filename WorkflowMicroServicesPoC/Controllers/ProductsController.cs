using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using WorkflowMicroServicesPoC.Interfaces;
using WorkflowMicroServicesPoC.Models;

namespace WorkflowMicroServicesPoC.Controllers
{
    public class ProductsController : ApiController
    {

        Product[] products = new Product[]
        {
                    new Product { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 },
                    new Product { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M },
                    new Product { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M }
        };
        private readonly ILogger _logger;

        public ProductsController(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<Product> Get()
        {
            _logger.Log("Product GET");
            return products;
        }

        public Product Get(int id)
        {
            var product = products.FirstOrDefault((p) => p.Id == id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            return products.Where(p => string.Equals(p.Category, category,
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
