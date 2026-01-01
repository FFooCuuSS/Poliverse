using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldCheck1_7 : MonoBehaviour
{
    public GameObject holdUIPrefab;      // Inspector에서 Prefab 연결
    public float shrinkSpeed = 1.2f;
    public int maxHoldCount = 4;

    private int currentHoldCount = 0;
    private GameObject currentHoldUI;
    private Transform targetCircle;
    private Transform shrinkingCircle;
    private bool isHolding = false;

    // Hold 시작
    public void OnHoldStart(Transform prisoner)
    {
        if (currentHoldCount >= maxHoldCount) return; // 최대 횟수 체크

        // 이전 UI 제거
        if (currentHoldUI != null)
        {
            Destroy(currentHoldUI);
            currentHoldUI = null;
        }

        // 새로운 UI 생성
        currentHoldUI = Instantiate(holdUIPrefab);

        // 화면 맨 위로 보이게 하기
        SpriteRenderer[] renderers = currentHoldUI.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.sortingLayerName = "UI"; // UI용 Sorting Layer
            sr.sortingOrder = 200;      // 높은 숫자 -> 맨 위
        }

        // 죄수 주변 랜덤 위치
        currentHoldUI.transform.position = prisoner.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(1.0f, 1.5f),
            0f
        );

        // 원들 찾기
        targetCircle = currentHoldUI.transform.Find("TargetCircle");
        shrinkingCircle = currentHoldUI.transform.Find("ShrinkingCircle");

        // 항상 새로 생성될 때 크기 초기화
        if (shrinkingCircle != null)
            shrinkingCircle.localScale = Vector3.one * 1.5f; // 줄어드는 원
        if (targetCircle != null)
            targetCircle.localScale = Vector3.one; // 기준 원

        isHolding = true;
    }


    void Update()
    {
        if (isHolding && shrinkingCircle != null)
        {
            // 줄어들기
            shrinkingCircle.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;

            if (shrinkingCircle.localScale.x <= 0f)
            {
                // 줄어든 원 삭제
                Destroy(currentHoldUI);
                currentHoldUI = null;
                isHolding = false;

                // 성공 카운트 증가
                currentHoldCount++;

                // Minigame에 성공 알림
                FindObjectOfType<Minigame_1_7>().OnHoldSuccess();
            }
        }
    }

    public bool CanHold()
    {
        return currentHoldCount < maxHoldCount && !isHolding;
    }

    public void ResetHolds()
    {
        currentHoldCount = 0;
        isHolding = false;
        if (currentHoldUI != null)
            Destroy(currentHoldUI);
    }

    public void HideHoldUI()
    {
        if (currentHoldUI != null)
        {
            Destroy(currentHoldUI);
            currentHoldUI = null;
        }
    }
}


