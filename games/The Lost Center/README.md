# The Lost Center - Geometry Construction Tool

A Unity-based geometry construction tool for creating points, lines, and circles using compass and straightedge methods.

## Features

### Core Functionality
- **Point Tool**: Create construction points by clicking
- **Line Tool**: Create lines between two existing points
- **Circle Tool**: Create circles defined by center and radius points
- **Camera Controls**: Pan with middle mouse, zoom with scroll wheel

### New Improvements
- **Tool Switching**: Press P (Point), L (Line), C (Circle) to switch tools
- **Undo/Redo**: Ctrl+Z to undo, Ctrl+Y to redo
- **Save/Load**: Ctrl+S to save, Ctrl+O to load constructions
- **Construction Validation**: Prevents invalid geometric constructions
- **Geometric Constraints**: Snap-to-grid and intersection detection
- **Code Organization**: Shared utility methods and reduced duplication

## Setup Instructions

### 1. Scene Setup
1. Create a new Unity 2D project or open existing one
2. Create an empty GameObject named "ConstructionManager"
3. Add the `ConstructionManager` component
4. Assign prefabs:
   - Point Prefab: Assets/prefabs/point.prefab
   - Line Prefab: Assets/prefabs/LineRenderer.prefab
   - Circle Prefab: Assets/prefabs/CircleRenderer.prefab

### 2. Tool Setup
1. Create empty GameObjects for each tool:
   - "PointTool" with `PointTool` component
   - "LineTool" with `LineTool` component
   - "CircleTool" with `CircleTool` component
2. Assign the ConstructionManager reference to each tool
3. Initially disable all tools (they will be enabled by ToolManager)

### 3. Tool Manager Setup
1. Create "ToolManager" GameObject with `ToolManager` component
2. Assign references:
   - ConstructionManager
   - PointTool, LineTool, CircleTool references
3. Create UI buttons and assign them to the ToolManager

### 4. Camera Setup
1. Add `cameraPan` component to main camera
2. Add `cameraZoom` component to main camera
3. Configure zoom speed and limits as needed

### 5. UI Setup (Optional)
1. Create a Canvas with UI buttons:
   - Tool selection buttons (Point, Line, Circle)
   - Action buttons (Undo, Redo, Clear)
   - File buttons (Save, Load)
   - Toggle buttons (Snap to Grid, Show Intersections)
2. Add `ConstructionUI` component to a GameObject
3. Assign all button and text references

## Usage

### Basic Controls
- **Left Click**: Create points or select points for lines/circles
- **Middle Mouse + Drag**: Pan camera
- **Scroll Wheel**: Zoom in/out
- **P**: Switch to Point tool
- **L**: Switch to Line tool
- **C**: Switch to Circle tool
- **Ctrl+Z**: Undo last action
- **Ctrl+Y**: Redo last action
- **Ctrl+S**: Save construction
- **Ctrl+O**: Load construction

### Creating Constructions
1. **Points**: Select Point tool and click anywhere
2. **Lines**: Select Line tool, click first point, then second point
3. **Circles**: Select Circle tool, click center point, then radius point

### Geometric Features
- **Snap to Grid**: Points automatically snap to grid intersections
- **Intersection Detection**: Automatically find and display intersections between lines and circles
- **Validation**: System prevents invalid constructions (identical points, etc.)

## Code Structure

### Core Scripts
- `ConstructionManager.cs`: Manages all construction objects and operations
- `ConstructionPoint.cs`, `ConstructionLine.cs`, `ConstructionCircle.cs`: Data models
- `GeoPoint.cs`, `GeoLine.cs`, `GeoCircle.cs`: Geometric primitives

### Tool Scripts
- `PointTool.cs`, `LineTool.cs`, `CircleTool.cs`: Individual tool implementations
- `ToolManager.cs`: Central tool switching and management

### Utility Scripts
- `ConstructionUtils.cs`: Shared utility methods (finding closest points, validation)
- `GeometricConstraints.cs`: Intersection detection and geometric calculations

### View Scripts
- `PointView.cs`, `LineView.cs`, `CircleView.cs`: Rendering and visualization

### UI Scripts
- `ConstructionUI.cs`: UI management and user interaction

## File Structure
```
Assets/
├── scripts/
│   ├── Construction/
│   │   ├── ConstructionManager.cs
│   │   ├── ConstructionPoint.cs
│   │   ├── ConstructionLine.cs
│   │   └── ConstructionCircle.cs
│   ├── Geometry/
│   │   ├── GeoPoint.cs
│   │   ├── GeoLine.cs
│   │   └── GeoCircle.cs
│   ├── Tools/
│   │   ├── PointTool.cs
│   │   ├── LineTool.cs
│   │   ├── CircleTool.cs
│   │   └── ToolManager.cs
│   ├── View/
│   │   ├── PointView.cs
│   │   ├── LineView.cs
│   │   └── CircleView.cs
│   ├── Utils/
│   │   ├── ConstructionUtils.cs
│   │   └── GeometricConstraints.cs
│   ├── Input/
│   │   └── InputController.cs
│   ├── UI/
│   │   └── ConstructionUI.cs
│   └── camera/
│       ├── cameraPan.cs
│       └── cameraZoom.cs
├── prefabs/
│   ├── point.prefab
│   ├── LineRenderer.prefab
│   └── CircleRenderer.prefab
└── Scenes/
    └── SampleScene.unity
```

## Future Enhancements
- Additional geometric tools (midpoint, perpendicular, angle bisector)
- Measurement tools (distance, angle)
- Construction steps recording and playback
- Export to image or vector formats
- More advanced intersection algorithms
- Constraint-based construction system

## Troubleshooting
- **Tools not working**: Ensure ToolManager is properly configured and tools are assigned
- **Points not visible**: Check that point prefab is assigned and has proper sprite/renderer
- **Lines not drawing**: Verify LineRenderer prefab and material settings
- **Save/Load not working**: Check file permissions and Application.persistentDataPath

## License
This is a learning project for geometry construction in Unity.