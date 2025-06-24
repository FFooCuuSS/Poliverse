using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public int thiefCnt = 7;
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D=new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D click = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if(click.collider != null)
            {
                GameObject clickedObj = click.collider.gameObject;  
                if(clickedObj.CompareTag("Thief")) 
                {
                    Debug.Log("µµµÏÀÌ¾ß");
                    thiefCnt--;
                    clickedObj.SetActive(false);
                }
                if(clickedObj.CompareTag("Police"))
                {
                    Debug.Log("³ª´Â °æÂû");
                }
            }
        }
        if(thiefCnt<=0)
        {
            GetComponent<Minigame_1_5>().Succeed();
            Debug.Log("success");
        }
    }
}
