# 🎬 TMDB Backend API (.NET)

A production-style backend API built with **ASP.NET Core**, featuring **Google OAuth (OpenID Connect)** authentication, **JWT-based authorization**, and user-specific features like **favorite movies**.

---

## 🚀 Features

* 🔐 **Google Login (OAuth 2.0 + OpenID Connect)**
* 🎟️ **JWT Authentication for API security**
* ❤️ **User Favorite Movies (Toggle system)**
* 📊 **Top 10 Movies API (with caching)**
* ⚡ **In-Memory Caching for performance**
* 🗄️ **Entity Framework Core with Migrations**
* 🔍 Clean and scalable architecture

---

## 🧠 Tech Stack

* **.NET (ASP.NET Core Web API)**
* **Entity Framework Core**
* **SQL Server / SQLite**
* **Google OAuth (OIDC)**
* **JWT (JSON Web Tokens)**
* **IMemoryCache**
* **Postman (API testing)**

---

## 🔐 Authentication Flow

```text
User → Google Login
     → Google returns id_token (OIDC)
     → Backend validates token
     → Backend creates JWT
     → Client uses JWT for API calls
```

---

## 📌 API Endpoints

### 🔑 Auth

#### `POST /api/auth/google`

Login using Google ID token

**Request Body:**

```json
"id_token_here"
```

**Response:**

```json
{
  "token": "jwt_token"
}
```

---

### 🎬 Movies

#### `GET /api/movies/top10`

Returns top 10 movies (cached)

#### `PUT /api/movies`

Toggle favorite for a movie

**Headers:**

```
Authorization: Bearer <JWT>
```

**Body:**

```json
5
```

---

## ❤️ Favorites System

* One user can favorite multiple movies
* Implemented using a **many-to-many relationship**
* Uses a `UserFavorites` join table

---

## ⚡ Caching

Top 10 movies are cached using:

```csharp
IMemoryCache
```

This improves performance by avoiding repeated DB calls.

---

## 🗄️ Database & Migrations

### Create Migration

```bash
dotnet ef migrations add InitialCreate
```

### Update Database

```bash
dotnet ef database update
```

👉 Migrations are included and should be applied in all environments.

---

## 🧪 Testing (Postman)

### 1. Get JWT

```
POST /api/auth/google
```

### 2. Use JWT

```
Authorization: Bearer <JWT>
```

### 3. Call Protected APIs

```
PUT /api/movies
GET /api/movies/top10
```

---

## ⚠️ Common Issues

* ❌ 401 Unauthorized → Check JWT / Bearer header
* ❌ OAuth issues → Check Google Client ID + origin
* ❌ Slow queries → Add DB indexes (e.g., VoteCount)

---

## 📈 Future Improvements

* Refresh Token implementation
* Role-based authorization
* Pagination & filtering
* Distributed caching (Redis)
* Logging & monitoring
* Frontend integration (Angular/React)

---

## 🧠 Key Concepts Used

* OpenID Connect (Authentication)
* OAuth 2.0 (Authorization)
* JWT Bearer Tokens
* EF Core Migrations
* Caching strategies

---

## 👨‍💻 Author

Built as a learning + production-ready backend project.

---

## ⭐ Notes

This project demonstrates a **real-world backend architecture** with authentication, caching, and scalable design patterns.
