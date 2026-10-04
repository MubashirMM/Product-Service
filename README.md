# 🛍️ Product Service

Product catalog service for a microservices platform — **.NET 10, GraphQL (Hot Chocolate) and MongoDB**. Verifies RS256 JWTs issued by the [Auth Service](https://github.com/MubashirMM/Auth-service) locally using its public key — no call back to the auth service at request time.

Part of a polyglot microservices architecture — the Auth Service is Java/Spring Boot/MySQL, this service is .NET/GraphQL/MongoDB. Different stacks, one shared trust model.

---

## 🏗️ Architecture

```javascript
                        signs JWT with PRIVATE key
 ┌────────────┐  login  ┌─────────────────┐   public key only   ┌──────────────────┐
 │  Client    │────────▶│  Auth Service   │────────────────────▶│ Product Service  │
 └────────────┘  JWT    │ Java · Spring   │   (safe to share)   │ .NET 10 · GraphQL│
        │               │ Boot · MySQL    │                     │ MongoDB          │
        │               └─────────────────┘                     └──────────────────┘
        │                                                          verifies token
        └───────────────────── Authorization: Bearer <token> ─────── locally
```

- **Auth Service** — the *only* holder of the private key. Signs tokens, owns user data.
- **Product Service** (this repo) — holds only the *public key*. Verifies every request locally in milliseconds.

## 🧰 Tech Stack

| Layer | Technology |
| --- | --- |
| Language | C# · .NET 10 |
| API | GraphQL — Hot Chocolate 16 (`/graphql`, Banana Cake Pop UI) |
| Database | MongoDB (driver 3.12) |
| Auth | JWT Bearer (RS256) — public-key verification of Auth Service tokens |
| Build | .NET CLI / `ProductService.slnx` |

## ✨ Features

- Product catalog: create, read, update, delete
- Single GraphQL endpoint (`/graphql`) with built-in Banana Cake Pop IDE
- MongoDB persistence (auto-generated `ObjectId`s)
- Stateless JWT authentication — tokens verified locally against the Auth Service's public key

## 🔑 How authentication works here

1. Client logs in via the Auth Service → receives an RS256 JWT (`sub` = user id, `iss` = `auth-service`, `aud` = `product-service`, 15 min expiry)
2. Client calls this service with `Authorization: Bearer <token>`
3. The JWT Bearer middleware verifies the **signature with the public key**, plus expiry, issuer and audience — pure cryptography, no network call, no shared secret
4. Because only the public key is present here, this service can **verify but never forge** tokens

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK
- MongoDB running locally (or via Docker below)

### Setup

```bash
# 1. Clone
git clone https://github.com/MubashirMM/Product-Service.git
cd Product-Service

# 2. Start MongoDB (no user setup needed — Mongo creates the DB on first write)
docker run -d --name mongo -p 27017:27017 mongo:8
# (or use an existing local MongoDB)

# 3. Copy the Auth Service's PUBLIC key into this project
mkdir keys
cp ../Auth-service/keys/public.pem keys/public.pem

# 4. Run
dotnet restore
dotnet run
```

Service starts on `http://localhost:5xxx/graphql` (check console output; Banana Cake Pop IDE opens at `/graphql`).

## ⚙️ Configuration

`appsettings.json` holds non-secret structure; per-machine values can be overridden with environment variables (`MongoDb__ConnectionString`, `Jwt__PublicKeyPath`, …) or `appsettings.Development.json`.

| Setting | Description | Default |
| --- | --- | --- |
| `MongoDb:ConnectionString` | MongoDB connection string | `mongodb://localhost:27017` |
| `MongoDb:DatabaseName` | Database name | `ProductDb` |
| `Jwt:PublicKeyPath` | Path to the Auth Service's public PEM | `keys/public.pem` |
| `Jwt:Issuer` | Expected `iss` claim | `auth-service` |

## 📡 API Reference

**Endpoint:** `http://localhost:<port>/graphql` — explore interactively via Banana Cake Pop at `/graphql`.

| Type | Field | Auth | Description |
| --- | --- | --- | --- |
| Query | `products` | Public | List all products |
| Query | `product(id: String!)` | Public | Get one product by id |
| Mutation | `createProduct(name, price, category, stock)` | Bearer | Add a product |
| Mutation | `updateProduct(id, name, price, category, stock)` | Bearer | Replace a product |
| Mutation | `deleteProduct(id: String!)` | Bearer | Delete a product |

### Get a token from the Auth Service

```bash
curl -X POST http://localhost:8080/app/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"email@gmail.com","password":"password"}'
```

### Query products (public)

```graphql
query {
  products {
    id
    name
    price
    category
    stock
  }
}
```

### Create a product (Bearer token required)

```graphql
mutation {
  createProduct(
    name: "Wireless Mouse"
    price: 29.99
    category: "Electronics"
    stock: 50
  ) {
    id
    name
    price
    category
    stock
  }
}
```

In Banana Cake Pop / HTTP headers:

```json
{ "Authorization": "Bearer <token-from-auth-service>" }
```

## 📁 Project Structure

```javascript
Product-Service/
├── keys/
│   └── public.pem            # Auth Service's PUBLIC key (safe to share)
├── Data/
│   └── MongoDbContext.cs     # Mongo client + Products collection
├── GraphQL/
│   ├── Queries/
│   │   └── ProductQuery.cs   # products, product(id)
│   └── Mutations/
│       └── ProductMutation.cs# createProduct, updateProduct, deleteProduct
├── Model/
│   └── Product.cs            # Mongo document (ObjectId, name, price, category, stock)
├── Services/
│   └── ProductRepository.cs  # CRUD against MongoDB
├── Program.cs                # DI + Hot Chocolate server, MapGraphQL()
├── appsettings.json
└── ProductService.csproj
```

## 🔒 Security Notes

- Only the **public** key lives in this repo — verification without forgeability.
- Token lifetime (15 min), issuer and audience are all enforced at verification time.
- In production, inject `MongoDb:ConnectionString` and key paths via your platform's secret manager (Docker/Kubernetes secrets, Vault, AWS Secrets Manager).

## 🔗 Related Services

| Service | Stack | Repo |
| --- | --- | --- |
| **Product Service** (this) | .NET 10 · GraphQL · MongoDB | you're here |
| **Auth Service** | Java · Spring Boot · MySQL | [github.com/MubashirMM/Auth-service](https://github.com/MubashirMM/Auth-service) |

## 🗺️ Roadmap

- [ ] Wire JWT Bearer validation in `Program.cs` (RS256 public key, issuer, audience)
- [ ] Per-user ownership: `OwnerId` on products + `[Authorize]` on mutations (user id from token `sub`)
- [ ] Role-based access (admin vs user)
- [ ] Pagination and filtering on `products` query
- [ ] Async events to other services (e.g. "product created") via message broker
- [ ] Docker + docker-compose for the whole platform

## 📄 License

MIT
