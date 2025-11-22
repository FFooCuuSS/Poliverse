using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Mirror_3_15 : MonoBehaviour
{
    public float duration;   // 이동 시간

    void Update()
    {
        float x = transform.position.x;
        
        if (x >= -1f && x <= 1f)
        {
            Vector3 p = transform.position;
            p.x = 0f;
            transform.position = p;
        }
    }

    public void SummonTo()
    {
        Vector3 target = new Vector3(4f, 0f, transform.position.z);

        transform.DOMove(target, 3f)
         .SetEase(Ease.InOutQuad);
    }

    public void SetChildActive()
    {
        Invoke("delayedActive", 3f);
    }
    
    void delayedActive()
    {
        Transform child0 = transform.GetChild(0);
        child0.gameObject.SetActive(true);
    }
}
