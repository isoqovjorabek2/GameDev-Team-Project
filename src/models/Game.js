const mongoose = require('mongoose');

const gameSchema = new mongoose.Schema({
  name: {
    type: String,
    required: true,
    unique: true
  },
  type: {
    type: String,
    enum: ['real-time', 'turn-based', 'single-player'],
    required: true
  },
  description: String,
  maxPlayers: {
    type: Number,
    default: 2
  },
  minPlayers: {
    type: Number,
    default: 1
  },
  isActive: {
    type: Boolean,
    default: true
  },
  settings: {
    roundTime: Number, // in seconds for real-time games
    turnTime: Number, // in seconds for turn-based games
    scoreLimit: Number,
    customSettings: mongoose.Schema.Types.Mixed
  },
  createdAt: { type: Date, default: Date.now },
  updatedAt: { type: Date, default: Date.now }
});

// Update timestamp on save
gameSchema.pre('save', function(next) {
  this.updatedAt = Date.now();
  next();
});

module.exports = mongoose.model('Game', gameSchema);