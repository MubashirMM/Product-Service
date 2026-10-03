using ProductService.Data;
using ProductService.GraphQL.Mutations;
using ProductService.GraphQL.Queries;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<ProductRepository>();

// Registers GraphQL with Hot Chocolate
builder.Services
    .AddGraphQLServer()
    .AddQueryType<ProductQuery>()
    .AddMutationType<ProductMutation>();

var app = builder.Build();

app.MapGraphQL();   // endpoint: /graphql

app.Run();