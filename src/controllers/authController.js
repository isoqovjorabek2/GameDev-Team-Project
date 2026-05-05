const jwt = require('jsonwebtoken');
const { body, validationResult } = require('express-validator');
const Player = require('../models/Player');

// Generate JWT token
const generateToken = (playerId) => {
  return jwt.sign({ id: playerId }, process.env.JWT_SECRET, {
    expiresIn: '7d' // Token expires in 7 days
  });
};

// Register new player
const register = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ errors: errors.array() });
    }

    const { username, email, password, deviceId } = req.body;

    // Check if player already exists
    const existingPlayer = await Player.findOne({
      $or: [{ email }, { username }]
    });

    if (existingPlayer) {
      return res.status(400).json({ error: 'Player already exists' });
    }

    // Create new player
    const player = new Player({
      username,
      email,
      password,
      deviceId: deviceId || null
    });

    await player.save();

    // Generate token
    const token = generateToken(player._id);

    res.status(201).json({
      message: 'Player registered successfully',
      token,
      player: {
        id: player._id,
        username: player.username,
        email: player.email,
        profile: player.profile
      }
    });
  } catch (error) {
    console.error('Registration error:', error);
    res.status(500).json({ error: 'Registration failed' });
  }
};

// Login player
const login = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ errors: errors.array() });
    }

    const { email, password, deviceId } = req.body;

    // Find player by email or device ID
    let player;
    if (deviceId) {
      player = await Player.findOne({ deviceId });
    } else {
      player = await Player.findOne({ email });
    }

    if (!player) {
      return res.status(401).json({ error: 'Invalid credentials' });
    }

    // Check password (if not using device ID)
    if (password && !deviceId) {
      const isMatch = await player.comparePassword(password);
      if (!isMatch) {
        return res.status(401).json({ error: 'Invalid credentials' });
      }
    }

    // Update last login
    player.lastLogin = Date.now();
    await player.save();

    // Generate token
    const token = generateToken(player._id);

    res.json({
      message: 'Login successful',
      token,
      player: {
        id: player._id,
        username: player.username,
        email: player.email,
        profile: player.profile,
        stats: player.stats
      }
    });
  } catch (error) {
    console.error('Login error:', error);
    res.status(500).json({ error: 'Login failed' });
  }
};

// Get current player info
const getCurrentPlayer = async (req, res) => {
  try {
    res.json({
      player: {
        id: req.player._id,
        username: req.player.username,
        email: req.player.email,
        profile: req.player.profile,
        stats: req.player.stats,
        inventory: req.player.inventory,
        achievements: req.player.achievements,
        settings: req.player.settings
      }
    });
  } catch (error) {
    console.error('Get player error:', error);
    res.status(500).json({ error: 'Failed to get player info' });
  }
};

// Validation rules
const registerValidation = [
  body('username').trim().isLength({ min: 3, max: 20 }),
  body('email').isEmail().normalizeEmail(),
  body('password').isLength({ min: 6 })
];

const loginValidation = [
  body('email').optional().isEmail().normalizeEmail(),
  body('password').optional().isLength({ min: 6 }),
  body('deviceId').optional()
];

module.exports = {
  register,
  login,
  getCurrentPlayer,
  registerValidation,
  loginValidation
};