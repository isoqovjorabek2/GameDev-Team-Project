using UnityEngine;

public class LineTool : MonoBehaviour
{
    public ConstructionManager constructionManager;
    public ConstructionPoint first = null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ConstructionPoint p = ConstructionUtils.FindClosestPoint(pos, constructionManager.points);

            if (p == null)
            {
                return;
            }

            if (first == null)
            {
                first = p;
            }
            else
            {
                constructionManager.CreateLine(first, p);
                first = null;
            }
        }
    }
}
