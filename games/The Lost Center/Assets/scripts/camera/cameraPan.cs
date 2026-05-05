
using UnityEngine;

public class cameraPan : MonoBehaviour
{
   public float panSpeed =1f; //speed of panning 
   private Vector3 lastMousePosition; //last tracked mouse position 

   void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePosition = Input.mousePosition; //track mouse position when middle mouse button is pressed 
        }
        if(Input.GetMouseButton(2))
        {
           Vector3 delta = Input.mousePosition - lastMousePosition; // calculate how much the mouse moved from the last frame 
           Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime; // convert mouse movement to world movement, invert x and y for intuitive panning
           Camera cam = Camera.main; // get the main camera
              cam.transform.Translate(move); // move the camera based on the calculated movement
              lastMousePosition = Input.mousePosition; // update the last mouse position for the next frame
        }
    }


   

}
