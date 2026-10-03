using MongoDB.Driver;
using ProductService.Data;
using ProductService.Model;

namespace ProductService.Services.ProductRepositry;

public class ProductRepository
{
    private readonly MongoDbContext _context;

    public ProductRepository(MongoDbContext context) => _context = context;

    // users see ONLY their own products
    public async Task<List<Product>> GetByUserAsync(string userId) =>
        await _context.Products.Find(p => p.UserId == userId).ToListAsync();

    // even "get one" checks ownership — user B can't read user A's product by guessing the id
    public async Task<Product?> GetByIdAndUserAsync(string id, string userId) =>
        await _context.Products
            .Find(p => p.Id == id && p.UserId == userId)
            .FirstOrDefaultAsync();

    public async Task<Product> CreateAsync(Product product)
    {
        await _context.Products.InsertOneAsync(product);
        return product;
    }

    public async Task<bool> UpdateAsync(string id, string userId, Product product)
    {
        var result = await _context.Products
            .ReplaceOneAsync(p => p.Id == id && p.UserId == userId, product);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var result = await _context.Products
            .DeleteOneAsync(p => p.Id == id && p.UserId == userId);
        return result.DeletedCount > 0;
    }
}