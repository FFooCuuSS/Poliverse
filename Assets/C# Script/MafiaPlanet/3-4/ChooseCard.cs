using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseCard : MiniGameBase
{
    public bool start;
    public bool isSuccess;
    public bool isFailure;
    void Start()
    {
        start = false;
        isSuccess = false;
        isFailure = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            ChooseCardStart();
        }
    }
    void ChooseCardStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D click = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (click.collider != null)
            {
                GameObject clickedObj = click.collider.gameObject;
                if (clickedObj.GetComponent<CardColor>().isTrapCard)
                {
                    isSuccess = true;
                    base.Success();

                }
                else { isFailure = true;
                    base.Fail();
                }
                clickedObj.SetActive(false);
            }
        }
    }
}
