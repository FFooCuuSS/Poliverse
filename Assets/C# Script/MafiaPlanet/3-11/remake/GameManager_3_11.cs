using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_3_11 : MonoBehaviour
{
    [Header("연결할 스크립트")]
    [SerializeField] private ObjectSpawner_3_11 objectSpawner;
    [SerializeField] private SliceZone_3_11 sliceZone;

    [Header("점수 설정")]
    [SerializeField] private int targetScore = 1;
    [SerializeField] private int bombPenalty = 1;

    [SerializeField] private GameObject bombEffect;
    // 현재 점수
    private int currentScore = 0;

    // 게임이 진행 중인지 확인
    private bool isPlaying = false;

    // 외부에서 현재 점수를 확인할 때 사용
    public int CurrentScore => currentScore;

    // 외부에서 게임 진행 상태를 확인할 때 사용
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        // 기존 Minigame_3_11_remake와 동일하게 좌클릭을 사용한다.
        // Minigame_3_11_remake는 리듬 입력을 전달하고,
        // 이 스크립트는 중앙에 있는 물체의 점수를 처리한다.
        if (Input.GetMouseButtonDown(0))
        {
            CheckSliceInput();
        }
    }

    /// <summary>
    /// 게임 시작 시 점수를 초기화하고 Spawn을 시작한다.
    /// </summary>
    public void StartGame()
    {
        currentScore = 0;
        isPlaying = true;

        if (sliceZone != null)
        {
            sliceZone.ClearZone();
        }

        if (objectSpawner != null)
        {
            objectSpawner.BeginSpawn();
        }
        else
        {
            Debug.LogWarning(
                "[3-11] ObjectSpawner_3_11이 연결되지 않았습니다."
            );
        }

        Debug.Log("[3-11] 게임 시작");
        Debug.Log("[3-11] 현재 점수: " + currentScore);
    }

    /// <summary>
    /// 좌클릭 시 중앙 SliceZone에 있는 물체를 확인한다.
    /// </summary>
    private void CheckSliceInput()
    {
        if (sliceZone == null)
        {
            Debug.LogWarning(
                "[3-11] SliceZone_3_11이 연결되지 않았습니다."
            );

            return;
        }

        // 중앙 판정 구역에 있는 물체 중
        // 중앙에 가장 가까운 물체 하나를 가져온다.
        FlyingObject_3_11 hitObject =
            sliceZone.GetClosestObject();

        // 판정 구역에 아무것도 없는 경우
        if (hitObject == null)
        {
            Debug.Log(
                "[3-11] 빈 공간 클릭 / 현재 점수: " +
                currentScore
            );

            return;
        }

        ProcessHitObject(hitObject);
    }

    /// <summary>
    /// 충돌한 물체의 태그를 확인하고 점수를 처리한다.
    /// </summary>
    private void ProcessHitObject(
        FlyingObject_3_11 hitObject
    )
    {
        if (hitObject == null)
        {
            return;
        }

        // 이미 처리된 물체를 다시 누른 경우 무시한다.
        if (hitObject.IsProcessed)
        {
            return;
        }

        // 서류 또는 목표물 처리
        if (hitObject.CompareTag("Target"))
        {
            AddTargetScore();
        }
        // 폭탄 처리
        else if (hitObject.CompareTag("Bomb"))
        {
            AddBombPenalty();
        }
        else
        {
            Debug.LogWarning(
                "[3-11] 알 수 없는 태그입니다: " +
                hitObject.tag
            );

            return;
        }

        // SliceZone 내부 목록에서 제거한다.
        sliceZone.RemoveObject(hitObject);

        // 물체를 처리한 뒤 제거한다.
        hitObject.ProcessObject();
    }
    public void PlayBombEffect()
    {
        StartCoroutine(BombEffectCoroutine());
    }

    private IEnumerator BombEffectCoroutine()
    {
        bombEffect.SetActive(true);

        yield return new WaitForSeconds(1f);

        bombEffect.SetActive(false);
    }
    /// <summary>
    /// Target 적중 시 점수를 증가시킨다.
    /// </summary>
    private void AddTargetScore()
    {
        currentScore += targetScore;

        Debug.Log(
            "[3-11] Target 적중 / +" +
            targetScore +
            "점 / 현재 점수: " +
            currentScore
        );
    }

    /// <summary>
    /// Bomb 적중 시 점수를 감소시킨다.
    /// </summary>
    private void AddBombPenalty()
    {
        PlayBombEffect();
        currentScore -= bombPenalty;

        Debug.Log(
            "[3-11] Bomb 적중 / -" +
            bombPenalty +
            "점 / 현재 점수: " +
            currentScore
        );
    }

    /// <summary>
    /// 외부에서 점수를 직접 증가시킬 때 사용할 수 있다.
    /// </summary>
    public void AddScore(int amount)
    {
        currentScore += amount;

        Debug.Log(
            "[3-11] 점수 증가: +" +
            amount +
            " / 현재 점수: " +
            currentScore
        );
    }

    /// <summary>
    /// 외부에서 점수를 직접 감소시킬 때 사용할 수 있다.
    /// </summary>
    public void SubtractScore(int amount)
    {
        currentScore -= amount;

        Debug.Log(
            "[3-11] 점수 감소: -" +
            amount +
            " / 현재 점수: " +
            currentScore
        );
    }

    /// <summary>
    /// 게임을 정지한다.
    /// 현재는 종료 결과 없이 로그만 출력한다.
    /// </summary>
    public void StopGame()
    {
        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;

        if (objectSpawner != null)
        {
            objectSpawner.StopSpawn();
        }

        if (sliceZone != null)
        {
            sliceZone.ClearZone();
        }

        Debug.Log(
            "[3-11] 게임 정지 / 최종 점수: " +
            currentScore
        );
    }

    /// <summary>
    /// 현재 점수를 0점으로 초기화한다.
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;

        Debug.Log("[3-11] 점수 초기화");
    }
}