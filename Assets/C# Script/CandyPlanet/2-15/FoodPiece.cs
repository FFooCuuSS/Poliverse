using UnityEngine;

public class FoodPiece : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("click");
            Collider2D biteZone = GameObject.FindGameObjectWithTag("BiteZone")
                                         .GetComponent<Collider2D>();

            if (biteZone != null)
            {
                // 조각과 BiteZone이 겹치는지 체크
                if (biteZone.IsTouching(GetComponent<Collider2D>()))
                {
                    Destroy(gameObject);
                    Debug.Log($"{gameObject.name} 삭제됨");
                }
            }
        }
    }
}
