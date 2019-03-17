using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeFactory;

namespace ControllerGenerator {
    public class ProductControllerGenerator {

        private const string FileName = "ProductController.cs";
        private const string ProductClassNameIdentifier = "Product";
        private const string FetchProductsMethodIdentifier = "FetchProducts";

        public static void Generate() {
            using (TextWriter writer = File.CreateText("ProductController.cs")) {
                var factory = CSharpFactory.Instance;
                DumpUsings(factory, writer);
                DumpController(factory, writer);
            }
        }

        /// <summary>
        /// using system.collections.generic;
        /// using system.linq;
        /// using system.web.http;
        /// using codefactorydemo.models;
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="writer"></param>
        private static void DumpUsings(AbstractCodeFactory factory, TextWriter writer) {
            factory.UsingDirective(
                factory.Identifier("System", "Collections", "Generic"),
                factory.Identifier("System", "Linq"),
                factory.Identifier("System", "Web", "Http"),
                factory.Identifier("CodeFactoryDemo", "Models")).Flush(writer);
        }

        private static void DumpController(AbstractCodeFactory factory, TextWriter writer) {
            var controllerClass = GetProductControllerClass(factory, writer);
            factory.DeclareNamespace(factory.Identifier("CodeFactoryDemo", "Controllers"), controllerClass).Flush(writer);
        }

        private static Statement GetProductControllerClass(AbstractCodeFactory factory, TextWriter writer) {
            var body = new List<Statement>();
            body.Add(GenerateFetchProducts(factory));
            body.Add(GenerateGetAllProducts(factory));
            body.Add(GenerateGetProductById(factory));
            return factory.DeclareClass("ProductsController", factory.Identifier("ApiController"), body.ToArray());
        }

        /// <summary>
        /// private Product[] FetchProducts() {
        ///    return new Product[] {
        ///        new Product { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 },
        ///        new Product { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M },
        ///        new Product { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M }
        ///    };
        /// }
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        private static Statement GenerateFetchProducts(AbstractCodeFactory factory) {
            var body = GenerateFetchProductsBody(factory);
            return factory.DeclareTypedMethod(Visibility.Private, $"{ProductClassNameIdentifier}[]", FetchProductsMethodIdentifier, Enumerable.Empty<TypedArgument>(), body);
        }

        private static Statement GenerateFetchProductsBody(AbstractCodeFactory factory) {
            return factory.Return(factory.NewArray(ProductClassNameIdentifier,
                GenerateNewProduct(factory, "1", "Tomato Soup", "Groceries", "1"),
                GenerateNewProduct(factory, "2", "Yo-yo", "Toys", "3.75M"),
                GenerateNewProduct(factory, "3", "Hammer", "Hardware", "16.99M")
                ));
        }

        private static Expression GenerateNewProduct(AbstractCodeFactory factory, string id, string name, string category, string price) {
            return factory.New(
                factory.Identifier(ProductClassNameIdentifier),
                factory.Identifier(id),
                factory.Identifier(factory.String(name)),
                factory.Identifier(factory.String(category)),
                factory.Identifier(price));
        }

        /// <summary>
        /// public IEnumerable<Product> GetAllProducts() {
        ///    return FetchProducts();
        /// }
        /// </summary>
        /// <param name="factory"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private static Statement GenerateGetAllProducts(AbstractCodeFactory factory) {
            var body = GenerateGetAllProductsBody(factory);
            return factory.DeclareTypedMethod(Visibility.Public, $"IEnumerable<{ProductClassNameIdentifier}>", "GetAllProducts", Enumerable.Empty<TypedArgument>(), body); 
        }

        private static Statement GenerateGetAllProductsBody(AbstractCodeFactory factory) {
            return factory.Return(factory.Call(factory.Identifier(FetchProductsMethodIdentifier)));
        }

        /// <summary>
        /// public IHttpActionResult GetProduct(int id) {
        ///     var product = FetchProducts().FirstOrDefault((p) => p.Id == id);
        ///     if (product == null) {
        ///         return NotFound();
        ///     }
        ///    return Ok(product);
        /// }
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        private static Statement GenerateGetProductById(AbstractCodeFactory factory) {
            var body = GenerateGetProductByIdBody(factory);
            var arg = new List<TypedArgument>();
            arg.Add(new TypedArgument("int", "id"));
            return factory.DeclareTypedMethod(Visibility.Public, "IHttpActionResult", "GetProduct", arg, body.ToArray());
        }

        private static List<Statement> GenerateGetProductByIdBody(AbstractCodeFactory factory) {
            const string productLambdaIdentifier = "p";
            const string productIdentifier = "product";
            var body = new List<Statement>();

            body.Add(factory.DeclareVar(productIdentifier,
                factory.Call(factory.Identifier(factory.Call(factory.Identifier(FetchProductsMethodIdentifier)), factory.Identifier("FirstOrDefault")),
                factory.Lambda(productLambdaIdentifier, factory.Eq(factory.Identifier(productLambdaIdentifier, "Id"), factory.Identifier("id"), ExpressionType.Integer)))));
            body.Add(factory.If(factory.IsNull(factory.Identifier(productIdentifier)),
                factory.Return(factory.Call(factory.Identifier("NotFound")))));
            body.Add(factory.Return(factory.Call(factory.Identifier("Ok"), factory.Identifier(productIdentifier))));
            return body;
        }
    }
}
