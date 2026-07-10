using UnityEngine;

public class IcicleController : MonoBehaviour
{
    private int beatCount = 0; // 꿈틀거린 횟수 카운트
    private bool isDropped = false;

    [SerializeField] private float roundTripTime = 0.5f; // RhythmAnimation과 동일하게 설정
    private float timer = 0f;

    void Update()
    {
        if (isDropped) return;

        timer += Time.deltaTime;
        if (timer >= roundTripTime)
        {
            timer = 0f;
            beatCount++;

            Debug.Log($"꿈틀! 현재 횟수: {beatCount}");

            if (beatCount >= 3)
            {
                DropIcicle();
            }
        }
    }

    private void DropIcicle()
    {
        isDropped = true;
        Debug.Log("3번 꿈틀 완료, 고드름 낙하!");
        // 여기에 고드름이 아래로 떨어지는 로직 (DOTween 이동 등) 실행
    }
}