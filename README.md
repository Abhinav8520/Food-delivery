# Food Delivery App

A full-stack food delivery application built with the MERN stack (MongoDB, Express.js, React, Node.js).

## Project Structure

```
food-delivery/
├── frontend/          # Customer-facing React application
├── backend/           # Node.js/Express API server
├── admin/             # Admin panel React application
└── README.md          # This file
```

## Tech Stack

- **Frontend & Admin**: React 18, Vite, React Router, Axios
- **Backend**: Node.js, Express.js, MongoDB, Mongoose, JWT, Stripe
- **AI Features**: OpenAI-powered chatbot for customer support

## Prerequisites

- Node.js (v16 or higher)
- MongoDB
- npm or yarn

## Getting Started

### 1. Install Dependencies
```bash
cd backend && npm install
cd ../frontend && npm install
cd ../admin && npm install
```

### 2. Environment Setup

Create `.env` files in each directory:

**Backend (.env)**
```env
PORT=5000
MONGODB_URI=mongodb://localhost:27017/food-delivery
JWT_SECRET=your-secret-key
STRIPE_SECRET_KEY=your-stripe-secret-key
OPENAI_API_KEY=your-openai-api-key
```

**Frontend (.env)**
```env
VITE_API_URL=http://localhost:5000
```

**Admin (.env)**
```env
VITE_API_URL=http://localhost:5000
```

### 3. Start the Application

```bash
# Backend
cd backend && npm run server

# Frontend (Customer App)
cd frontend && npm run dev

# Admin Panel
cd admin && npm run dev
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run server` - Start backend server (backend only)
