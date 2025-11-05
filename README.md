# Food Delivery App

A full-stack food delivery application built with the MERN stack (MongoDB, Express.js, React, Node.js).

## Project Structure

```
food-delivery/
├── frontend/                    # Customer-facing React application
├── backend/                     # Node.js/Express API server
├── admin/                       # Admin panel React application
├── recommendation-service/      # C# ASP.NET Core recommendation microservice
├── start-app.ps1                # PowerShell startup script (recommended)
├── start-app.bat                # Batch startup script (alternative)
└── README.md                    # This file
```

## Tech Stack

- **Frontend & Admin**: React 18, Vite, React Router, Axios
- **Backend**: Node.js, Express.js, MongoDB, Mongoose, JWT, Stripe
- **AI Features**: OpenAI-powered chatbot for customer support

## Prerequisites

- Node.js (v16 or higher)
- .NET SDK 7.0 or higher (for C# Recommendation Service)
- MongoDB
- npm or yarn

## Getting Started

### 1. Install Dependencies
```bash
# Node.js services
cd backend && npm install
cd ../frontend && npm install
cd ../admin && npm install

# C# Recommendation Service
cd ../recommendation-service/RecommendationService
dotnet restore
```

### 2. Environment Setup

Create `.env` files in each directory:

**Backend (.env)**
```env
PORT=4000
MONGODB_URI=mongodb://localhost:27017/food-delivery
JWT_SECRET=your-secret-key
STRIPE_SECRET_KEY=your-stripe-secret-key
OPENAI_API_KEY=your-openai-api-key
RECOMMENDATION_SERVICE_URL=http://localhost:5001
```

**Frontend (.env)**
```env
VITE_API_URL=http://localhost:4000
```

**Admin (.env)**
```env
VITE_API_URL=http://localhost:4000
```

**Note:** The C# Recommendation Service reads from `backend/.env` automatically, so you only need to configure the backend `.env` file.

### 3. Start the Application

**Option 1: Quick Start (Recommended)**

Use the unified startup script to launch all services at once:

**Windows (PowerShell):**
```powershell
.\start-app.ps1
```

**Windows (Command Prompt):**
```cmd
start-app.bat
```

This will start all services in the correct order:
1. C# Recommendation Service (port 5001)
2. Node.js Backend (port 4000)
3. Frontend (port 5173)
4. Admin Panel (port 5174)

**Option 2: Manual Start**

If you prefer to start services manually:

```bash
# Terminal 1: C# Recommendation Service (must start first)
cd recommendation-service/RecommendationService
dotnet run

# Terminal 2: Node.js Backend
cd backend
npm run server

# Terminal 3: Frontend (Customer App)
cd frontend
npm run dev

# Terminal 4: Admin Panel
cd admin
npm run dev
```

**Important:** Start services in the order listed above, as the backend depends on the recommendation service.

## Available Scripts

- `.\start-app.ps1` or `start-app.bat` - Start all services at once (recommended)
- `npm run dev` - Start development server (frontend/admin)
- `npm run build` - Build for production
- `npm run server` - Start backend server (backend only)
- `dotnet run` - Start C# recommendation service

## Service URLs

Once all services are running, access your application at:

- **Frontend (Customer)**: http://localhost:5173
- **Admin Panel**: http://localhost:5174
- **Backend API**: http://localhost:4000
- **Recommendation Service**: http://localhost:5001
