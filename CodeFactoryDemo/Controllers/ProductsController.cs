/*
 * This controller was generated using the ProductControllerGenerator.
 * It serves as a proof of concept for the use of the language abstraction Code Factory.
 */

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using CodeFactoryDemo.Models;

namespace CodeFactoryDemo.Controllers {
    public class ProductsController : ApiController {
        private Product[] FetchProducts() {
            return new Product[] { new Product(1, "Tomato Soup", "Groceries", 1), new Product(2, "Yo-yo", "Toys", 3.75M), new Product(3, "Hammer", "Hardware", 16.99M) };
        }
        public IEnumerable<Product> GetAllProducts() {
            return FetchProducts();
        }
        public IHttpActionResult GetProduct(int id) {
            var product = FetchProducts().FirstOrDefault((p) => (p.Id == id));
            if ((product == null)) {
                return NotFound();
            }

            return Ok(product);
        }
    }

}


