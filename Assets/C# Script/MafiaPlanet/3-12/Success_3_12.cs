using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Success_3_12 : MonoBehaviour
{
    public GameObject stage_3_12;
    private Minigame_3_12 minigame_3_12;

    [Header("충돌이 성공으로 인정될 대상")]
    public Collider2D targetCollider;

    private void Start()
    {
        minigame_3_12 = stage_3_12.GetComponent<Minigame_3_12>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == targetCollider)
        {
            BlockingDirectionalDrag_3_12 bdd = other.GetComponent<BlockingDirectionalDrag_3_12>();
            minigame_3_12.Succeed();
            bdd.Revealed();
        }
    }
}
