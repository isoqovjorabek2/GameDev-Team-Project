// Room management system for game sessions

class RoomManager {
  constructor() {
    this.rooms = new Map();
    this.playerRooms = new Map(); // Track which room each player is in
  }

  // Create a new room
  createRoom({ creatorId, creatorName, gameType, gameSettings, maxPlayers = 2, minPlayers = 1 }) {
    const roomId = this.generateRoomId();

    const room = {
      id: roomId,
      creatorId,
      creatorName,
      gameType,
      gameSettings: gameSettings || {},
      maxPlayers,
      minPlayers,
      players: new Map(),
      gameState: {},
      status: 'waiting', // waiting, playing, completed
      createdAt: Date.now(),
      startTime: null,
      timer: null
    };

    this.rooms.set(roomId, room);
    console.log(`Room created: ${roomId} by ${creatorName}`);

    return roomId;
  }

  // Get room information
  getRoomInfo(roomId) {
    return this.rooms.get(roomId);
  }

  // Add player to room
  addPlayerToRoom(roomId, playerId, player) {
    const room = this.rooms.get(roomId);
    if (!room) {
      throw new Error('Room not found');
    }

    if (room.players.size >= room.maxPlayers) {
      throw new Error('Room is full');
    }

    if (room.players.has(playerId)) {
      throw new Error('Player already in room');
    }

    room.players.set(playerId, {
      playerId,
      username: player.username,
      avatar: player.profile?.avatar,
      level: player.profile?.level || 1,
      score: 0,
      isReady: false,
      isDisconnected: false,
      joinedAt: Date.now()
    });

    this.playerRooms.set(playerId, roomId);
    console.log(`Player ${player.username} added to room ${roomId}`);

    return room;
  }

  // Remove player from room
  removePlayerFromRoom(roomId, playerId) {
    const room = this.rooms.get(roomId);
    if (!room) {
      throw new Error('Room not found');
    }

    room.players.delete(playerId);
    this.playerRooms.delete(playerId);

    console.log(`Player ${playerId} removed from room ${roomId}`);

    return room;
  }

  // Get player's current room
  getPlayerRoom(playerId) {
    return this.playerRooms.get(playerId);
  }

  // Set player ready state
  setPlayerReady(roomId, playerId, isReady) {
    const room = this.rooms.get(roomId);
    if (!room) return;

    const player = room.players.get(playerId);
    if (player) {
      player.isReady = isReady;
    }
  }

  // Set player disconnected state
  setPlayerDisconnected(roomId, playerId) {
    const room = this.rooms.get(roomId);
    if (!room) return;

    const player = room.players.get(playerId);
    if (player) {
      player.isDisconnected = true;
      player.disconnectedAt = Date.now();
    }
  }

  // Check if all players are ready
  areAllPlayersReady(roomId) {
    const room = this.rooms.get(roomId);
    if (!room) return false;

    const activePlayers = Array.from(room.players.values()).filter(
      p => !p.isDisconnected
    );

    return activePlayers.length > 0 && activePlayers.every(p => p.isReady);
  }

  // Update game state
  updateGameState(roomId, newState) {
    const room = this.rooms.get(roomId);
    if (!room) return;

    room.gameState = { ...room.gameState, ...newState };
  }

  // Set room status
  setRoomStatus(roomId, status) {
    const room = this.rooms.get(roomId);
    if (!room) return;

    room.status = status;
    if (status === 'playing' && !room.startTime) {
      room.startTime = Date.now();
    }
  }

  // Set room timer
  setRoomTimer(roomId, timer) {
    const room = this.rooms.get(roomId);
    if (!room) return;

    // Clear existing timer
    if (room.timer) {
      clearInterval(room.timer);
    }

    room.timer = timer;
  }

  // Check if room should be cleaned up
  shouldCleanupRoom(roomId) {
    const room = this.rooms.get(roomId);
    if (!room) return true;

    // Clean up if room is completed and has been inactive for 30 seconds
    if (room.status === 'completed') {
      const inactiveTime = Date.now() - (room.endTime || room.startTime);
      return inactiveTime > 30000; // 30 seconds
    }

    // Clean up if no active players
    const activePlayers = Array.from(room.players.values()).filter(
      p => !p.isDisconnected
    );

    return activePlayers.length === 0;
  }

  // Delete room
  deleteRoom(roomId) {
    const room = this.rooms.get(roomId);
    if (!room) return;

    // Clear timer if exists
    if (room.timer) {
      clearInterval(room.timer);
    }

    // Remove player room mappings
    room.players.forEach((player, playerId) => {
      this.playerRooms.delete(playerId);
    });

    this.rooms.delete(roomId);
    console.log(`Room deleted: ${roomId}`);
  }

  // Get all active rooms
  getActiveRooms() {
    return Array.from(this.rooms.values()).filter(
      room => room.status !== 'completed'
    );
  }

  // Get available rooms for joining
  getAvailableRooms(gameType = null) {
    const rooms = Array.from(this.rooms.values()).filter(room => {
      if (room.status !== 'waiting') return false;
      if (gameType && room.gameType !== gameType) return false;

      const activePlayers = Array.from(room.players.values()).filter(
        p => !p.isDisconnected
      );

      return activePlayers.length < room.maxPlayers;
    });

    return rooms.map(room => ({
      id: room.id,
      gameType: room.gameType,
      gameSettings: room.gameSettings,
      currentPlayers: Array.from(room.players.values()).filter(p => !p.isDisconnected).length,
      maxPlayers: room.maxPlayers,
      minPlayers: room.minPlayers,
      creatorName: room.creatorName,
      createdAt: room.createdAt
    }));
  }

  // Get room statistics
  getRoomStats() {
    const rooms = Array.from(this.rooms.values());

    return {
      totalRooms: rooms.length,
      activeRooms: rooms.filter(r => r.status === 'playing').length,
      waitingRooms: rooms.filter(r => r.status === 'waiting').length,
      completedRooms: rooms.filter(r => r.status === 'completed').length,
      totalPlayers: rooms.reduce((sum, room) => sum + room.players.size, 0),
      activePlayers: rooms.reduce((sum, room) => {
        return sum + Array.from(room.players.values()).filter(p => !p.isDisconnected).length;
      }, 0)
    };
  }

  // Generate unique room ID
  generateRoomId() {
    const timestamp = Date.now().toString(36);
    const random = Math.random().toString(36).substring(2, 8);
    return `${timestamp}-${random}`.toUpperCase();
  }

  // Clean up inactive rooms
  cleanupInactiveRooms() {
    const roomsToDelete = [];

    this.rooms.forEach((room, roomId) => {
      if (this.shouldCleanupRoom(roomId)) {
        roomsToDelete.push(roomId);
      }
    });

    roomsToDelete.forEach(roomId => {
      this.deleteRoom(roomId);
    });

    return roomsToDelete.length;
  }

  // Get room by player ID
  getRoomByPlayer(playerId) {
    const roomId = this.playerRooms.get(playerId);
    return roomId ? this.rooms.get(roomId) : null;
  }

  // Update player score in room
  updatePlayerScore(roomId, playerId, score) {
    const room = this.rooms.get(roomId);
    if (!room) return;

    const player = room.players.get(playerId);
    if (player) {
      player.score = score;
    }
  }

  // Get room leaderboard
  getRoomLeaderboard(roomId) {
    const room = this.rooms.get(roomId);
    if (!room) return [];

    return Array.from(room.players.values())
      .sort((a, b) => (b.score || 0) - (a.score || 0))
      .map((player, index) => ({
        rank: index + 1,
        playerId: player.playerId,
        username: player.username,
        score: player.score || 0,
        isDisconnected: player.isDisconnected
      }));
  }
}

// Export singleton instance
const roomManager = new RoomManager();

// Periodic cleanup of inactive rooms (every 5 minutes)
setInterval(() => {
  const cleanedCount = roomManager.cleanupInactiveRooms();
  if (cleanedCount > 0) {
    console.log(`Cleaned up ${cleanedCount} inactive rooms`);
  }
}, 5 * 60 * 1000);

module.exports = { roomManager, RoomManager };