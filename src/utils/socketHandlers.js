const jwt = require('jsonwebtoken');
const Player = require('../models/Player');
const GameSession = require('../models/GameSession');
const { roomManager } = require('./roomManager');
const { gameStateManager } = require('./gameState');
const { chatHandler } = require('./chatHandler');

// Store connected players
const connectedPlayers = new Map();

// Socket authentication middleware
const authenticateSocket = async (socket, next) => {
  try {
    const token = socket.handshake.auth.token || socket.handshake.headers.authorization?.replace('Bearer ', '');

    if (!token) {
      return next(new Error('Authentication required'));
    }

    const decoded = jwt.verify(token, process.env.JWT_SECRET);
    const player = await Player.findById(decoded.id).select('-password');

    if (!player) {
      return next(new Error('Player not found'));
    }

    socket.player = player;
    socket.playerId = player._id.toString();
    next();
  } catch (error) {
    next(new Error('Invalid token'));
  }
};

// Main socket handlers setup
const setupSocketHandlers = (io) => {
  // Authentication middleware
  io.use(authenticateSocket);

  // Connection handler
  io.on('connection', (socket) => {
    const playerId = socket.playerId;
    const username = socket.player.username;

    console.log(`Player connected: ${username} (${playerId})`);

    // Store connected player
    connectedPlayers.set(playerId, {
      socketId: socket.id,
      player: socket.player,
      connectedAt: Date.now()
    });

    // Join default lobby
    socket.join('lobby');

    // Notify others in lobby
    socket.to('lobby').emit('player_joined_lobby', {
      playerId,
      username,
      onlineCount: connectedPlayers.size
    });

    // Send current online count to the connected player
    socket.emit('lobby_info', {
      onlineCount: connectedPlayers.size,
      players: Array.from(connectedPlayers.values()).map(p => ({
        playerId: p.player._id.toString(),
        username: p.player.username,
        level: p.player.profile.level
      }))
    });

    // ===== ROOM MANAGEMENT =====

    // Join game room
    socket.on('join_room', async (data) => {
      try {
        const { roomId } = data;

        // Validate room exists
        const roomInfo = roomManager.getRoomInfo(roomId);
        if (!roomInfo) {
          socket.emit('error', { message: 'Room not found' });
          return;
        }

        // Check if player is already in room
        if (roomInfo.players.has(playerId)) {
          socket.emit('error', { message: 'Already in room' });
          return;
        }

        // Join room
        socket.join(roomId);
        roomManager.addPlayerToRoom(roomId, playerId, socket.player);

        // Notify room
        io.to(roomId).emit('player_joined_room', {
          playerId,
          username,
          playerCount: roomInfo.players.size
        });

        // Send room state to new player
        socket.emit('room_state', {
          roomId,
          players: Array.from(roomInfo.players.values()),
          gameState: roomInfo.gameState
        });

        console.log(`${username} joined room: ${roomId}`);
      } catch (error) {
        console.error('Join room error:', error);
        socket.emit('error', { message: 'Failed to join room' });
      }
    });

    // Leave game room
    socket.on('leave_room', async (data) => {
      try {
        const { roomId } = data;

        // Remove from room
        roomManager.removePlayerFromRoom(roomId, playerId);
        socket.leave(roomId);

        // Notify room
        io.to(roomId).emit('player_left_room', {
          playerId,
          username,
          playerCount: roomManager.getRoomInfo(roomId)?.players.size || 0
        });

        console.log(`${username} left room: ${roomId}`);
      } catch (error) {
        console.error('Leave room error:', error);
        socket.emit('error', { message: 'Failed to leave room' });
      }
    });

    // Create game room
    socket.on('create_room', async (data) => {
      try {
        const { gameType, gameSettings, maxPlayers } = data;

        // Create room
        const roomId = roomManager.createRoom({
          creatorId: playerId,
          creatorName: username,
          gameType,
          gameSettings,
          maxPlayers
        });

        // Join the created room
        socket.join(roomId);
        roomManager.addPlayerToRoom(roomId, playerId, socket.player);

        socket.emit('room_created', {
          roomId,
          roomInfo: roomManager.getRoomInfo(roomId)
        });

        console.log(`${username} created room: ${roomId}`);
      } catch (error) {
        console.error('Create room error:', error);
        socket.emit('error', { message: 'Failed to create room' });
      }
    });

    // ===== GAME EVENTS =====

    // Start game
    socket.on('game_start', async (data) => {
      try {
        const { roomId } = data;

        const roomInfo = roomManager.getRoomInfo(roomId);
        if (!roomInfo) {
          socket.emit('error', { message: 'Room not found' });
          return;
        }

        // Check if player is room creator
        if (roomInfo.creatorId !== playerId) {
          socket.emit('error', { message: 'Only room creator can start game' });
          return;
        }

        // Check minimum players
        if (roomInfo.players.size < roomInfo.minPlayers) {
          socket.emit('error', { message: 'Not enough players to start' });
          return;
        }

        // Initialize game state
        const initialState = gameStateManager.initializeGameState(roomInfo);
        roomManager.updateGameState(roomId, initialState);
        roomManager.setRoomStatus(roomId, 'playing');

        // Notify all players
        io.to(roomId).emit('game_started', {
          roomId,
          gameState: initialState,
          startTime: Date.now()
        });

        // Start game timers if applicable
        if (roomInfo.gameSettings.roundTime) {
          startGameTimer(io, roomId, roomInfo.gameSettings.roundTime);
        }

        console.log(`Game started in room: ${roomId}`);
      } catch (error) {
        console.error('Game start error:', error);
        socket.emit('error', { message: 'Failed to start game' });
      }
    });

    // Game move/action
    socket.on('game_move', async (data) => {
      try {
        const { roomId, move, moveData } = data;

        const roomInfo = roomManager.getRoomInfo(roomId);
        if (!roomInfo) {
          socket.emit('error', { message: 'Room not found' });
          return;
        }

        // Validate player is in room
        if (!roomInfo.players.has(playerId)) {
          socket.emit('error', { message: 'Not in this room' });
          return;
        }

        // Validate move (game-specific logic)
        const validationResult = gameStateManager.validateMove(roomInfo, playerId, move, moveData);
        if (!validationResult.valid) {
          socket.emit('move_rejected', {
            reason: validationResult.reason,
            move,
            moveData
          });
          return;
        }

        // Apply move to game state
        const updatedState = gameStateManager.applyMove(roomInfo, playerId, move, moveData);
        roomManager.updateGameState(roomId, updatedState);

        // Broadcast move to all players
        io.to(roomId).emit('move_broadcast', {
          playerId,
          username,
          move,
          moveData,
          timestamp: Date.now()
        });

        // Send updated game state
        io.to(roomId).emit('game_state_updated', {
          gameState: updatedState,
          lastMove: { playerId, username, move, moveData }
        });

        // Check win condition
        const winResult = gameStateManager.checkWinCondition(roomInfo);
        if (winResult.winner) {
          handleGameWin(io, roomId, winResult);
        }

      } catch (error) {
        console.error('Game move error:', error);
        socket.emit('error', { message: 'Failed to process move' });
      }
    });

    // Player ready state
    socket.on('player_ready', async (data) => {
      try {
        const { roomId, isReady } = data;

        const roomInfo = roomManager.getRoomInfo(roomId);
        if (!roomInfo) return;

        roomManager.setPlayerReady(roomId, playerId, isReady);

        io.to(roomId).emit('player_ready_changed', {
          playerId,
          username,
          isReady
        });

        // Check if all players are ready
        const allReady = roomManager.areAllPlayersReady(roomId);
        if (allReady && roomInfo.players.size >= roomInfo.minPlayers) {
          io.to(roomId).emit('all_players_ready', { canStart: true });
        }

      } catch (error) {
        console.error('Player ready error:', error);
      }
    });

    // ===== CHAT EVENTS =====

    // Send chat message
    socket.on('send_message', (data) => {
      chatHandler.handleSendMessage(io, socket, data);
    });

    // Typing indicator
    socket.on('typing_start', (data) => {
      chatHandler.handleTypingStart(io, socket, data);
    });

    socket.on('typing_stop', (data) => {
      chatHandler.handleTypingStop(io, socket, data);
    });

    // ===== PLAYER EVENTS =====

    // Update player status
    socket.on('player_status_update', (data) => {
      try {
        const { status } = data;

        // Update in connected players
        const playerInfo = connectedPlayers.get(playerId);
        if (playerInfo) {
          playerInfo.status = status;
        }

        // Broadcast to rooms player is in
        socket.rooms.forEach(room => {
          if (room !== socket.id && room !== 'lobby') {
            io.to(room).emit('player_status_updated', {
              playerId,
              username,
              status
            });
          }
        });

      } catch (error) {
        console.error('Player status update error:', error);
      }
    });

    // Request reconnection
    socket.on('request_reconnect', async (data) => {
      try {
        const { roomId } = data;

        const roomInfo = roomManager.getRoomInfo(roomId);
        if (!roomInfo) {
          socket.emit('reconnect_failed', { reason: 'Room not found' });
          return;
        }

        // Rejoin room
        socket.join(roomId);
        roomManager.addPlayerToRoom(roomId, playerId, socket.player);

        // Send current state
        socket.emit('reconnect_success', {
          roomId,
          gameState: roomInfo.gameState,
          players: Array.from(roomInfo.players.values())
        });

        // Notify others
        socket.to(roomId).emit('player_reconnected', {
          playerId,
          username
        });

        console.log(`${username} reconnected to room: ${roomId}`);
      } catch (error) {
        console.error('Reconnect error:', error);
        socket.emit('reconnect_failed', { reason: 'Reconnection failed' });
      }
    });

    // ===== DISCONNECTION =====

    socket.on('disconnect', async () => {
      console.log(`Player disconnected: ${username} (${playerId})`);

      // Remove from connected players
      connectedPlayers.delete(playerId);

      // Notify lobby
      io.to('lobby').emit('player_left_lobby', {
        playerId,
        username,
        onlineCount: connectedPlayers.size
      });

      // Handle room disconnections
      socket.rooms.forEach(roomId => {
        if (roomId !== socket.id && roomId !== 'lobby') {
          const roomInfo = roomManager.getRoomInfo(roomId);
          if (roomInfo) {
            // Mark player as disconnected
            roomManager.setPlayerDisconnected(roomId, playerId);

            // Notify room
            io.to(roomId).emit('player_disconnected', {
              playerId,
              username,
              playerCount: roomInfo.players.size
            });

            // Check if room should be cleaned up
            if (roomManager.shouldCleanupRoom(roomId)) {
              handleRoomCleanup(io, roomId);
            }
          }
        }
      });

      // Update player's last login time
      try {
        await Player.findByIdAndUpdate(playerId, { lastLogin: Date.now() });
      } catch (error) {
        console.error('Error updating player last login:', error);
      }
    });

    // Error handling
    socket.on('error', (error) => {
      console.error(`Socket error for ${username}:`, error);
    });
  });

  // ===== HELPER FUNCTIONS =====

  // Game timer
  const startGameTimer = (io, roomId, duration) => {
    const timer = setInterval(() => {
      const roomInfo = roomManager.getRoomInfo(roomId);
      if (!roomInfo || roomInfo.status !== 'playing') {
        clearInterval(timer);
        return;
      }

      const timeLeft = roomInfo.gameState.timeLeft - 1;
      roomInfo.gameState.timeLeft = timeLeft;

      io.to(roomId).emit('timer_update', { timeLeft });

      if (timeLeft <= 0) {
        clearInterval(timer);
        handleGameTimeout(io, roomId);
      }
    }, 1000);

    roomManager.setRoomTimer(roomId, timer);
  };

  // Handle game win
  const handleGameWin = (io, roomId, winResult) => {
    const roomInfo = roomManager.getRoomInfo(roomId);
    if (!roomInfo) return;

    roomManager.setRoomStatus(roomId, 'completed');

    // Update player stats
    updatePlayerStats(winResult.winner, roomInfo.players, true);

    io.to(roomId).emit('game_ended', {
      roomId,
      winner: winResult.winner,
      reason: winResult.reason,
      finalState: roomInfo.gameState,
      endTime: Date.now()
    });

    // Schedule room cleanup
    setTimeout(() => handleRoomCleanup(io, roomId), 30000); // 30 seconds
  };

  // Handle game timeout
  const handleGameTimeout = (io, roomId) => {
    const roomInfo = roomManager.getRoomInfo(roomId);
    if (!roomInfo) return;

    roomManager.setRoomStatus(roomId, 'completed');

    // Determine winner by score
    const winner = gameStateManager.determineWinnerByScore(roomInfo);

    io.to(roomId).emit('game_ended', {
      roomId,
      winner,
      reason: 'timeout',
      finalState: roomInfo.gameState,
      endTime: Date.now()
    });

    setTimeout(() => handleRoomCleanup(io, roomId), 30000);
  };

  // Handle room cleanup
  const handleRoomCleanup = async (io, roomId) => {
    try {
      const roomInfo = roomManager.getRoomInfo(roomId);
      if (!roomInfo) return;

      // Clear timer if exists
      if (roomInfo.timer) {
        clearInterval(roomInfo.timer);
      }

      // Save game session to database if completed
      if (roomInfo.status === 'completed') {
        await saveGameSession(roomInfo);
      }

      // Remove room
      roomManager.deleteRoom(roomId);

      // Notify remaining players
      io.to(roomId).emit('room_closed', { roomId });

      // Make all players leave the room
      io.in(roomId).socketsLeave(roomId);

      console.log(`Room cleaned up: ${roomId}`);
    } catch (error) {
      console.error('Room cleanup error:', error);
    }
  };

  // Update player stats
  const updatePlayerStats = async (winnerId, players, isWin) => {
    try {
      const updates = players.map(async (player) => {
        const isWinner = player.playerId === winnerId;
        await Player.findByIdAndUpdate(player.playerId, {
          $inc: {
            'stats.gamesPlayed': 1,
            'stats.gamesWon': isWinner ? 1 : 0,
            'stats.totalScore': player.score || 0
          },
          $max: {
            'stats.highScore': player.score || 0
          }
        });
      });

      await Promise.all(updates);
    } catch (error) {
      console.error('Error updating player stats:', error);
    }
  };

  // Save game session to database
  const saveGameSession = async (roomInfo) => {
    try {
      const session = new GameSession({
        gameId: roomInfo.gameId,
        players: Array.from(roomInfo.players.values()).map(p => ({
          playerId: p.playerId,
          username: p.username,
          score: p.score || 0,
          isReady: p.isReady || false,
          joinedAt: p.joinedAt || Date.now()
        })),
        status: 'completed',
        gameState: roomInfo.gameState,
        settings: roomInfo.gameSettings,
        startTime: roomInfo.startTime,
        endTime: Date.now(),
        winner: roomInfo.gameState.winner
      });

      await session.save();
      console.log(`Game session saved: ${session._id}`);
    } catch (error) {
      console.error('Error saving game session:', error);
    }
  };
};

module.exports = { setupSocketHandlers, connectedPlayers };