const mongoose = require('mongoose');

const gameSessionSchema = new mongoose.Schema({
  gameId: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Game',
    required: true
  },
  players: [{
    playerId: {
      type: mongoose.Schema.Types.ObjectId,
      ref: 'Player',
      required: true
    },
    username: String,
    score: { type: Number, default: 0 },
    isReady: { type: Boolean, default: false },
    joinedAt: { type: Date, default: Date.now }
  }],
  status: {
    type: String,
    enum: ['waiting', 'in-progress', 'completed', 'abandoned'],
    default: 'waiting'
  },
  gameState: {
    type: mongoose.Schema.Types.Mixed,
    default: {}
  },
  settings: {
    roundTime: Number,
    turnTime: Number,
    scoreLimit: Number,
    customSettings: mongoose.Schema.Types.Mixed
  },
  currentRound: { type: Number, default: 1 },
  maxRounds: { type: Number, default: 1 },
  currentTurn: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Player'
  },
  winner: {
    type: mongoose.Schema.Types.ObjectId,
    ref: 'Player'
  },
  startTime: Date,
  endTime: Date,
  createdAt: { type: Date, default: Date.now },
  updatedAt: { type: Date, default: Date.now }
});

// Update timestamp on save
gameSessionSchema.pre('save', function(next) {
  this.updatedAt = Date.now();
  next();
});

// Index for finding active sessions
gameSessionSchema.index({ status: 1, gameId: 1 });
gameSessionSchema.index({ 'players.playerId': 1 });

module.exports = mongoose.model('GameSession', gameSessionSchema);