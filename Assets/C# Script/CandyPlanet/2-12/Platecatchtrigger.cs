using UnityEngine;

/// <summary>
/// 접시 오브젝트에 Collider2D(Is Trigger 체크)와 함께 부착.
/// 초코링(ChocoRingMarker)과 겹치면 Minigame_2_12에 캐치 시도를 알린다.
/// 레인 일치 여부 판단은 Minigame_2_12.HandleCatchAttempt에서 처리.
/// </summary>
[RequireComponent(typeof(PlateController))]
public class PlateCatchTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var marker = other.GetComponent<ChocoRingMarker>();
        if (marker == null || marker.caught) return;

        Minigame_2_12.Instance?.HandleCatchAttempt(other.gameObject, marker);
    }
}