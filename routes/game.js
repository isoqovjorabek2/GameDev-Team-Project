const express = require("express");
const router = express.Router();
const User = require("../models/User");
const jwt = require("jsonwebtoken");


console.log("game.js file loaded");
// token tekshiruvchi middleware
function auth(req, res, next) {
  const token = req.header("Authorization");

  if (!token) {
    return res.status(401).json({ message: "No token" });
  }

  try {
    const verified = jwt.verify(token, process.env.JWT_SECRET);
    req.userId = verified.id;
    next();
  } catch (err) {
    res.status(401).json({ message: "Invalid token" });
  }
}

// SAVE SCORE
router.post("/score", auth, async (req, res) => {
  try {
    const { score } = req.body;

    const user = await User.findByIdAndUpdate(
      req.userId,
      { score },
      { new: true }
    );

    res.json({
      message: "Score saved",
      username: user.username,
      score: user.score
    });
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

// LEADERBOARD
router.get("/leaderboard", async (req, res) => {
  try {
    const users = await User.find()
      .select("username score")
      .sort({ score: -1 })
      .limit(10);

    res.json(users);
  } catch (err) {
    res.status(500).json({ message: err.message });
  }
});

module.exports = router;