using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeadArea : MonoBehaviour
{
    [SerializeField] private BoxCollider2D deadArea;
    [SerializeField] private CircleCollider2D jelly;

    private Minigame_2_7 minigame_2_7;
    public GameObject stage_2_7;
   

    private void Start()
    {
        minigame_2_7 = stage_2_7.GetComponent<Minigame_2_7>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            minigame_2_7.Failure();
        }
    }

}
