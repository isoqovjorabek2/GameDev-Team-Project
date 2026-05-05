const express = require('express');
const router = express.Router();
const { authenticate } = require('../middleware/auth');
const {
  register,
  login,
  getCurrentPlayer,
  registerValidation,
  loginValidation
} = require('../controllers/authController');

// Register new player
router.post('/register', registerValidation, register);

// Login player
router.post('/login', loginValidation, login);

// Get current player info (protected route)
router.get('/me', authenticate, getCurrentPlayer);

module.exports = router;