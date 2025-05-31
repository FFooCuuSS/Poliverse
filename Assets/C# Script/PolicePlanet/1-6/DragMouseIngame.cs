using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class DragMouseIngame : MonoBehaviour
{
    private void OnMouseDrag()
    {
        transform.position = GetMousePos();
        GameObject clickedObj = this.gameObject;
        if(clickedObj.CompareTag("Police") )
        {
           // Debug.Log("Police");
        }
    }

    Vector3 GetMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
    
}
