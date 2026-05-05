// Game state management and synchronization utilities

class GameStateManager {
  constructor() {
    this.gameStateHistory = new Map(); // Store state history for conflict resolution
  }

  // Initialize game state for a new game
  initializeGameState(roomInfo) {
    const baseState = {
      status: 'playing',
      currentTurn: this.determineFirstPlayer(roomInfo),
      turnOrder: this.determineTurnOrder(roomInfo),
      scores: this.initializeScores(roomInfo),
      board: this.initializeBoard(roomInfo),
      timeLeft: roomInfo.gameSettings.roundTime || 0,
      round: 1,
      maxRounds: roomInfo.gameSettings.maxRounds || 1,
      moves: [],
      lastMoveTime: Date.now(),
      customState: {}
    };

    // Add game-specific initialization
    switch (roomInfo.gameType) {
      case 'real-time':
        return this.initializeRealTimeState(baseState, roomInfo);
      case 'turn-based':
        return this.initializeTurnBasedState(baseState, roomInfo);
      default:
        return baseState;
    }
  }

  // Determine first player
  determineFirstPlayer(roomInfo) {
    const players = Array.from(roomInfo.players.values());
    // Random selection for fairness
    const randomIndex = Math.floor(Math.random() * players.length);
    return players[randomIndex].playerId;
  }

  // Determine turn order
  determineTurnOrder(roomInfo) {
    const players = Array.from(roomInfo.players.values());
    // Shuffle for random turn order
    const shuffled = [...players].sort(() => Math.random() - 0.5);
    return shuffled.map(p => p.playerId);
  }

  // Initialize scores
  initializeScores(roomInfo) {
    const scores = {};
    roomInfo.players.forEach((player, playerId) => {
      scores[playerId] = 0;
    });
    return scores;
  }

  // Initialize game board
  initializeBoard(roomInfo) {
    // Default empty board - can be customized per game type
    return {
      cells: [],
      dimensions: { width: 10, height: 10 },
      pieces: []
    };
  }

  // Initialize real-time game state
  initializeRealTimeState(baseState, roomInfo) {
    return {
      ...baseState,
      gameMode: 'real-time',
      startTime: Date.now(),
      endTime: roomInfo.gameSettings.roundTime ? Date.now() + (roomInfo.gameSettings.roundTime * 1000) : null,
      playerStates: this.initializePlayerStates(roomInfo),
      objects: [],
      projectiles: []
    };
  }

  // Initialize turn-based game state
  initializeTurnBasedState(baseState, roomInfo) {
    return {
      ...baseState,
      gameMode: 'turn-based',
      turnTimeLimit: roomInfo.gameSettings.turnTime || 30,
      currentTurnStartTime: Date.now(),
      playerStates: this.initializePlayerStates(roomInfo),
      availableMoves: []
    };
  }

  // Initialize player states
  initializePlayerStates(roomInfo) {
    const playerStates = {};
    roomInfo.players.forEach((player, playerId) => {
      playerStates[playerId] = {
        position: { x: 0, y: 0 },
        health: 100,
        energy: 100,
        inventory: [],
        status: 'active'
      };
    });
    return playerStates;
  }

  // Validate a move
  validateMove(roomInfo, playerId, move, moveData) {
    const gameState = roomInfo.gameState;

    // Check if game is active
    if (gameState.status !== 'playing') {
      return { valid: false, reason: 'Game is not active' };
    }

    // Check if player is in the game
    if (!gameState.scores.hasOwnProperty(playerId)) {
      return { valid: false, reason: 'Player not in game' };
    }

    // Check if player is disconnected
    const player = roomInfo.players.get(playerId);
    if (player && player.isDisconnected) {
      return { valid: false, reason: 'Player is disconnected' };
    }

    // Turn-based validation
    if (roomInfo.gameType === 'turn-based') {
      if (gameState.currentTurn !== playerId) {
        return { valid: false, reason: 'Not your turn' };
      }

      // Check turn time limit
      const turnTimeElapsed = Date.now() - gameState.currentTurnStartTime;
      if (turnTimeElapsed > gameState.turnTimeLimit * 1000) {
        return { valid: false, reason: 'Turn time exceeded' };
      }
    }

    // Game-specific move validation
    return this.validateGameSpecificMove(roomInfo, playerId, move, moveData);
  }

  // Validate game-specific moves
  validateGameSpecificMove(roomInfo, playerId, move, moveData) {
    // This is where game-specific validation logic would go
    // For now, we'll provide basic validation

    switch (move) {
      case 'move':
        return this.validateMovement(roomInfo, playerId, moveData);
      case 'attack':
        return this.validateAttack(roomInfo, playerId, moveData);
      case 'collect':
        return this.validateCollection(roomInfo, playerId, moveData);
      case 'use_item':
        return this.validateItemUse(roomInfo, playerId, moveData);
      default:
        // Allow unknown moves for flexibility
        return { valid: true };
    }
  }

  // Validate movement
  validateMovement(roomInfo, playerId, moveData) {
    const { position, direction } = moveData;
    const gameState = roomInfo.gameState;
    const playerState = gameState.playerStates[playerId];

    if (!playerState) {
      return { valid: false, reason: 'Player state not found' };
    }

    // Check if player has enough energy
    if (playerState.energy < 10) {
      return { valid: false, reason: 'Not enough energy' };
    }

    // Validate position bounds
    const { width, height } = gameState.board.dimensions;
    if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height) {
      return { valid: false, reason: 'Position out of bounds' };
    }

    return { valid: true };
  }

  // Validate attack
  validateAttack(roomInfo, playerId, moveData) {
    const { targetId, attackType } = moveData;
    const gameState = roomInfo.gameState;
    const playerState = gameState.playerStates[playerId];

    if (!playerState) {
      return { valid: false, reason: 'Player state not found' };
    }

    // Check if target exists
    if (!gameState.playerStates[targetId]) {
      return { valid: false, reason: 'Target not found' };
    }

    // Check if target is in range (simplified)
    const attackerPos = playerState.position;
    const targetPos = gameState.playerStates[targetId].position;
    const distance = Math.sqrt(
      Math.pow(targetPos.x - attackerPos.x, 2) +
      Math.pow(targetPos.y - attackerPos.y, 2)
    );

    if (distance > 3) { // Max attack range of 3
      return { valid: false, reason: 'Target out of range' };
    }

    return { valid: true };
  }

  // Validate collection
  validateCollection(roomInfo, playerId, moveData) {
    const { itemId } = moveData;
    const gameState = roomInfo.gameState;

    // Check if item exists on board
    const itemExists = gameState.board.items?.some(item => item.id === itemId);
    if (!itemExists) {
      return { valid: false, reason: 'Item not found' };
    }

    return { valid: true };
  }

  // Validate item use
  validateItemUse(roomInfo, playerId, moveData) {
    const { itemId } = moveData;
    const playerState = roomInfo.gameState.playerStates[playerId];

    if (!playerState) {
      return { valid: false, reason: 'Player state not found' };
    }

    // Check if player has the item
    const hasItem = playerState.inventory?.some(item => item.id === itemId);
    if (!hasItem) {
      return { valid: false, reason: 'Item not in inventory' };
    }

    return { valid: true };
  }

  // Apply a move to the game state
  applyMove(roomInfo, playerId, move, moveData) {
    const gameState = { ...roomInfo.gameState };
    const timestamp = Date.now();

    // Record move
    gameState.moves.push({
      playerId,
      move,
      moveData,
      timestamp
    });

    // Apply game-specific move effects
    switch (move) {
      case 'move':
        this.applyMovement(gameState, playerId, moveData);
        break;
      case 'attack':
        this.applyAttack(gameState, playerId, moveData);
        break;
      case 'collect':
        this.applyCollection(gameState, playerId, moveData);
        break;
      case 'use_item':
        this.applyItemUse(gameState, playerId, moveData);
        break;
      default:
        // Apply custom move logic
        this.applyCustomMove(gameState, playerId, move, moveData);
    }

    // Update last move time
    gameState.lastMoveTime = timestamp;

    // Handle turn progression for turn-based games
    if (roomInfo.gameType === 'turn-based') {
      gameState = this.progressTurn(gameState, roomInfo);
    }

    return gameState;
  }

  // Apply movement
  applyMovement(gameState, playerId, moveData) {
    const { position } = moveData;
    const playerState = gameState.playerStates[playerId];

    if (playerState) {
      playerState.position = { ...position };
      playerState.energy = Math.max(0, playerState.energy - 10); // Movement costs 10 energy
    }
  }

  // Apply attack
  applyAttack(gameState, playerId, moveData) {
    const { targetId, damage = 20 } = moveData;
    const attackerState = gameState.playerStates[playerId];
    const targetState = gameState.playerStates[targetId];

    if (attackerState && targetState) {
      targetState.health = Math.max(0, targetState.health - damage);
      attackerState.energy = Math.max(0, attackerState.energy - 15); // Attack costs 15 energy

      // Check if target is defeated
      if (targetState.health <= 0) {
        targetState.status = 'defeated';
        gameState.scores[playerId] = (gameState.scores[playerId] || 0) + 100; // Bonus for defeat
      }
    }
  }

  // Apply collection
  applyCollection(gameState, playerId, moveData) {
    const { itemId, value = 10 } = moveData;
    const playerState = gameState.playerStates[playerId];

    if (playerState) {
      // Add to inventory
      playerState.inventory = playerState.inventory || [];
      playerState.inventory.push({ id: itemId, collectedAt: Date.now() });

      // Update score
      gameState.scores[playerId] = (gameState.scores[playerId] || 0) + value;

      // Remove item from board
      gameState.board.items = gameState.board.items?.filter(item => item.id !== itemId) || [];
    }
  }

  // Apply item use
  applyItemUse(gameState, playerId, moveData) {
    const { itemId, effect } = moveData;
    const playerState = gameState.playerStates[playerId];

    if (playerState) {
      // Remove item from inventory
      playerState.inventory = playerState.inventory?.filter(item => item.id !== itemId) || [];

      // Apply effect
      switch (effect) {
        case 'heal':
          playerState.health = Math.min(100, playerState.health + 30);
          break;
        case 'energy':
          playerState.energy = Math.min(100, playerState.energy + 50);
          break;
        case 'speed':
          // Temporary speed boost could be implemented
          break;
        default:
          // Custom effect
          if (effect && typeof effect === 'object') {
            Object.assign(playerState, effect);
          }
      }
    }
  }

  // Apply custom move
  applyCustomMove(gameState, playerId, move, moveData) {
    // Store custom moves for game-specific processing
    gameState.customState = gameState.customState || {};
    gameState.customState[move] = gameState.customState[move] || [];
    gameState.customState[move].push({
      playerId,
      moveData,
      timestamp: Date.now()
    });
  }

  // Progress to next turn (turn-based games)
  progressTurn(gameState, roomInfo) {
    const currentTurnIndex = gameState.turnOrder.indexOf(gameState.currentTurn);
    const nextTurnIndex = (currentTurnIndex + 1) % gameState.turnOrder.length;
    const nextPlayerId = gameState.turnOrder[nextTurnIndex];

    // Check if round is complete
    if (nextTurnIndex === 0) {
      gameState.round += 1;

      // Check if max rounds reached
      if (gameState.round > gameState.maxRounds) {
        gameState.status = 'completed';
        return gameState;
      }
    }

    // Update turn
    gameState.currentTurn = nextPlayerId;
    gameState.currentTurnStartTime = Date.now();

    // Regenerate energy for all players
    Object.keys(gameState.playerStates).forEach(playerId => {
      gameState.playerStates[playerId].energy = Math.min(
        100,
        gameState.playerStates[playerId].energy + 20
      );
    });

    return gameState;
  }

  // Check win condition
  checkWinCondition(roomInfo) {
    const gameState = roomInfo.gameState;

    // Check if game is already completed
    if (gameState.status === 'completed') {
      return {
        winner: gameState.winner,
        reason: 'Game already completed'
      };
    }

    // Game-specific win conditions
    switch (roomInfo.gameType) {
      case 'real-time':
        return this.checkRealTimeWinCondition(roomInfo);
      case 'turn-based':
        return this.checkTurnBasedWinCondition(roomInfo);
      default:
        return this.checkDefaultWinCondition(roomInfo);
    }
  }

  // Check real-time win condition
  checkRealTimeWinCondition(roomInfo) {
    const gameState = roomInfo.gameState;

    // Check time limit
    if (gameState.endTime && Date.now() >= gameState.endTime) {
      const winner = this.determineWinnerByScore(roomInfo);
      return {
        winner,
        reason: 'Time limit reached'
      };
    }

    // Check if only one player remains active
    const activePlayers = Object.entries(gameState.playerStates).filter(
      ([id, state]) => state.status === 'active'
    );

    if (activePlayers.length === 1) {
      return {
        winner: activePlayers[0][0],
        reason: 'Last player standing'
      };
    }

    return { winner: null, reason: null };
  }

  // Check turn-based win condition
  checkTurnBasedWinCondition(roomInfo) {
    const gameState = roomInfo.gameState;

    // Check if max rounds reached
    if (gameState.round > gameState.maxRounds) {
      const winner = this.determineWinnerByScore(roomInfo);
      return {
        winner,
        reason: 'Max rounds reached'
      };
    }

    // Check if only one player remains active
    const activePlayers = Object.entries(gameState.playerStates).filter(
      ([id, state]) => state.status === 'active'
    );

    if (activePlayers.length === 1) {
      return {
        winner: activePlayers[0][0],
        reason: 'Last player standing'
      };
    }

    return { winner: null, reason: null };
  }

  // Check default win condition
  checkDefaultWinCondition(roomInfo) {
    const gameState = roomInfo.gameState;

    // Check score limit
    const scoreLimit = roomInfo.gameSettings.scoreLimit;
    if (scoreLimit) {
      for (const [playerId, score] of Object.entries(gameState.scores)) {
        if (score >= scoreLimit) {
          return {
            winner: playerId,
            reason: 'Score limit reached'
          };
        }
      }
    }

    return { winner: null, reason: null };
  }

  // Determine winner by score
  determineWinnerByScore(roomInfo) {
    const gameState = roomInfo.gameState;
    let highestScore = -1;
    let winner = null;

    for (const [playerId, score] of Object.entries(gameState.scores)) {
      if (score > highestScore) {
        highestScore = score;
        winner = playerId;
      }
    }

    // Handle ties
    const tiedPlayers = Object.entries(gameState.scores)
      .filter(([id, score]) => score === highestScore)
      .map(([id]) => id);

    if (tiedPlayers.length > 1) {
      // For ties, you could implement tiebreaker logic
      // For now, return the first one
      return tiedPlayers[0];
    }

    return winner;
  }

  // Create delta update for efficient bandwidth usage
  createDeltaUpdate(oldState, newState) {
    const delta = {};

    // Compare and find differences
    for (const key in newState) {
      if (JSON.stringify(oldState[key]) !== JSON.stringify(newState[key])) {
        delta[key] = newState[key];
      }
    }

    return delta;
  }

  // Apply delta update to existing state
  applyDeltaUpdate(baseState, delta) {
    return {
      ...baseState,
      ...delta
    };
  }

  // Compress state for transmission
  compressState(gameState) {
    // Remove unnecessary data for transmission
    const { moves, customState, ...compressedState } = gameState;

    return {
      ...compressedState,
      moveCount: moves.length,
      lastMove: moves[moves.length - 1] || null
    };
  }

  // Get state history for conflict resolution
  getStateHistory(roomId) {
    return this.gameStateHistory.get(roomId) || [];
  }

  // Save state to history
  saveStateToHistory(roomId, gameState) {
    if (!this.gameStateHistory.has(roomId)) {
      this.gameStateHistory.set(roomId, []);
    }

    const history = this.gameStateHistory.get(roomId);
    history.push({
      state: gameState,
      timestamp: Date.now()
    });

    // Keep only last 100 states
    if (history.length > 100) {
      history.shift();
    }
  }

  // Clear state history
  clearStateHistory(roomId) {
    this.gameStateHistory.delete(roomId);
  }
}

// Export singleton instance
const gameStateManager = new GameStateManager();

module.exports = { gameStateManager, GameStateManager };