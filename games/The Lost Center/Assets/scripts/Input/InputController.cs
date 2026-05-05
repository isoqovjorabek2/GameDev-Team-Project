using UnityEngine;

public class InputController : MonoBehaviour
{
    public ConstructionManager constructionManager;
   

    void Update()
    {
        
    }

    ConstructionPoint FindClosestPoint(Vector2 pos)
    {
        return ConstructionUtils.FindClosestPoint(pos, constructionManager.points);
    }

    
}
