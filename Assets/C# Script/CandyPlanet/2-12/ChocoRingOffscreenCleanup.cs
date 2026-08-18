using UnityEngine;

/// <summary>
/// 화면(카메라 뷰) 하단 바깥쪽에 배치하는 오브젝트에 부착.
/// BoxCollider2D(Is Trigger 체크)를 같이 넣어주세요.
///
/// Show 단계에서 낙하 중인 프리뷰 초코링(ChocoRingFaller가 붙은 ChocoRingMarker)이
/// 이 트리거에 닿으면 자동으로 삭제됩니다. 캐치 단계(Input) 링은 ChocoRingFaller가
/// 붙어있지 않으므로 여기 닿아도 무시됩니다 (그쪽은 그릇에서 캐치/미스로 별도 처리됨).
///
/// 배치 팁: 실제 카메라(Main Camera)의 화면 하단보다 살짝 더 아래쪽에 폭 넓은
/// BoxCollider2D를 놓으면, 카메라가 Show 단계 위치에 있을 때 기준으로
/// "화면 밖으로 완전히 나간 시점"에 정확히 삭제됩니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ChocoRingOffscreenCleanup : MonoBehaviour
{
    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var marker = other.GetComponent<ChocoRingMarker>();
        if (marker == null) return;

        // 프리뷰 낙하 중인 링만 대상으로 삼음 (캐치용 링은 ChocoRingFaller가 없어서 무시됨)
        if (other.GetComponent<ChocoRingFaller>() == null) return;

        Destroy(other.gameObject);
    }
}