using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ConstructionManager : MonoBehaviour
{
    public PointView pointPrefab;
    public LineView linePrefab;
    public CircleView circlePrefab;

    private int nextId = 0;
    public List<ConstructionPoint> points = new List<ConstructionPoint>();
    public List<ConstructionLine> lines = new List<ConstructionLine>();
    public List<ConstructionCircle> circles = new List<ConstructionCircle>();

    private Stack<ConstructionAction> undoStack = new Stack<ConstructionAction>();
    private Stack<ConstructionAction> redoStack = new Stack<ConstructionAction>();

    private const string SAVE_FILE = "construction_save.json";

    public ConstructionPoint CreatePoint(GeoPoint position)
    {
        ConstructionPoint p = new ConstructionPoint(nextId++, position);
        points.Add(p);
        PointView view = Instantiate(pointPrefab);
        view.Initialize(p);
        return p;
    }

    public ConstructionLine CreateLine(ConstructionPoint start, ConstructionPoint end)
    {
        if (!ConstructionUtils.IsValidConstruction(start) || !ConstructionUtils.IsValidConstruction(end))
        {
            Debug.LogError("Invalid points for line construction");
            return null;
        }

        if (start == end)
        {
            Debug.LogError("Cannot create line with identical points");
            return null;
        }

        ConstructionLine line = new ConstructionLine(nextId++, start, end);
        lines.Add(line);
        LineView view = Instantiate(linePrefab);
        view.Initialize(line);
        return line;
    }

    public ConstructionCircle CreateCircle(ConstructionPoint center, ConstructionPoint radius)
    {
        if (!ConstructionUtils.IsValidConstruction(center) || !ConstructionUtils.IsValidConstruction(radius))
        {
            Debug.LogError("Invalid points for circle construction");
            return null;
        }

        if (center == radius)
        {
            Debug.LogError("Cannot create circle with identical center and radius points");
            return null;
        }

        ConstructionCircle circle = new ConstructionCircle(center, radius);
        circles.Add(circle);
        CircleView view = Instantiate(circlePrefab);
        view.Initialize(circle);
        return circle;
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            ConstructionAction action = undoStack.Pop();
            redoStack.Push(action);
            action.Undo(this);
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            ConstructionAction action = redoStack.Pop();
            undoStack.Push(action);
            action.Redo(this);
        }
    }

    public void SaveConstruction()
    {
        ConstructionData data = new ConstructionData();
        data.points = new List<PointData>();

        foreach (var point in points)
        {
            data.points.Add(new PointData
            {
                id = point.id,
                x = point.geoPoint.position.x,
                y = point.geoPoint.position.y,
                isDerived = point.isDerived
            });
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        File.WriteAllText(path, json);
        Debug.Log("Construction saved to: " + path);
    }

    public void LoadConstruction()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILE);
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        ConstructionData data = JsonUtility.FromJson<ConstructionData>(json);

        ClearConstruction();

        foreach (var pointData in data.points)
        {
            Vector2 position = new Vector2(pointData.x, pointData.y);
            GeoPoint geoPoint = new GeoPoint(position);
            ConstructionPoint point = new ConstructionPoint(pointData.id, geoPoint, pointData.isDerived);
            points.Add(point);

            PointView view = Instantiate(pointPrefab);
            view.Initialize(point);
        }

        nextId = points.Count > 0 ? points[points.Count - 1].id + 1 : 0;
        Debug.Log("Construction loaded from: " + path);
    }

    public void ClearConstruction()
    {
        points.Clear();
        lines.Clear();
        circles.Clear();
        undoStack.Clear();
        redoStack.Clear();
    }
}

[System.Serializable]
public class ConstructionData
{
    public List<PointData> points;
}

[System.Serializable]
public class PointData
{
    public int id;
    public float x;
    public float y;
    public bool isDerived;
}

public abstract class ConstructionAction
{
    public abstract void Undo(ConstructionManager manager);
    public abstract void Redo(ConstructionManager manager);
}

public class CreatePointAction : ConstructionAction
{
    private ConstructionPoint point;

    public CreatePointAction(ConstructionPoint point)
    {
        this.point = point;
    }

    public override void Undo(ConstructionManager manager)
    {
        manager.points.Remove(point);
    }

    public override void Redo(ConstructionManager manager)
    {
        manager.points.Add(point);
    }
}