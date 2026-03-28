# SocialNet — Social Networking Platform

A full-stack social networking application built with **ASP.NET Core 8**, **SQL Server**, and **React + Vite**.

## 🏗️ Architecture

```
social-network-project/
├── backend/
│   ├── SocialNetwork.sln
│   └── SocialNetwork.API/          # ASP.NET Core 8 Web API
│       ├── Controllers/            # Auth, Users, Posts, Connections, Search
│       ├── Models/                 # EF Core entity models
│       ├── DTOs/                   # Data Transfer Objects
│       ├── Data/
│       │   ├── AppDbContext.cs     # EF Core DbContext
│       │   └── DataSeeder.cs       # 10 sample users + posts
│       ├── Services/
│       │   ├── AuthService.cs      # JWT auth
│       │   ├── RecommendationService.cs  # BFS-based friend suggestions
│       │   ├── SearchService.cs    # Trie-based search
│       │   └── PostRankingService.cs     # Heap-based trending
│       ├── Algorithms/
│       │   ├── GraphTraversal.cs   # BFS/DFS — O(V + E)
│       │   ├── Trie.cs             # Autocomplete — O(L)
│       │   └── MaxHeap.cs          # Top-K ranking — O(log n)
│       └── Program.cs              # App startup + middleware
└── frontend/
    └── src/
        ├── pages/                  # Login, Register, Home, UserList, UserProfile, Search, Connections
        ├── components/             # Navbar, PostCard, UserCard, ConnectButton
        ├── services/api.js         # Axios API client
        └── context/AuthContext.jsx # JWT auth state
```

## 🧠 Algorithm Implementation

| Feature | Algorithm | Complexity |
|---|---|---|
| Friend recommendations | BFS (second-degree connections) | O(V + E) |
| User suggestions ranking | Max-Heap with weighted score | O(n log k) |
| Search autocomplete | Trie prefix lookup | O(L) |
| Post trending | Heap + HN-style decay | O(n log k) |
| Shortest path (degrees) | BFS | O(V + E) |
| Connection check | DFS | O(V + E) |

### Suggestion Scoring Formula
```
score = (mutual_friends × 15)
      + (common_interests × 10)
      + (common_skills × 8)
      + (second_degree_bonus × 20)
      + (recency_boost × 5)
```

### Trending Post Score (Hacker News–style)
```
score = (likes×3 + comments×2 + views×0.1) / (hours_age ^ 1.8)
```

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (LocalDB or full instance)
- [Node.js 18+](https://nodejs.org)
- [npm or yarn](https://npmjs.com)

### Backend Setup

1. **Configure the connection string** in `backend/SocialNetwork.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=SocialNetworkDB;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

2. **Restore packages and run migrations**:
   ```bash
   cd backend/SocialNetwork.API
   dotnet restore
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Run the API**:
   ```bash
   dotnet run
   ```
   The API will be at `https://localhost:7001` and Swagger UI at `https://localhost:7001/swagger`

   > The database is automatically seeded with 10 sample users on first run!

### Frontend Setup

1. **Install dependencies**:
   ```bash
   cd frontend
   npm install
   ```

2. **Start the dev server**:
   ```bash
   npm run dev
   ```
   The app will be at `http://localhost:5173`

## 🔑 Sample Login Credentials

All sample users have the same password: `Password123!`

| Username | Name | Specialization |
|---|---|---|
| `alex_dev` | Alex Johnson | Full-Stack / C# |
| `sarah_ml` | Sarah Chen | Machine Learning |
| `mike_cloud` | Michael Rodriguez | Cloud / DevOps |
| `priya_design` | Priya Patel | UI/UX Design |
| `james_sec` | James Williams | Cybersecurity |
| `emily_data` | Emily Thompson | Data Science |
| `raj_mobile` | Raj Sharma | Mobile Dev |
| `lisa_devops` | Lisa Park | DevOps |
| `carlos_backend` | Carlos Mendez | Backend / ASP.NET |
| `zoe_ai` | Zoe Anderson | AI Research |

## 📋 API Endpoints

### Authentication
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login, returns JWT |

### Users
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/users` | Get all users (paginated) |
| GET | `/api/users/{id}` | Get user profile |
| GET | `/api/users/me` | Get current user |
| GET | `/api/users/suggestions` | Get AI-powered suggestions |
| GET | `/api/users/interests` | Get all interests |
| GET | `/api/users/skills` | Get all skills |

### Posts
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/posts/feed` | Personalized feed |
| GET | `/api/posts/trending` | Trending posts (Heap-ranked) |
| POST | `/api/posts` | Create a post |
| DELETE | `/api/posts/{id}` | Delete a post |
| POST | `/api/posts/{id}/like` | Toggle like |
| GET | `/api/posts/{id}/comments` | Get comments |
| POST | `/api/posts/{id}/comments` | Add comment |

### Connections
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/connections/request/{id}` | Send connection request |
| PUT | `/api/connections/accept/{id}` | Accept request |
| PUT | `/api/connections/reject/{id}` | Reject request |
| DELETE | `/api/connections/{id}` | Remove connection |
| GET | `/api/connections/my` | My connections |
| GET | `/api/connections/requests/received` | Pending requests |

### Search
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/search/users?q=` | Search users (Trie O(L)) |
| GET | `/api/search/autocomplete?prefix=` | Autocomplete suggestions |

## 🚢 Deployment (via GitHub Copilot / Azure)

### Option 1: Azure App Service + Azure SQL

1. Push this repo to GitHub
2. In Azure Portal: create an **Azure SQL Database** and an **App Service** (Linux, .NET 8)
3. Set environment variable `ConnectionStrings__DefaultConnection` in App Service → Configuration
4. Use GitHub Actions (auto-generated by Azure) for CI/CD

### Option 2: Docker

```bash
# Backend
cd backend/SocialNetwork.API
docker build -t socialnet-api .

# Frontend (build static)
cd frontend
npm run build
```

### GitHub Actions CI/CD
Create `.github/workflows/deploy.yml` using the Azure App Service publish profile.

## 🔐 Environment Variables

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string |
| `Jwt__Key` | JWT signing secret (min 32 chars) |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |
| `Jwt__ExpiryInDays` | Token expiry (default: 7) |

## 📦 Tech Stack

**Backend**
- ASP.NET Core 8 (Web API)
- Entity Framework Core 8 (ORM)
- SQL Server
- JWT Bearer Authentication
- BCrypt.Net (password hashing)
- Swagger / OpenAPI

**Frontend**
- React 18
- Vite
- React Router v6
- Axios (HTTP client)
- Tailwind CSS
- date-fns
- lucide-react (icons)
- react-hot-toast (notifications)
