using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleDestroy : MonoBehaviour
{
    [SerializeField] private Sprite brokenIcicleSprite;
    private SpriteRenderer spriteRenderer;
    private bool isBroken = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.tag == "Floor" && !isBroken)
        {
            isBroken = true;
            StartCoroutine(BreakAndDestroy());
        }
    }

    private IEnumerator BreakAndDestroy()
    {
        // 스프라이트를 부서진 고드름으로 변경
        spriteRenderer.sprite = brokenIcicleSprite;

        // 0.3초 대기 후 삭제
        yield return new WaitForSeconds(0.3f);

        Destroy(gameObject);
    }
}
