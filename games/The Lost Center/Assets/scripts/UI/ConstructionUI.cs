using UnityEngine;
using UnityEngine.UI;

public class ConstructionUI : MonoBehaviour
{
    public ToolManager toolManager;
    public ConstructionManager constructionManager;

    [Header("Tool Buttons")]
    public Button pointToolButton;
    public Button lineToolButton;
    public Button circleToolButton;

    [Header("Action Buttons")]
    public Button undoButton;
    public Button redoButton;
    public Button clearButton;

    [Header("File Buttons")]
    public Button saveButton;
    public Button loadButton;

    [Header("Options")]
    public Toggle snapToGridToggle;
    public Toggle showIntersectionsToggle;

    [Header("Info Display")]
    public Text toolInfoText;
    public Text statsText;

    void Start()
    {
        SetupUI();
        UpdateStats();
    }

    void SetupUI()
    {
        if (toolManager != null)
        {
            if (pointToolButton != null)
                pointToolButton.onClick.AddListener(() => toolManager.SetTool(ToolType.Point));

            if (lineToolButton != null)
                lineToolButton.onClick.AddListener(() => toolManager.SetTool(ToolType.Line));

            if (circleToolButton != null)
                circleToolButton.onClick.AddListener(() => toolManager.SetTool(ToolType.Circle));
        }

        if (undoButton != null)
            undoButton.onClick.AddListener(() => constructionManager?.Undo());

        if (redoButton != null)
            redoButton.onClick.AddListener(() => constructionManager?.Redo());

        if (clearButton != null)
            clearButton.onClick.AddListener(ClearConstruction);

        if (saveButton != null)
            saveButton.onClick.AddListener(() => constructionManager?.SaveConstruction());

        if (loadButton != null)
            loadButton.onClick.AddListener(() => constructionManager?.LoadConstruction());

        if (snapToGridToggle != null)
            snapToGridToggle.onValueChanged.AddListener(OnSnapToGridChanged);

        if (showIntersectionsToggle != null)
            showIntersectionsToggle.onValueChanged.AddListener(OnShowIntersectionsChanged);
    }

    void ClearConstruction()
    {
        if (constructionManager != null)
        {
            constructionManager.ClearConstruction();
            UpdateStats();
        }
    }

    void OnSnapToGridChanged(bool enabled)
    {
        // Implement snap to grid functionality
        Debug.Log("Snap to grid: " + enabled);
    }

    void OnShowIntersectionsChanged(bool enabled)
    {
        // Implement intersection display functionality
        Debug.Log("Show intersections: " + enabled);
    }

    void Update()
    {
        UpdateStats();
    }

    void UpdateStats()
    {
        if (statsText != null && constructionManager != null)
        {
            statsText.text = $"Points: {constructionManager.points.Count} | Lines: {constructionManager.lines.Count} | Circles: {constructionManager.circles.Count}";
        }

        if (toolInfoText != null && toolManager != null)
        {
            toolInfoText.text = $"Current Tool: {toolManager.GetCurrentToolName()}";
        }
    }
}