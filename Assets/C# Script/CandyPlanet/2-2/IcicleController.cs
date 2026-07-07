using UnityEngine;

public class IcicleController : MonoBehaviour
{
    private int beatCount = 0; // 꿈틀거린 횟수 카운트
    private bool isDropped = false;

    // RhythmAnimation의 Restart() 함수 끝부분에 아래 함수를 호출하도록 연결하거나,
    // 만약 코드 수정이 정 어렵다면 Update에서 감지하는 방식을 씁니다.

    // [대안] RhythmAnimation을 건드리지 않고 스스로 박자를 추적하는 방식
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