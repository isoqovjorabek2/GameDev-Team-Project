const jwt = require('jsonwebtoken');
const Player = require('../models/Player');

// Socket authentication middleware
const socketAuth = async (socket, next) => {
  try {
    // Get token from handshake auth or headers
    const token = socket.handshake.auth.token ||
                  socket.handshake.headers.authorization?.replace('Bearer ', '');

    if (!token) {
      return next(new Error('Authentication required: No token provided'));
    }

    // Verify JWT token
    let decoded;
    try {
      decoded = jwt.verify(token, process.env.JWT_SECRET);
    } catch (jwtError) {
      if (jwtError.name === 'TokenExpiredError') {
        return next(new Error('Authentication failed: Token expired'));
      } else if (jwtError.name === 'JsonWebTokenError') {
        return next(new Error('Authentication failed: Invalid token'));
      } else {
        return next(new Error('Authentication failed: Token verification error'));
      }
    }

    // Check if token has required fields
    if (!decoded || !decoded.id) {
      return next(new Error('Authentication failed: Invalid token structure'));
    }

    // Find player in database
    const player = await Player.findById(decoded.id).select('-password');
    if (!player) {
      return next(new Error('Authentication failed: Player not found'));
    }

    // Check if player is active/banned (if you have such fields)
    if (player.isBanned) {
      return next(new Error('Authentication failed: Account is banned'));
    }

    // Attach player to socket
    socket.player = player;
    socket.playerId = player._id.toString();
    socket.token = token;

    // Log successful authentication
    console.log(`Socket authenticated: ${player.username} (${player._id})`);

    next();
  } catch (error) {
    console.error('Socket authentication error:', error);
    next(new Error('Authentication failed: Server error'));
  }
};

// Optional socket authentication (doesn't fail if no token)
const optionalSocketAuth = async (socket, next) => {
  try {
    const token = socket.handshake.auth.token ||
                  socket.handshake.headers.authorization?.replace('Bearer ', '');

    if (token) {
      try {
        const decoded = jwt.verify(token, process.env.JWT_SECRET);
        const player = await Player.findById(decoded.id).select('-password');

        if (player) {
          socket.player = player;
          socket.playerId = player._id.toString();
          socket.token = token;
          socket.isAuthenticated = true;
        }
      } catch (error) {
        // Token invalid but we continue without authentication
        socket.isAuthenticated = false;
      }
    } else {
      socket.isAuthenticated = false;
    }

    next();
  } catch (error) {
    // Continue without authentication on error
    socket.isAuthenticated = false;
    next();
  }
};

// Rate limiting for socket connections
const socketRateLimiter = (maxConnections = 5, windowMs = 60000) => {
  const connections = new Map();

  return (socket, next) => {
    const playerId = socket.playerId || socket.id;
    const now = Date.now();

    // Clean up old entries
    for (const [id, data] of connections.entries()) {
      if (now - data.timestamp > windowMs) {
        connections.delete(id);
      }
    }

    // Check connection count
    const playerConnections = connections.get(playerId);
    if (playerConnections && playerConnections.count >= maxConnections) {
      return next(new Error('Rate limit exceeded: Too many connections'));
    }

    // Record connection
    if (playerConnections) {
      playerConnections.count++;
      playerConnections.timestamp = now;
    } else {
      connections.set(playerId, { count: 1, timestamp: now });
    }

    // Clean up on disconnect
    socket.on('disconnect', () => {
      const data = connections.get(playerId);
      if (data) {
        data.count--;
        if (data.count <= 0) {
          connections.delete(playerId);
        }
      }
    });

    next();
  };
};

// Validate socket handshake data
const validateHandshake = (socket, next) => {
  try {
    const { gameVersion, clientVersion } = socket.handshake.query;

    // Validate game version if provided
    if (gameVersion) {
      const minVersion = process.env.MIN_GAME_VERSION || '1.0.0';
      if (this.compareVersions(gameVersion, minVersion) < 0) {
        return next(new Error(`Game version ${gameVersion} is not supported. Minimum required: ${minVersion}`));
      }
    }

    // Store client info
    socket.clientInfo = {
      gameVersion: gameVersion || 'unknown',
      clientVersion: clientVersion || 'unknown',
      userAgent: socket.handshake.headers['user-agent'] || 'unknown',
      ip: socket.handshake.address || 'unknown'
    };

    next();
  } catch (error) {
    console.error('Handshake validation error:', error);
    next(new Error('Handshake validation failed'));
  }
};

// Version comparison helper
const compareVersions = (version1, version2) => {
  const v1 = version1.split('.').map(Number);
  const v2 = version2.split('.').map(Number);

  for (let i = 0; i < Math.max(v1.length, v2.length); i++) {
    const num1 = v1[i] || 0;
    const num2 = v2[i] || 0;

    if (num1 > num2) return 1;
    if (num1 < num2) return -1;
  }

  return 0;
};

// Room access control middleware
const roomAccessControl = (socket, next) => {
  socket.canJoinRoom = (roomId) => {
    // Implement room access control logic
    // For example, check if player is banned from room, room is full, etc.
    return true;
  };

  socket.canSendMessage = (roomId) => {
    // Implement message sending control
    // For example, check if player is muted, room allows messages, etc.
    return true;
  };

  next();
};

// Socket error handling middleware
const socketErrorHandler = (socket, next) => {
  socket.on('error', (error) => {
    console.error(`Socket error for ${socket.player?.username || 'unknown'}:`, error.message);

    // Send error to client
    socket.emit('error', {
      message: error.message || 'An error occurred',
      code: error.code || 'SOCKET_ERROR'
    });
  });

  next();
};

// Socket logging middleware
const socketLogger = (socket, next) => {
  const originalEmit = socket.emit;

  // Log outgoing messages
  socket.emit = function(event, ...args) {
    if (process.env.NODE_ENV === 'development') {
      console.log(`[OUT] ${socket.player?.username || 'unknown'} -> ${event}:`, JSON.stringify(args).substring(0, 100));
    }
    return originalEmit.apply(this, [event, ...args]);
  };

  // Log incoming messages
  socket.onAny((event, ...args) => {
    if (process.env.NODE_ENV === 'development') {
      console.log(`[IN] ${socket.player?.username || 'unknown'} <- ${event}:`, JSON.stringify(args).substring(0, 100));
    }
  });

  next();
};

// Combine multiple middleware
const combineSocketMiddleware = (...middlewares) => {
  return (socket, next) => {
    let index = 0;

    const runNext = (error) => {
      if (error) {
        return next(error);
      }

      if (index >= middlewares.length) {
        return next();
      }

      const middleware = middlewares[index++];
      middleware(socket, runNext);
    };

    runNext();
  };
};

// Export middleware functions
module.exports = {
  socketAuth,
  optionalSocketAuth,
  socketRateLimiter,
  validateHandshake,
  roomAccessControl,
  socketErrorHandler,
  socketLogger,
  combineSocketMiddleware
};