using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeShooter : MonoBehaviour
{

    [Header("효과음")]
    [SerializeField] private AudioClip hitSound;
    [Header("효과음")]
    [SerializeField] private AudioClip missSound;
    private bool isEnemyInTrigger = false;
    private Collider2D currentEnemy;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mouse Clicked!");

            if (isEnemyInTrigger && currentEnemy != null)
            {
                if (hitSound != null)
                {
                    GameRoot.Instance.Audio.PlaySfx(hitSound);
                }


                Debug.Log("Enemy Hit!");
                Destroy(currentEnemy.gameObject);
            }
            else
            {
                if (missSound != null)
                {
                    GameRoot.Instance.Audio.PlaySfx(missSound);
                }
                Debug.Log("Missed!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            isEnemyInTrigger = true;
            currentEnemy = collision;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            isEnemyInTrigger = false;
            currentEnemy = null;
        }
    }
}