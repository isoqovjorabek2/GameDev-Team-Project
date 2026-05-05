using UnityEngine;

public class PointView : MonoBehaviour
{
    public ConstructionPoint data; // The construction point this view represents
    public void Initialize(ConstructionPoint constructionPoint)
    {
        data = constructionPoint;
        transform.position = constructionPoint.geoPoint.position; // Set the view's position to match the construction point's geographic position
    }

    void Update()
    {
        transform.position = data.geoPoint.position; // Continuously update the view's position to match the construction point's geographic position
    }


}
