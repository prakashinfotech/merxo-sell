# 🐳 MerxoSell — Docker Setup Guide

This guide explains how to run the entire **MerxoSell** multi-vendor e-commerce platform using Docker & Docker Compose.

---

## 🛠️ Architecture & Services

The Docker setup orchestrates 3 main services:

| Service | Technology | Port (Host) | Internal Port | Description |
| :--- | :--- | :--- | :--- | :--- |
| **`db`** | MS SQL Server 2022 | `1433` | `1433` | Database instance storing product, order, and user data. |
| **`backend`** | .NET 8.0 Web API | `5000` | `5000` | Core REST API, EF Core migrations & auto-seeding. |
| **`frontend`** | Angular 19 + Nginx | `4200` | `80` | High-performance compiled single-page application. |

---

## 🚀 Quick Start

### 1. Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### 2. Launch the Application

Run the following command from the project root directory:

```bash
docker compose up -d --build
```

Docker will:
1. Pull the official SQL Server 2022 image.
2. Build the ASP.NET Core API image.
3. Build the Angular 19 production bundle and configure Nginx.
4. Start SQL Server, wait until it passes health checks, and launch the backend API.
5. Apply database migrations automatically on boot and seed initial default users.
6. Launch the frontend container.

---

## 🌐 Accessing Services

- **Frontend SPA**: [http://localhost:4200](http://localhost:4200)
- **Backend API Base**: [http://localhost:5000/api](http://localhost:5000/api)
- **SQL Server Connection**: `Server=localhost,1433;Database=MerxoSellDb;User Id=sa;Password=YourStrongPass123!;TrustServerCertificate=True;`

---

## 🔑 Default Login Credentials

| Role | Email | Password |
| :--- | :--- | :--- |
| **Super Admin** | `admin@merxosell.com` | `Admin@123` |
| **Seller** | `seller@merxosell.com` | `Seller@123` |

---

## 📋 Common Docker Commands

### View Service Logs
```bash
# View logs from all services
docker compose logs -f

# View backend logs only
docker compose logs backend -f

# View database logs only
docker compose logs db -f
```

### Stop Containers
```bash
# Stop containers keeping volumes intact
docker compose down

# Stop containers and remove persisted volumes (resets database)
docker compose down -v
```

### Rebuild Specific Service
```bash
# Rebuild and restart frontend
docker compose up -d --build frontend

# Rebuild and restart backend
docker compose up -d --build backend
```
