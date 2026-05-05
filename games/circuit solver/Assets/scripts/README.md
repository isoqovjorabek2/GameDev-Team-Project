# Circuit Solver — Unity 2D Puzzle Game

A Unity 6 / URP puzzle game built around a Modified Nodal Analysis (MNA)
engine. Solve circuits by filling in the missing component values until
the circuit meets the puzzle target (a current, a node voltage, or a
voltage across a component).

## How to run

1. Open the project in **Unity 6000.3.7+** (URP, Input System 1.18+).
2. First time only: **Window → TextMeshPro → Import TMP Essentials**
   (required for every TMP-based Unity project).
3. Open `Assets/Scenes/SampleScene.unity`.
4. Press **Play**. The `GameBootstrapper` spawns every screen at runtime
   (menu, puzzle select, gameplay, result) so no prefab wiring is
   required.

### Optional — generate puzzle assets on disk

The runtime ships with 10 built-in puzzles that are generated in memory.
If you want to edit them as ScriptableObject assets:

* Menu: **Circuit Solver → Generate Default Puzzles**
* They're written to `Assets/ScriptableObjects/Puzzles/` and also copied
  into `Assets/Resources/Puzzles/` so the bootstrapper picks them up
  automatically.

## Folder map

```
Assets/
  Scripts/
    Core/        CircuitSolver (MNA), GaussianSolver, CircuitGraphBuilder, SpriteFactory
    Managers/    GameManager, PuzzleManager, UIManager, GameBootstrapper, DefaultPuzzleLibrary
    Components/  CircuitRenderer, ComponentSprite, ComponentInputField, WireRenderer, HighlightSystem
    Data/        CircuitComponent, CircuitNode, CircuitPuzzle SO, PuzzleTarget, Theme
    UI/          MainMenuScreen, PuzzleSelectScreen, GameplayHUDScreen, ResultScreen, UIFactory
    Editor/      PuzzleGeneratorMenu
  ScriptableObjects/Puzzles/   (created by the editor menu)
  Prefabs/                     (empty — everything is runtime-built)
  Scenes/SampleScene.unity
```

## Engine details

* **Modified Nodal Analysis** with Gaussian elimination + partial
  pivoting.
* Supports resistors, batteries, ideal voltage sources, and zero-Ω
  wires (wires are stamped as very-low-R resistors).
* Detects open circuits (unreachable nodes from ground), short circuits
  (explosive current) and singular systems, returning an explicit
  `SolveStatus` enum.
* Reports node voltages, signed component currents (A → B), and
  component voltage drops along with convenience aggregates
  (`R_eq`, `I_total`, `P_total`) shown on the HUD.

## Puzzle progression

| # | Title                   | Concept                              |
|---|-------------------------|--------------------------------------|
| 1 | First Current           | Ohm's Law, 1 unknown                 |
| 2 | Mystery Resistor        | Series resistors                     |
| 3 | Voltage Divider         | Node voltage target                  |
| 4 | Parallel Twins          | Two branches, one hidden R           |
| 5 | Mirror Branches         | Parallel pair, both hidden           |
| 6 | Mixed Branch            | R1 + (R2 ‖ R3)                       |
| 7 | Dual Battery            | Two series EMFs (KVL)                |
| 8 | Balanced Pair           | Voltage divider symmetry             |
| 9 | Ladder Network          | Three-node mesh                      |
|10 | Full Wheatstone Bridge  | Balance condition (I = 0)            |

All targets have been numerically verified against the solver.

## Palette

| Role                 | Hex       |
|----------------------|-----------|
| Background (navy)    | `#0D1B2A` |
| Board (cream)        | `#F0EDE8` |
| Wires (orange)       | `#FF6B35` |
| Battery / accent     | `#00FF87` |
| Resistor body        | `#FF8C42` |
| Danger red           | `#FF3B5C` |

## Extending

* Add a new puzzle: either author a `CircuitPuzzle` ScriptableObject in
  `Assets/Resources/Puzzles/` or extend `DefaultPuzzleLibrary`.
* Each `CircuitComponent` has `nodeA` / `nodeB` integer terminals. Node
  `0` is ground; use identical node ids across components to wire them
  together.
* New target kinds can be added to `TargetKind` and handled inside
  `PuzzleManager.EvaluateTarget`.
