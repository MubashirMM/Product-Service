using System.Security.Claims;
// removed HotChocolate.Authorization to avoid schema directive issues when the package extensions are not available
using ProductService.Model;
using ProductService.Services.ProductRepositry;

namespace ProductService.GraphQL.Mutations;

public class ProductMutation
{
    public async Task<Product> CreateProduct(
        ClaimsPrincipal user,                          // ← from JWT, NOT from client
        [Service] ProductRepository repo,
        string name, decimal price, string category, int stock)
    {
        var userId = GetUserId(user);                  // helper below

        var product = new Product
        {
            UserId = userId,                           // stamped with owner
            Name = name,
            Price = price,
            Category = category,
            Stock = stock
        };
        return await repo.CreateAsync(product);
    }

    public async Task<bool> UpdateProduct(
        ClaimsPrincipal user,
        [Service] ProductRepository repo,
        string id, string name, decimal price, string category, int stock)
    {
        var userId = GetUserId(user);

        var product = new Product
        {
            Id = id,
            UserId = userId,                           // ownership preserved
            Name = name,
            Price = price,
            Category = category,
            Stock = stock
        };
        return await repo.UpdateAsync(id, userId, product);
    }

    public async Task<bool> DeleteProduct(
        ClaimsPrincipal user,
        [Service] ProductRepository repo,
        string id)
        => await repo.DeleteAsync(id, GetUserId(user));

    // ---- helper: extract stable user id from token ----
    private static string GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirst("sub")?.Value
              ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(id))
            throw new GraphQLException("Token has no user id (sub claim).");
        return id;
    }
}