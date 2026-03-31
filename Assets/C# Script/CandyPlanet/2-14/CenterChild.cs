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
        if (miniGame == null) return;

        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");

        foreach (GameObject food in foods)
        {
            Vector3 dir = transform.position - food.transform.position; // 플레이어 중심 방향
            float dist = dir.magnitude;

            if (dist < attractionRadius) // 끌어당김 범위 내
            {
                food.transform.position += dir.normalized * attractionForce * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Food")) return;

        Debug.Log("음식이 가운데 닿음 → 실패 처리");

        if (miniGame != null)
        {
            // 실패 카운트 추가 등 처리 가능
            // 예: miniGame.FailCount++;

            // 필요 시 실패 이벤트 호출 등
        }

        Destroy(col.gameObject);
    }
}
