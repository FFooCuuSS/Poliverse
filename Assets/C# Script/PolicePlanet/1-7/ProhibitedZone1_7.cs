using UnityEngine;

public class ProhibitedZone1_7 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            // SpriteRenderer는 금지 아이템의 부모에 존재
            SpriteRenderer sr = other.GetComponentInParent<SpriteRenderer>();
            if (sr != null)
            {
                ProhibitedItemManager1_7 manager = FindObjectOfType<ProhibitedItemManager1_7>();
                if (manager == null) return;

                int index = manager.prohibitedSprites.IndexOf(sr.sprite);
                if (index != -1)
                {
                    Debug.Log("금지 아이템이 존에 들어옴");
                    GameManager1_7.instance.IncreaseSuccessCount();


                    // 부모의 부모인 죄수 오브젝트에서 PrisonerController1_7 호출
                    Transform prisoner = sr.transform.parent?.parent;
                    if (prisoner != null)
                    {
                        PrisonerController1_7 controller = prisoner.GetComponent<PrisonerController1_7>();
                        if (controller != null)
                        {
                            controller.StartSuccessEscape();
                            Debug.Log("죄수 보내기");
                        }
                    }
                    //Destroy(sr.gameObject);
                }
            }
        }
    }
}
