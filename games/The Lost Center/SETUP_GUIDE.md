# Unity Scene Setup Guide

This guide will help you set up "The Lost Center" project in Unity with all the new features.

## Step 1: Basic Scene Setup

1. Open Unity and create a new 2D project
2. Import the project files into the Assets folder
3. Open the SampleScene (or create a new scene)

## Step 2: Create Construction Manager

1. Create an empty GameObject: `GameObject > Create Empty`
2. Name it "ConstructionManager"
3. Add the `ConstructionManager` component:
   - Click "Add Component"
   - Search for "ConstructionManager"
   - Select it
4. In the Inspector, assign the prefabs:
   - **Point Prefab**: Drag `Assets/prefabs/point.prefab`
   - **Line Prefab**: Drag `Assets/prefabs/LineRenderer.prefab`
   - **Circle Prefab**: Drag `Assets/prefabs/CircleRenderer.prefab`

## Step 3: Create Tools

### Point Tool
1. Create empty GameObject named "PointTool"
2. Add `PointTool` component
3. Assign ConstructionManager reference

### Line Tool
1. Create empty GameObject named "LineTool"
2. Add `LineTool` component
3. Assign ConstructionManager reference
4. **Disable** the component (uncheck the box next to component name)

### Circle Tool
1. Create empty GameObject named "CircleTool"
2. Add `CircleTool` component
3. Assign ConstructionManager reference
4. **Disable** the component

## Step 4: Create Tool Manager

1. Create empty GameObject named "ToolManager"
2. Add `ToolManager` component
3. Assign references in Inspector:
   - **Construction Manager**: Drag the ConstructionManager GameObject
   - **Point Tool**: Drag the PointTool GameObject
   - **Line Tool**: Drag the LineTool GameObject
   - **Circle Tool**: Drag the CircleTool GameObject

## Step 5: Setup Camera Controls

1. Select the Main Camera
2. Add `cameraPan` component
3. Add `cameraZoom` component
4. Configure camera settings:
   - Set Orthographic mode
   - Adjust size (recommended: 10-20)
   - Set background color if desired

## Step 6: Create UI (Optional but Recommended)

### Canvas Setup
1. Create UI Canvas: `GameObject > UI > Canvas`
2. Set Canvas Scaler to "Scale With Screen Size"

### Tool Buttons
1. Create buttons for each tool:
   - `GameObject > UI > Button` - name it "PointButton"
   - Duplicate for "LineButton" and "CircleButton"
2. Position buttons at top of screen
3. Change button text to "Point", "Line", "Circle"

### Action Buttons
1. Create buttons for actions:
   - "UndoButton", "RedoButton", "ClearButton"
2. Position below tool buttons

### File Buttons
1. Create buttons for file operations:
   - "SaveButton", "LoadButton"
2. Position below action buttons

### Info Display
1. Create Text elements:
   - "ToolInfoText" - shows current tool
   - "StatsText" - shows point/line/circle counts
2. Position at bottom of screen

### Setup ConstructionUI
1. Create empty GameObject named "ConstructionUI"
2. Add `ConstructionUI` component
3. Assign all button and text references in Inspector

## Step 7: Configure Tool Manager with UI

1. Select ToolManager GameObject
2. Assign UI button references:
   - **Point Button**: Drag PointButton
   - **Line Button**: Drag LineButton
   - **Circle Button**: Drag CircleButton
   - **Undo Button**: Drag UndoButton
   - **Save Button**: Drag SaveButton
   - **Load Button**: Drag LoadButton

## Step 8: Test the Setup

1. Enter Play mode
2. Test basic controls:
   - Press 'P' to select Point tool
   - Click to create points
   - Press 'L' to select Line tool
   - Click two points to create a line
   - Press 'C' to select Circle tool
   - Click center and radius points
3. Test camera controls:
   - Middle mouse drag to pan
   - Scroll to zoom
4. Test UI buttons (if created)

## Step 9: Configure Additional Features

### Snap to Grid
1. In PointTool, modify the CreatePoint call to use:
   ```csharp
   Vector2 snappedPos = ConstructionUtils.SnapToGrid(world);
   constructionManager.CreatePoint(snappedPos);
   ```

### Show Intersections
1. Create a new script `IntersectionView.cs`
2. Use `GeometricConstraints.FindAllIntersections()` to find intersections
3. Display them with small markers

## Common Issues and Solutions

### Tools not responding
- **Problem**: Clicking doesn't create points/lines/circles
- **Solution**: Check that ToolManager is enabled and tools are properly assigned

### Points not visible
- **Problem**: Points are created but not visible
- **Solution**: Ensure point prefab has a SpriteRenderer or MeshRenderer

### Lines not drawing
- **Problem**: Lines are created but not visible
- **Solution**: Check LineRenderer material and width settings

### Camera controls not working
- **Problem**: Can't pan or zoom
- **Solution**: Verify cameraPan and cameraZoom components are on Main Camera

### UI buttons not working
- **Problem**: Clicking buttons does nothing
- **Solution**: Check that ConstructionUI has all button references assigned

## Scene Hierarchy Example

```
Scene
├── Main Camera
│   ├── Camera Controller
│   │   ├── cameraPan
│   │   └── cameraZoom
├── ConstructionManager
├── ToolManager
├── PointTool
├── LineTool (disabled)
├── CircleTool (disabled)
├── ConstructionUI
└── Canvas
    ├── PointButton
    ├── LineButton
    ├── CircleButton
    ├── UndoButton
    ├── RedoButton
    ├── ClearButton
    ├── SaveButton
    ├── LoadButton
    ├── ToolInfoText
    └── StatsText
```

## Next Steps

Once basic setup is complete:
1. Customize colors and materials
2. Add more geometric tools
3. Implement construction constraints
4. Add measurement tools
5. Create construction tutorials

## Support

For issues or questions:
- Check the Console for error messages
- Verify all component references are assigned
- Ensure prefabs are properly configured
- Test with a fresh scene if problems persist