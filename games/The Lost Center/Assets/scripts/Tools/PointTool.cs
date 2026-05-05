using UnityEngine;

public class PointTool : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ConstructionManager constructionManager;
 
    // Update is called once per frame
    void Update()
    {
         if (Input.GetMouseButtonDown(0)) // left click
        {
            Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            constructionManager.CreatePoint(world);
        }
    }
}
