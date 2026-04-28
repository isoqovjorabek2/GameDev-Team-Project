# GameDev-Team-Project
# 🎮 Unity Game Backend

This is a backend server for a Unity game built with:

* Node.js
* Express.js
* MongoDB
* JWT Authentication

## 🚀 Features

* User registration & login
* JWT authentication
* Save player score
* Leaderboard system

## 📦 Installation

```bash
npm install
```

## ▶️ Run Server

```bash
node server.js
```

## 🔐 Environment Variables

Create a `.env` file:

```
PORT=5001
MONGO_URI=mongodb://127.0.0.1:27017/unity_game
JWT_SECRET=your_secret_key
```

## 📡 API Endpoints

### Auth

* POST `/api/auth/register`
* POST `/api/auth/login`

### Game

* POST `/api/game/score`
* GET `/api/game/leaderboard`

## 🧠 Author
Jasurbek


