using System.Collections;
using UnityEngine;

public class TemperatureController : MonoBehaviour
{
    [SerializeField] private int[] roundCounts;
    private int currentRound = 0;

    [Header("온도계")]
    [SerializeField] private GameObject gauge;

    [SerializeField] private float moveAmount = 10f;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float interval = 0.3f;

    private Vector3 startPos;

    private int playerSwipeCount = 0;
    private bool playerTurn = false;

    private void Start()
    {
        startPos = gauge.transform.localPosition;

        StartCoroutine(SystemTurn());
    }

    private int CurrentDownCount
    {
        get
        {
            return roundCounts[currentRound];
        }
    }
    IEnumerator SystemTurn()
    {
        playerTurn = false;

        // 시스템이 지정 횟수만큼 내림
        for (int i = 0; i < CurrentDownCount; i++)
        {
            yield return StartCoroutine(MoveDown());
            yield return new WaitForSeconds(interval);
        }

        Debug.Log("플레이어 차례");

        playerSwipeCount = 0;
        playerTurn = true;
    }

    IEnumerator MoveDown()
    {
        Vector3 target = startPos + Vector3.down * moveAmount;

        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            gauge.transform.localPosition =
               Vector3.Lerp(startPos, target, t / duration);

            yield return null;
        }

        t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;

            gauge.transform.localPosition =
                Vector3.Lerp(target, startPos, t / duration);

            yield return null;
        }

        gauge.transform.localPosition = startPos;
    }

    // 플레이어 스와이프 시 호출
    public void OnSwipe()
    {
        if (!playerTurn) return;

        playerSwipeCount++;

        Debug.Log($"스와이프 : {playerSwipeCount}");

        // 성공
        if (playerSwipeCount >= currentRound) 
        {
            playerTurn = false;

            Debug.Log("클리어!");
        }
    }
}