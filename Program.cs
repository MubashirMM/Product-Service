using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProductService.Data;
using ProductService.GraphQL.Mutations;
using ProductService.GraphQL.Queries;
using ProductService.Services.ProductRepositry;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

// 1. Clear default claim mappings so "sub" isn't remapped
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 2. Register Database & Repositories
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<ProductRepository>();

// 3. Configure RSA Public Key Verification
var pemPath = System.IO.Path.Combine(builder.Environment.ContentRootPath, "Keys", "public.pem");
if (!File.Exists(pemPath))
{
    throw new FileNotFoundException($"Public key file missing at expected path: {pemPath}");
}

var pemKey = File.ReadAllText(pemPath);
var rsa = RSA.Create();
rsa.ImportFromPem(pemKey);
var rsaSecurityKey = new RsaSecurityKey(rsa);

// 4. Register ASP.NET Core JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
           
            ValidateIssuer = true,
            ValidIssuer = "auth-service",

            ValidateAudience = true,
            ValidAudience = "product-service",

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaSecurityKey,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5) 

        };
    });

// 5. Register ASP.NET Core Authorization Policy Engine (on IServiceCollection)
builder.Services.AddAuthorization();

// 6. Register HotChocolate GraphQL Engine with Authorization Support
builder.Services
    .AddGraphQLServer()
    .AddQueryType<ProductQuery>()
    .AddMutationType<ProductMutation>()
    .AddAuthorization() // ✅ Enabled via HotChocolate.AspNetCore.Authorization
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

// 7. Configure Middleware Execution Pipeline Order
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.Run();