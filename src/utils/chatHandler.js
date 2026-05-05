// Real-time chat system for game rooms

class ChatHandler {
  constructor() {
    this.messageHistory = new Map(); // Store message history per room
    this.typingUsers = new Map(); // Track typing users per room
    this.maxHistorySize = 100; // Max messages to keep per room
    this.typingTimeout = 3000; // Clear typing indicator after 3 seconds
  }

  // Handle sending a message
  handleSendMessage(io, socket, data) {
    try {
      const { roomId, message, messageType = 'text', targetId = null } = data;
      const playerId = socket.playerId;
      const username = socket.player.username;

      // Validate message
      if (!message || typeof message !== 'string' || message.trim().length === 0) {
        socket.emit('error', { message: 'Invalid message' });
        return;
      }

      if (message.length > 500) {
        socket.emit('error', { message: 'Message too long (max 500 characters)' });
        return;
      }

      // Create message object
      const messageObj = {
        id: this.generateMessageId(),
        roomId,
        playerId,
        username,
        message: message.trim(),
        messageType,
        targetId, // For private messages
        timestamp: Date.now(),
        isSystem: false
      };

      // Store message in history
      this.addToMessageHistory(roomId, messageObj);

      // Send message based on type
      if (messageType === 'private' && targetId) {
        // Private message
        this.sendPrivateMessage(io, socket, messageObj, targetId);
      } else {
        // Room message
        this.sendRoomMessage(io, roomId, messageObj);
      }

      console.log(`Message sent in room ${roomId} by ${username}: ${message.substring(0, 30)}...`);

    } catch (error) {
      console.error('Send message error:', error);
      socket.emit('error', { message: 'Failed to send message' });
    }
  }

  // Send message to room
  sendRoomMessage(io, roomId, messageObj) {
    io.to(roomId).emit('new_message', messageObj);
  }

  // Send private message
  sendPrivateMessage(io, socket, messageObj, targetId) {
    // Send to sender
    socket.emit('new_message', {
      ...messageObj,
      isPrivate: true
    });

    // Send to recipient if they're connected
    const targetSocket = this.findSocketByPlayerId(io, targetId);
    if (targetSocket) {
      targetSocket.emit('new_message', {
        ...messageObj,
        isPrivate: true
      });
    } else {
      // Recipient is offline, could implement offline message storage
      socket.emit('error', { message: 'Recipient is offline' });
    }
  }

  // Handle typing start
  handleTypingStart(io, socket, data) {
    try {
      const { roomId } = data;
      const playerId = socket.playerId;
      const username = socket.player.username;

      // Add to typing users
      if (!this.typingUsers.has(roomId)) {
        this.typingUsers.set(roomId, new Map());
      }

      const roomTypingUsers = this.typingUsers.get(roomId);
      roomTypingUsers.set(playerId, {
        username,
        startTime: Date.now()
      });

      // Notify room (excluding sender)
      socket.to(roomId).emit('user_typing', {
        playerId,
        username
      });

      // Clear typing indicator after timeout
      this.clearTypingIndicator(roomId, playerId);

    } catch (error) {
      console.error('Typing start error:', error);
    }
  }

  // Handle typing stop
  handleTypingStop(io, socket, data) {
    try {
      const { roomId } = data;
      const playerId = socket.playerId;

      // Remove from typing users
      const roomTypingUsers = this.typingUsers.get(roomId);
      if (roomTypingUsers) {
        roomTypingUsers.delete(playerId);

        // Notify room
        socket.to(roomId).emit('user_stopped_typing', {
          playerId
        });
      }

    } catch (error) {
      console.error('Typing stop error:', error);
    }
  }

  // Clear typing indicator after timeout
  clearTypingIndicator(roomId, playerId) {
    setTimeout(() => {
      const roomTypingUsers = this.typingUsers.get(roomId);
      if (roomTypingUsers && roomTypingUsers.has(playerId)) {
        roomTypingUsers.delete(playerId);

        // Notify room that user stopped typing
        // Note: This would need io reference, but we're in a timeout
        // In production, you'd want to handle this differently
      }
    }, this.typingTimeout);
  }

  // Add message to history
  addToMessageHistory(roomId, messageObj) {
    if (!this.messageHistory.has(roomId)) {
      this.messageHistory.set(roomId, []);
    }

    const history = this.messageHistory.get(roomId);
    history.push(messageObj);

    // Trim history if too large
    if (history.length > this.maxHistorySize) {
      history.shift();
    }
  }

  // Get message history for a room
  getMessageHistory(roomId, limit = 50) {
    const history = this.messageHistory.get(roomId) || [];
    return history.slice(-limit);
  }

  // Clear message history for a room
  clearMessageHistory(roomId) {
    this.messageHistory.delete(roomId);
    this.typingUsers.delete(roomId);
  }

  // Get typing users for a room
  getTypingUsers(roomId) {
    const roomTypingUsers = this.typingUsers.get(roomId);
    if (!roomTypingUsers) return [];

    return Array.from(roomTypingUsers.entries()).map(([playerId, data]) => ({
      playerId,
      username: data.username
    }));
  }

  // Send system message
  sendSystemMessage(io, roomId, message) {
    const messageObj = {
      id: this.generateMessageId(),
      roomId,
      playerId: 'system',
      username: 'System',
      message,
      messageType: 'system',
      timestamp: Date.now(),
      isSystem: true
    };

    this.addToMessageHistory(roomId, messageObj);
    io.to(roomId).emit('new_message', messageObj);
  }

  // Send game event message
  sendGameEventMessage(io, roomId, eventType, eventData) {
    const messageObj = {
      id: this.generateMessageId(),
      roomId,
      playerId: 'game',
      username: 'Game',
      message: this.formatGameEventMessage(eventType, eventData),
      messageType: 'game_event',
      eventType,
      eventData,
      timestamp: Date.now(),
      isSystem: true
    };

    this.addToMessageHistory(roomId, messageObj);
    io.to(roomId).emit('new_message', messageObj);
  }

  // Format game event message
  formatGameEventMessage(eventType, eventData) {
    switch (eventType) {
      case 'player_joined':
        return `${eventData.username} joined the game`;
      case 'player_left':
        return `${eventData.username} left the game`;
      case 'game_started':
        return 'Game started!';
      case 'game_ended':
        return `Game ended! Winner: ${eventData.winnerUsername}`;
      case 'player_scored':
        return `${eventData.username} scored ${eventData.points} points!`;
      case 'achievement_unlocked':
        return `${eventData.username} unlocked achievement: ${eventData.achievementName}`;
      default:
        return `Game event: ${eventType}`;
    }
  }

  // Find socket by player ID
  findSocketByPlayerId(io, playerId) {
    // This is a simplified version - in production you'd want to maintain
    // a mapping of player IDs to socket IDs
    for (const [socketId, socket] of io.sockets.sockets) {
      if (socket.playerId === playerId) {
        return socket;
      }
    }
    return null;
  }

  // Generate unique message ID
  generateMessageId() {
    const timestamp = Date.now().toString(36);
    const random = Math.random().toString(36).substring(2, 8);
    return `msg-${timestamp}-${random}`.toUpperCase();
  }

  // Get chat statistics
  getChatStats() {
    let totalMessages = 0;
    let totalRooms = 0;

    for (const [roomId, history] of this.messageHistory) {
      totalMessages += history.length;
      totalRooms++;
    }

    return {
      totalRooms,
      totalMessages,
      averageMessagesPerRoom: totalRooms > 0 ? Math.round(totalMessages / totalRooms) : 0
    };
  }

  // Mute player (admin function)
  mutePlayer(roomId, playerId, duration = 60000) {
    // This would need to be implemented with proper storage
    // For now, it's a placeholder
    console.log(`Player ${playerId} muted in room ${roomId} for ${duration}ms`);
  }

  // Unmute player
  unmutePlayer(roomId, playerId) {
    // Placeholder for unmute functionality
    console.log(`Player ${playerId} unmuted in room ${roomId}`);
  }

  // Filter inappropriate content
  filterContent(message) {
    // Basic content filtering - in production you'd want a more sophisticated system
    const inappropriateWords = ['badword1', 'badword2', 'badword3']; // Example

    let filteredMessage = message;
    inappropriateWords.forEach(word => {
      const regex = new RegExp(word, 'gi');
      filteredMessage = filteredMessage.replace(regex, '***');
    });

    return filteredMessage;
  }

  // Validate message content
  validateMessageContent(message) {
    // Check for empty messages
    if (!message || message.trim().length === 0) {
      return { valid: false, reason: 'Message cannot be empty' };
    }

    // Check for messages that are too long
    if (message.length > 500) {
      return { valid: false, reason: 'Message too long' };
    }

    // Check for spam (basic implementation)
    // In production, you'd want more sophisticated spam detection

    return { valid: true };
  }

  // Handle chat commands
  handleChatCommand(io, socket, roomId, command, args) {
    switch (command) {
      case '/help':
        this.sendSystemMessage(io, roomId, 'Available commands: /help, /clear, /stats');
        break;
      case '/clear':
        this.clearMessageHistory(roomId);
        this.sendSystemMessage(io, roomId, 'Chat history cleared');
        break;
      case '/stats':
        const stats = this.getChatStats();
        this.sendSystemMessage(io, roomId, `Chat stats: ${stats.totalMessages} messages in ${stats.totalRooms} rooms`);
        break;
      default:
        socket.emit('error', { message: 'Unknown command' });
    }
  }
}

// Export singleton instance
const chatHandler = new ChatHandler();

module.exports = { chatHandler, ChatHandler };