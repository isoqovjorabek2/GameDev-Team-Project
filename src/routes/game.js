const express = require('express');
const router = express.Router();
const { authenticate, optionalAuth } = require('../middleware/auth');
const {
  createGame,
  getAllGames,
  getGameById,
  createGameSession,
  joinGameSession,
  getAvailableSessions,
  updateSessionState
} = require('../controllers/gameController');

// Game management routes (admin only in production)
router.post('/', authenticate, createGame);
router.get('/', optionalAuth, getAllGames);
router.get('/:id', optionalAuth, getGameById);

// Game session routes
router.post('/sessions', authenticate, createGameSession);
router.post('/sessions/join', authenticate, joinGameSession);
router.get('/sessions/available', optionalAuth, getAvailableSessions);
router.put('/sessions/:sessionId/state', authenticate, updateSessionState);

module.exports = router;