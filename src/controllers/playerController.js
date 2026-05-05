const Player = require('../models/Player');

// Get player by ID
const getPlayerById = async (req, res) => {
  try {
    const player = await Player.findById(req.params.id)
      .select('-password -email -deviceId');

    if (!player) {
      return res.status(404).json({ error: 'Player not found' });
    }

    res.json({ player });
  } catch (error) {
    console.error('Get player error:', error);
    res.status(500).json({ error: 'Failed to get player' });
  }
};

// Update player profile
const updateProfile = async (req, res) => {
  try {
    const { username, avatar, settings } = req.body;

    const updateData = {};
    if (username) updateData.username = username;
    if (avatar) updateData['profile.avatar'] = avatar;
    if (settings) updateData.settings = settings;

    const player = await Player.findByIdAndUpdate(
      req.player._id,
      { $set: updateData },
      { new: true, runValidators: true }
    ).select('-password -email -deviceId');

    res.json({
      message: 'Profile updated successfully',
      player
    });
  } catch (error) {
    console.error('Update profile error:', error);
    res.status(500).json({ error: 'Failed to update profile' });
  }
};

// Update player stats
const updateStats = async (req, res) => {
  try {
    const { gamesPlayed, gamesWon, totalScore, highScore } = req.body;

    const player = await Player.findById(req.player._id);

    if (gamesPlayed) player.stats.gamesPlayed += gamesPlayed;
    if (gamesWon) player.stats.gamesWon += gamesWon;
    if (totalScore) player.stats.totalScore += totalScore;
    if (highScore && highScore > player.stats.highScore) {
      player.stats.highScore = highScore;
    }

    await player.save();

    res.json({
      message: 'Stats updated successfully',
      stats: player.stats
    });
  } catch (error) {
    console.error('Update stats error:', error);
    res.status(500).json({ error: 'Failed to update stats' });
  }
};

// Add item to inventory
const addToInventory = async (req, res) => {
  try {
    const { itemId, name, quantity } = req.body;

    const player = await Player.findById(req.player._id);

    // Check if item already exists in inventory
    const existingItem = player.inventory.find(item => item.itemId === itemId);

    if (existingItem) {
      existingItem.quantity += quantity;
    } else {
      player.inventory.push({ itemId, name, quantity });
    }

    await player.save();

    res.json({
      message: 'Item added to inventory',
      inventory: player.inventory
    });
  } catch (error) {
    console.error('Add to inventory error:', error);
    res.status(500).json({ error: 'Failed to add item to inventory' });
  }
};

// Get leaderboard
const getLeaderboard = async (req, res) => {
  try {
    const { limit = 10, sortBy = 'totalScore' } = req.query;

    const validSortFields = ['totalScore', 'highScore', 'gamesWon', 'experience'];
    const sortField = validSortFields.includes(sortBy) ? sortBy : 'totalScore';

    const leaderboard = await Player.find()
      .select('username profile stats')
      .sort({ [`stats.${sortField}`]: -1 })
      .limit(parseInt(limit));

    res.json({ leaderboard, sortBy: sortField });
  } catch (error) {
    console.error('Get leaderboard error:', error);
    res.status(500).json({ error: 'Failed to get leaderboard' });
  }
};

module.exports = {
  getPlayerById,
  updateProfile,
  updateStats,
  addToInventory,
  getLeaderboard
};