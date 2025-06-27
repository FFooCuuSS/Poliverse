using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckFalse1_1 : MonoBehaviour
{
    public GameObject stage_1_1;
    private Minigame_1_1 minigmae_1_1;

    private bool isChecked = false;

    private void Start()
    {
        minigmae_1_1 = stage_1_1.GetComponent<Minigame_1_1>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy") && !isChecked)
        {
            Debug.Log("¼º°ø");
            isChecked = true;
            minigmae_1_1.Success();
        }
    }
    
}
