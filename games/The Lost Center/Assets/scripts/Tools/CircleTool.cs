using UnityEngine;

public class CircleTool : MonoBehaviour
{
    public ConstructionPoint first = null;
    public ConstructionManager constructionManager;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            constructionManager.CreatePoint(world);

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
                constructionManager.CreateCircle(first, p);
                first = null;
            }
        }
    }
}
