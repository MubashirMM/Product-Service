using MongoDB.Driver;
using ProductService.Data;
using ProductService.Model;


namespace ProductService.Services;

public class ProductRepository
{
    private readonly MongoDbContext _context;

    public ProductRepository(MongoDbContext context) => _context = context;

    public async Task<List<Product>> GetAllAsync() =>
        await _context.Products.Find(_ => true).ToListAsync();

    public async Task<Product?> GetByIdAsync(string id) =>
        await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();

    public async Task<Product> CreateAsync(Product product)
    {
        await _context.Products.InsertOneAsync(product);
        return product;   // MongoDB fills in the Id after insert
    }

    public async Task<bool> UpdateAsync(string id, Product product)
    {
        var result = await _context.Products
            .ReplaceOneAsync(p => p.Id == id, product);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _context.Products
            .DeleteOneAsync(p => p.Id == id);
        return result.DeletedCount > 0;
    }
}