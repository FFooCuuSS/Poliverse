using UnityEngine;

// 3-4: 클릭/판정은 Minigame_3_4_Remake가 직접 처리한다.
// 이 스크립트는 프리팹 연결 유지용으로만 남겨둠 (매 프레임 로그 제거).
public class HandMover : MonoBehaviour
{
    [SerializeField] private Minigame_3_4_Remake game;
    public bool start = false;
    public bool checkIsSuspicious = false;
    public int clickCount = 0;
    public int suspiciousClickCount = 0;

    private void OnTriggerStay2D(Collider2D collision)
    {
        CardColor card = collision.GetComponent<CardColor>();

        if (card != null)
            checkIsSuspicious = card.isTrapCard;
    }
}