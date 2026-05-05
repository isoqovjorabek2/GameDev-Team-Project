# Game Frontend

Modern React frontend for the Unity Game Backend platform.

## Features

- **Authentication**: Login and registration with JWT tokens
- **Real-time Gaming**: Socket.io integration for live game sessions
- **Game Dashboard**: Browse available games and active sessions
- **Game Rooms**: Create and join game rooms with real-time chat
- **Leaderboard**: View top players and rankings
- **Profile Management**: Edit profile and view game statistics

## Tech Stack

- **React 18** - UI library
- **Vite** - Build tool and dev server
- **React Router** - Client-side routing
- **Socket.io Client** - Real-time communication
- **Zustand** - State management
- **Axios** - HTTP client
- **React Hook Form** - Form management
- **Styled Components** - CSS-in-JS
- **Lucide React** - Icon library
- **React Hot Toast** - Toast notifications

## Prerequisites

- Node.js 16+ and npm/yarn
- Running backend server on port 3000
- MongoDB database connection

## Installation

1. Install dependencies:
```bash
npm install
```

2. Configure environment variables:
```bash
# The .env file should contain:
VITE_API_URL=http://localhost:3000/api
```

3. Start the development server:
```bash
npm run dev
```

The frontend will be available at `http://localhost:5173`

## Build for Production

```bash
npm run build
```

The built files will be in the `dist` directory.

## Project Structure

```
game-frontend/
├── src/
│   ├── components/     # Reusable UI components
│   ├── pages/          # Page components
│   ├── services/       # API and socket services
│   ├── store/          # State management
│   ├── hooks/          # Custom React hooks
│   ├── utils/          # Utility functions
│   └── styles/         # Global styles
├── public/             # Static assets
└── index.html          # HTML entry point
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## API Integration

The frontend connects to the backend API at `/api` with the following endpoints:

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `GET /api/auth/me` - Get current user

### Games
- `GET /api/games` - Get all games
- `GET /api/games/:id` - Get game by ID
- `POST /api/games/sessions` - Create game session
- `POST /api/games/sessions/join` - Join game session
- `GET /api/games/sessions/available` - Get available sessions

### Players
- `GET /api/players/:id` - Get player by ID
- `PUT /api/players/profile` - Update profile
- `PUT /api/players/stats` - Update stats
- `GET /api/players/leaderboard/all` - Get leaderboard

## Socket Events

### Client → Server
- `join_room` - Join a game room
- `leave_room` - Leave a game room
- `create_room` - Create a new room
- `game_start` - Start the game
- `game_move` - Make a game move
- `player_ready` - Set ready status
- `send_message` - Send chat message
- `typing_start` - Start typing indicator
- `typing_stop` - Stop typing indicator

### Server → Client
- `room_state` - Current room state
- `player_joined_room` - Player joined notification
- `player_left_room` - Player left notification
- `player_ready_changed` - Ready status changed
- `all_players_ready` - All players ready
- `game_started` - Game started
- `game_state_updated` - Game state updated
- `move_broadcast` - Move broadcast
- `game_ended` - Game ended
- `timer_update` - Timer update
- `chat_message` - Chat message received

## Environment Variables

- `VITE_API_URL` - Backend API URL (default: http://localhost:3000/api)

## Development

The frontend uses Vite for fast development with hot module replacement. Changes to React components will automatically refresh the browser.

## Troubleshooting

### Connection Issues
- Ensure the backend server is running on port 3000
- Check that MongoDB is connected
- Verify CORS settings on the backend

### Socket Connection Issues
- Check that Socket.io server is running
- Verify WebSocket support in browser
- Check firewall/network settings

### Build Issues
- Clear node_modules and reinstall: `rm -rf node_modules && npm install`
- Clear Vite cache: `rm -rf node_modules/.vite`

## License

MIT