using MongoDB.Driver;
using ProductService.Model;

namespace ProductService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            var clinet = new MongoClient(config["MongoDb:ConnectionString"]);
            _database = clinet.GetDatabase(config["MongoDb:DatabaseName"]);
        }

        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
    }
}
