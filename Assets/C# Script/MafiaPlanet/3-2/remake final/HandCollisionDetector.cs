using UnityEngine;

public class HandCollisionDetector : MonoBehaviour
{
    // 부모 HandController
    private HandController handController;


    private void Awake()
    {
        // 부모에서 HandController 찾기
        handController =
            GetComponentInParent<HandController>();


        if (handController == null)
        {
            Debug.LogError(
                "[HandCollisionDetector] " +
                "HandController를 찾을 수 없습니다."
            );
        }
    }


    private void OnTriggerEnter2D(
        Collider2D other)
    {
        // Bag이 아니면 무시
        if (!other.CompareTag("Bag"))
        {
            return;
        }


        if (handController == null)
        {
            return;
        }


        // 현재 충돌한 Bag 전달
        handController.EnterBag(
            other.gameObject
        );
    }


    private void OnTriggerExit2D(
        Collider2D other)
    {
        // Bag이 아니면 무시
        if (!other.CompareTag("Bag"))
        {
            return;
        }


        if (handController == null)
        {
            return;
        }


        // Bag 충돌 종료 전달
        handController.ExitBag(
            other.gameObject
        );
    }
}