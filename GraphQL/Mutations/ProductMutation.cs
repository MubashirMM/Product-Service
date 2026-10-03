using ProductService.Model;
using ProductService.Services;

namespace ProductService.GraphQL.Mutations;

public class ProductMutation
{
    public async Task<Product> CreateProduct(
        [Service] ProductRepository repo,
        string name, decimal price, string category, int stock)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            Category = category,
            Stock = stock
        };
        return await repo.CreateAsync(product);
    }

    public async Task<bool> UpdateProduct(
    [Service] ProductRepository repo,
    string id, string name, decimal price, string category, int stock)
    {
        var product = new Product
        {
            Id = id,          // keep the same id, we are replacing the doc
            Name = name,
            Price = price,
            Category = category,
            Stock = stock
        };
        return await repo.UpdateAsync(id, product);
    }

    public async Task<bool> DeleteProduct([Service] ProductRepository repo, string id)
        => await repo.DeleteAsync(id);
}