# E-Commerce Web Project

## 🔐 Setup Instructions

### 1. Configure Your Secrets

This project uses sensitive credentials (database passwords, API keys). They are **NOT** included in the repository for security.

#### **Option A: User Secrets (Recommended for Development)**

Right-click your project in Visual Studio:
```
Manage User Secrets
```

This opens your local secrets file (stored on your machine, NOT in Git).

Copy from `appsettings.Example.json` and add your actual credentials:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your_actual_connection_string"
  },
  "Stripe": {
    "PublishableKey": "your_publishable_key",
    "SecretKey": "your_secret_key"
  },
  "Resend": {
    "ApiKey": "your_resend_key"
  }
}
```

#### **Option B: Create `appsettings.Development.json` (Local Only)**

Add to root of project with your credentials. **This is already in `.gitignore`, so it won't be committed.**

### 2. Clone & Setup (For Other Developers)

1. Clone the repo
2. Right-click project → **Manage User Secrets**
3. Add the credentials from your team lead or `.env` file
4. Run the app

---

## 📦 Tech Stack

- **.NET 10** (C#)
- **ASP.NET Core MVC + Razor Pages**
- **Entity Framework Core** with PostgreSQL (Neon)
- **Stripe** for payments
- **Resend** for emails
- **ASP.NET Core Identity** for auth

---

## 🚀 Getting Started

1. Install dependencies: `dotnet restore`
2. Update database: `dotnet ef database update`
3. Run: `dotnet run`
4. Visit: `https://localhost:7000`

---

**Never commit `appsettings.json` with real credentials!**
