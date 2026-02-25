using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    [SerializeField] private CapsuleCollider2D player;

    [SerializeField] private SpawnIcicle icicles;
    [SerializeField] private MiniGame2_2 minigame2_2;



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEntered();
        }
    }

    private void OnPlayerEntered()
    {
        if(icicles.index <= icicles.spawnDelays.Length)
        {
            minigame2_2.Succeed();
        }
        
    }
}
