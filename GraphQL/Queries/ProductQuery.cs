

using ProductService.Model;
using ProductService.Services;

namespace ProductService.GraphQL.Queries
{
    public class ProductQuery
    {

        public async Task<List<Product>> GetProducts(
            [Service] ProductRepository repo
            )
        {
            return await repo.GetAllAsync();
        }

        public async Task<Product?> GetProduct(
            string id,
            [Service] ProductRepository repo
            ) => await repo.GetByIdAsync(id);

    }
}