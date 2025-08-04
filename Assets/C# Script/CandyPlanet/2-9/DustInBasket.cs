using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DustInBasket : MonoBehaviour
{
    public int dustCount = 0;

    private Minigame_2_9 minigame_2_9;
    public GameObject stage_2_9;

    private void Start()
    {
        minigame_2_9 = stage_2_9.GetComponent<Minigame_2_9>();
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.gameObject.tag == "Dust")
        {
            dustCount++;
            Debug.Log(dustCount);
            Destroy(coll.gameObject, 0.1f);
        }
    }

    private void Update()
    {
        if(dustCount == 20) //20은 임시 숫자
        {
            minigame_2_9.Succeed();
        }
    }
}
