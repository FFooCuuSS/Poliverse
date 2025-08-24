using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    public GameObject stage_1_5;
    private Minigame_1_5 minigame_1_5;
    public GameObject instantiater;
    private SpawnPolice1_5 spawnPolice1_5;

    int thiefCnt;

    private void Start()
    {
        minigame_1_5 = stage_1_5.GetComponent<Minigame_1_5>();
        spawnPolice1_5 = instantiater.GetComponent<SpawnPolice1_5>();
        thiefCnt = spawnPolice1_5.enemyCnt;
    }

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
                    minigame_1_5.Fail();
                }
            }
        }
        if(thiefCnt<=0)
        {
            minigame_1_5.Succeed();
        }
    }
}
