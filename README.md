# Unity Game Backend

A comprehensive backend server for Unity games with MongoDB and real-time multiplayer support.

## Features

- Real-time Multiplayer with Socket.io
- JWT Authentication
- Game Management System
- Room-based Multiplayer
- Chat System
- Leaderboards
- Player Stats & Inventory

## Quick Start

1. Install dependencies: `npm install`
2. Set up .env file with MongoDB URI and JWT secret
3. Start server: `npm run dev`

## API Endpoints

### Authentication
- POST /api/auth/register
- POST /api/auth/login
- GET /api/auth/me

### Players
- GET /api/players/:id
- PUT /api/players/profile
- PUT /api/players/stats
- GET /api/players/leaderboard/all

### Games
- POST /api/games
- GET /api/games
- GET /api/games/:id
- POST /api/games/sessions
- POST /api/games/sessions/join
- GET /api/games/sessions/available

## Socket Events

### Client -> Server
- join_room, leave_room, create_room
- game_start, game_move, player_ready
- send_message, typing_start, typing_stop

### Server -> Client
- player_joined_room, player_left_room
- game_started, move_broadcast, game_state_updated
- new_message, user_typing, user_stopped_typing

## Unity Integration

Connect using Socket.io client with JWT authentication:

```csharp
var socket = new SocketIO("http://localhost:3000");
socket.ConnectAsync();
```

## Scripts

- npm run dev - Development server
- npm test - Run tests
- npm run lint - Code linting
- npm run format - Format code

## License

MIT
