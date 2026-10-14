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

    [Header("미니게임")]
    [SerializeField] private Minigame_3_5_remake minigame;

    private bool isAttacking = false;
    private bool isDemoMode = false;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
        Debug.Log("[ScopeShooter] 데모 모드: " + isDemoMode);
    }

    private void Update()
    {
        // 데모 모드에서는 플레이어 클릭을 처리하지 않음
        if (isDemoMode)
        {
            if (!isAttacking &&
                isEnemyInTrigger &&
                currentEnemy != null)
            {
                HitCurrentEnemy();
            }

            return;
        }

        // 일반 플레이에서만 마우스 클릭 허용
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[3-5] Mouse Clicked!");

            if (isEnemyInTrigger && currentEnemy != null)
            {
                HitCurrentEnemy();
            }
            else
            {
                PlaySound(missSound);
                Debug.Log("[3-5] Missed!");
            }
        }
    }

    private void HitCurrentEnemy()
    {
        if (isAttacking || currentEnemy == null)
            return;

        isAttacking = true;

        Collider2D enemyToDestroy = currentEnemy;

        currentEnemy = null;
        isEnemyInTrigger = false;

        PlaySound(hitSound);

        Debug.Log(
            isDemoMode
                ? "[3-5] 데모 자동 공격!"
                : "[3-5] Enemy Hit!"
        );

        Destroy(enemyToDestroy.gameObject);

        if (isDemoMode && minigame != null)
        {
            StartCoroutine(CheckCompletionAfterHit());
        }
        else
        {
            isAttacking = false;
        }
    }

    private IEnumerator CheckCompletionAfterHit()
    {
        if (minigame != null)
        {
            yield return StartCoroutine(
                minigame.CheckDemoCompletion()
            );
        }

        isAttacking = false;
    }


    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        if (GameRoot.Instance != null &&
            GameRoot.Instance.Audio != null)
        {
            GameRoot.Instance.Audio.PlaySfx(clip);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        isEnemyInTrigger = true;
        currentEnemy = collision;

        Debug.Log("[3-5] 적이 스코프 범위에 들어옴");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (currentEnemy != collision)
            return;

        isEnemyInTrigger = false;
        currentEnemy = null;
    }

    private void OnDisable()
    {
        currentEnemy = null;
        isEnemyInTrigger = false;
        isAttacking = false;
    }
}