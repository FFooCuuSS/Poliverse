using UnityEngine;

public class HandMover : MonoBehaviour
{
    [SerializeField] private Minigame_3_4_Remake game; // 인스펙터로 연결
    public bool start = false;
    public bool checkIsSuspicious = false;
    public int clickCount = 0;
    public int suspiciousClickCount = 0;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)&&clickCount<5)
        {
            game.SubmitInput();
            Debug.Log("CLICK DETECTED");
            clickCount++;
            if(checkIsSuspicious) suspiciousClickCount++;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<CardColor>() != null)
        {
            CardColor checkCardSuspicious = collision.GetComponent<CardColor>();
            Debug.Log("STAYING ON CARD: " + collision.gameObject.name);
            if (checkCardSuspicious != null)
            {
                if (checkCardSuspicious.isTrapCard)
                {
                    
                    Debug.Log("IS THIS A TRAP CARD? " + checkCardSuspicious.isTrapCard);
                    checkIsSuspicious = true;

                }
                else checkIsSuspicious = false;
            }
        }
    }
}