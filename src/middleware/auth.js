const jwt = require('jsonwebtoken');
const Player = require('../models/Player');

// Authentication middleware
const authenticate = async (req, res, next) => {
  try {
    const token = req.header('Authorization')?.replace('Bearer ', '');

    if (!token) {
      return res.status(401).json({ error: 'Authentication required' });
    }

    const decoded = jwt.verify(token, process.env.JWT_SECRET);
    const player = await Player.findById(decoded.id).select('-password');

    if (!player) {
      return res.status(401).json({ error: 'Player not found' });
    }

    req.player = player;
    next();
  } catch (error) {
    res.status(401).json({ error: 'Invalid token' });
  }
};

// Optional authentication (doesn't fail if no token)
const optionalAuth = async (req, res, next) => {
  try {
    const token = req.header('Authorization')?.replace('Bearer ', '');

    if (token) {
      const decoded = jwt.verify(token, process.env.JWT_SECRET);
      const player = await Player.findById(decoded.id).select('-password');
      if (player) {
        req.player = player;
      }
    }

    next();
  } catch (error) {
    next(); // Continue without authentication
  }
};

module.exports = { authenticate, optionalAuth };