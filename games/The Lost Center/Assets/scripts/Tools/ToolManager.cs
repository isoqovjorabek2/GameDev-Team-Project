using UnityEngine;
using UnityEngine.UI;

public enum ToolType
{
    None,
    Point,
    Line,
    Circle
}

public class ToolManager : MonoBehaviour
{
    public ConstructionManager constructionManager;
    public PointTool pointTool;
    public LineTool lineTool;
    public CircleTool circleTool;

    public Button pointButton;
    public Button lineButton;
    public Button circleButton;
    public Button undoButton;
    public Button saveButton;
    public Button loadButton;

    private ToolType currentTool = ToolType.None;

    void Start()
    {
        SetupButtons();
        SetTool(ToolType.Point);
    }

    void SetupButtons()
    {
        if (pointButton != null)
            pointButton.onClick.AddListener(() => SetTool(ToolType.Point));

        if (lineButton != null)
            lineButton.onClick.AddListener(() => SetTool(ToolType.Line));

        if (circleButton != null)
            circleButton.onClick.AddListener(() => SetTool(ToolType.Circle));

        if (undoButton != null)
            undoButton.onClick.AddListener(Undo);

        if (saveButton != null)
            saveButton.onClick.AddListener(Save);

        if (loadButton != null)
            loadButton.onClick.AddListener(Load);
    }

    public void SetTool(ToolType tool)
    {
        currentTool = tool;

        if (pointTool != null)
            pointTool.enabled = (tool == ToolType.Point);

        if (lineTool != null)
            lineTool.enabled = (tool == ToolType.Line);

        if (circleTool != null)
            circleTool.enabled = (tool == ToolType.Circle);

        UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
        if (pointButton != null)
            pointButton.GetComponent<Image>().color = currentTool == ToolType.Point ? Color.green : Color.white;

        if (lineButton != null)
            lineButton.GetComponent<Image>().color = currentTool == ToolType.Line ? Color.green : Color.white;

        if (circleButton != null)
            circleButton.GetComponent<Image>().color = currentTool == ToolType.Circle ? Color.green : Color.white;
    }

    void Undo()
    {
        if (constructionManager != null)
            constructionManager.Undo();
    }

    void Save()
    {
        if (constructionManager != null)
            constructionManager.SaveConstruction();
    }

    void Load()
    {
        if (constructionManager != null)
            constructionManager.LoadConstruction();
    }

    public ToolType GetCurrentTool()
    {
        return currentTool;
    }

    public string GetCurrentToolName()
    {
        return currentTool.ToString();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SetTool(ToolType.Point);
        else if (Input.GetKeyDown(KeyCode.L))
            SetTool(ToolType.Line);
        else if (Input.GetKeyDown(KeyCode.C))
            SetTool(ToolType.Circle);
        else if (Input.GetKeyDown(KeyCode.Z) && Input.GetKey(KeyCode.LeftControl))
            Undo();
        else if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
            Save();
    }
}