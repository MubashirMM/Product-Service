using System.Security.Claims;
using HotChocolate.Authorization; // Required for [Authorize]
using ProductService.Model;
using ProductService.Services.ProductRepositry;

namespace ProductService.GraphQL.Queries;

public class ProductQuery
{
    // Open to everyone — no JWT token required
    public string Health() => "OK";

    [Authorize] // 🔒 Requires valid JWT token
    public async Task<List<Product>> GetProducts(
        ClaimsPrincipal user,
        [Service] ProductRepository repo)
        => await repo.GetByUserAsync(GetUserId(user));

    [Authorize] // 🔒 Requires valid JWT token
    public async Task<Product?> GetProduct(
        string id,
        ClaimsPrincipal user,
        [Service] ProductRepository repo)
        => await repo.GetByIdAndUserAsync(id, GetUserId(user));

    [Authorize] // 🔒 Requires valid JWT token
    public string WhoAmI(ClaimsPrincipal user)
        => $"user: {GetUserId(user)}";

    private static string GetUserId(ClaimsPrincipal user)
        => user.FindFirst("sub")?.Value
        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new GraphQLException("No user id in token.");
}