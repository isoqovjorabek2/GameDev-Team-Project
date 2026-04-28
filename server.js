const express = require("express");
const mongoose = require("mongoose");
const cors = require("cors");
require("dotenv").config();

const app = express();

app.use(cors());
app.use(express.json());

app.get("/test", (req, res) => {
  res.json({ message: "Server works" });
});

const authRoutes = require("./routes/auth");
const gameRoutes = require("./routes/game");

app.use("/api/auth", authRoutes);
app.use("/api/game", gameRoutes);

console.log("Game routes loaded");

mongoose.connect(process.env.MONGO_URI)
  .then(() => console.log("MongoDB connected"))
  .catch(err => console.log(err));

const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
});

console.log("THIS IS MY SERVER FILE");