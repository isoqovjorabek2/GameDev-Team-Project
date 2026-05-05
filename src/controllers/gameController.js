const Game = require('../models/Game');
const GameSession = require('../models/GameSession');

// Create new game
const createGame = async (req, res) => {
  try {
    const { name, type, description, maxPlayers, minPlayers, settings } = req.body;

    const game = new Game({
      name,
      type,
      description,
      maxPlayers: maxPlayers || 2,
      minPlayers: minPlayers || 1,
      settings: settings || {}
    });

    await game.save();

    res.status(201).json({
      message: 'Game created successfully',
      game
    });
  } catch (error) {
    console.error('Create game error:', error);
    res.status(500).json({ error: 'Failed to create game' });
  }
};

// Get all games
const getAllGames = async (req, res) => {
  try {
    const games = await Game.find({ isActive: true });

    res.json({ games });
  } catch (error) {
    console.error('Get games error:', error);
    res.status(500).json({ error: 'Failed to get games' });
  }
};

// Get game by ID
const getGameById = async (req, res) => {
  try {
    const game = await Game.findById(req.params.id);

    if (!game) {
      return res.status(404).json({ error: 'Game not found' });
    }

    res.json({ game });
  } catch (error) {
    console.error('Get game error:', error);
    res.status(500).json({ error: 'Failed to get game' });
  }
};

// Create game session
const createGameSession = async (req, res) => {
  try {
    const { gameId, settings } = req.body;

    const game = await Game.findById(gameId);
    if (!game) {
      return res.status(404).json({ error: 'Game not found' });
    }

    const session = new GameSession({
      gameId,
      players: [{
        playerId: req.player._id,
        username: req.player.username,
        isReady: true
      }],
      settings: settings || game.settings,
      status: 'waiting'
    });

    await session.save();

    res.status(201).json({
      message: 'Game session created',
      session
    });
  } catch (error) {
    console.error('Create session error:', error);
    res.status(500).json({ error: 'Failed to create game session' });
  }
};

// Join game session
const joinGameSession = async (req, res) => {
  try {
    const { sessionId } = req.body;

    const session = await GameSession.findById(sessionId);
    if (!session) {
      return res.status(404).json({ error: 'Game session not found' });
    }

    if (session.status !== 'waiting') {
      return res.status(400).json({ error: 'Cannot join this session' });
    }

    // Check if player is already in the session
    const alreadyJoined = session.players.find(
      p => p.playerId.toString() === req.player._id.toString()
    );

    if (alreadyJoined) {
      return res.status(400).json({ error: 'Already in this session' });
    }

    // Check if session is full
    const game = await Game.findById(session.gameId);
    if (session.players.length >= game.maxPlayers) {
      return res.status(400).json({ error: 'Session is full' });
    }

    // Add player to session
    session.players.push({
      playerId: req.player._id,
      username: req.player.username,
      isReady: true
    });

    await session.save();

    res.json({
      message: 'Joined game session successfully',
      session
    });
  } catch (error) {
    console.error('Join session error:', error);
    res.status(500).json({ error: 'Failed to join game session' });
  }
};

// Get available game sessions
const getAvailableSessions = async (req, res) => {
  try {
    const { gameId } = req.query;

    const query = { status: 'waiting' };
    if (gameId) {
      query.gameId = gameId;
    }

    const sessions = await GameSession.find(query)
      .populate('gameId')
      .populate('players.playerId', 'username profile');

    res.json({ sessions });
  } catch (error) {
    console.error('Get sessions error:', error);
    res.status(500).json({ error: 'Failed to get game sessions' });
  }
};

// Update game session state
const updateSessionState = async (req, res) => {
  try {
    const { sessionId, gameState, status, currentTurn } = req.body;

    const session = await GameSession.findById(sessionId);
    if (!session) {
      return res.status(404).json({ error: 'Game session not found' });
    }

    // Verify player is in the session
    const playerInSession = session.players.find(
      p => p.playerId.toString() === req.player._id.toString()
    );

    if (!playerInSession) {
      return res.status(403).json({ error: 'Not in this session' });
    }

    if (gameState) session.gameState = gameState;
    if (status) session.status = status;
    if (currentTurn) session.currentTurn = currentTurn;

    if (status === 'in-progress' && !session.startTime) {
      session.startTime = Date.now();
    }

    if (status === 'completed') {
      session.endTime = Date.now();
    }

    await session.save();

    res.json({
      message: 'Session updated successfully',
      session
    });
  } catch (error) {
    console.error('Update session error:', error);
    res.status(500).json({ error: 'Failed to update game session' });
  }
};

module.exports = {
  createGame,
  getAllGames,
  getGameById,
  createGameSession,
  joinGameSession,
  getAvailableSessions,
  updateSessionState
};