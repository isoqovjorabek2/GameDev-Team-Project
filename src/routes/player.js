const express = require('express');
const router = express.Router();
const { authenticate, optionalAuth } = require('../middleware/auth');
const {
  getPlayerById,
  updateProfile,
  updateStats,
  addToInventory,
  getLeaderboard
} = require('../controllers/playerController');

// Get player by ID (public route)
router.get('/:id', optionalAuth, getPlayerById);

// Update player profile (protected route)
router.put('/profile', authenticate, updateProfile);

// Update player stats (protected route)
router.put('/stats', authenticate, updateStats);

// Add item to inventory (protected route)
router.post('/inventory', authenticate, addToInventory);

// Get leaderboard (public route)
router.get('/leaderboard/all', optionalAuth, getLeaderboard);

module.exports = router;