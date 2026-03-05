using UnityEngine;

/// <summary>
/// hand에 붙이는 스크립트.
/// - hand가 닿아있는 카드(Trigger)를 기억했다가
/// - 좌클릭 시 Minigame에 "이 카드 클릭했음"을 전달
/// </summary>
public class ChooseCardRemake : MonoBehaviour
{
    [Header("Minigame Ref")]
    [SerializeField] private Minigame_3_4_Remake minigame;

    [Header("Card Layer (optional)")]
    [SerializeField] private LayerMask cardLayer;

    // 현재 hand와 닿아있는 카드
    private GameObject currentCard;

    public bool start = false; // Minigame에서 true로 켜면 입력 활성

    private void Update()
    {
        if (start && Input.GetMouseButtonDown(0)) Debug.Log("CLICK DETECTED");
        if (!start) return;

        // 좌클릭
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("CLICK, currentCard = " + (currentCard ? currentCard.name : "NULL"));
            // 현재 hand가 닿아있는 카드가 있어야 "선택" 가능
            if (currentCard == null) return;

            // 미니게임에 전달 (접수 성공하면 true)
            //bool accepted = minigame != null && minigame.TrySubmitByClick(currentCard);

            // accepted가 false면(입력창 닫힘/판정 대기/횟수초과 등) 아무것도 안 함
        }
    }

    // hand가 카드에 닿았을 때
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<CardColor>() != null)
        {
            currentCard = collision.gameObject;
        }
    }

    
}