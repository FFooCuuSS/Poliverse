using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterChild : MonoBehaviour
{
    public float attractionForce = 4f;
    public float attractionRadius = 5f;

    private Minigame_2_14 miniGame;

    void Start()
    {
        miniGame = GetComponentInParent<Minigame_2_14>();
    }

    void Update()
    {
        if (miniGame == null || miniGame.IsSuccess || miniGame.IsInputLocked)
            return;

        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject food in foods)
        {
            DragAndDrop drag = food.GetComponent<DragAndDrop>();
            if (drag != null && drag.isDragging) continue;

            Vector3 dir = transform.position - food.transform.position;
            float dist = dir.magnitude;

            if (dist < attractionRadius)
            {
                food.transform.position += dir.normalized * attractionForce * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Food"))
        {
            miniGame?.Failure();
        }
    }
}
