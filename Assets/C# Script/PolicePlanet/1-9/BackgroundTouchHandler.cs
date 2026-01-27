using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTouchHandler : MonoBehaviour
{
    [SerializeField] private Minigame_1_9 minigame;
    [SerializeField] private Rope rope;
    [SerializeField] private Vector3 stretchOffset = new Vector3(2f, 0, 0);
    [SerializeField] private float stretchDuration = 0.3f;

    //private bool hasTouched = false;

    void Update()
    {
        //if (hasTouched) return;

        // PC 마우스 왼쪽 버튼 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            // 배경 콜라이더가 2D일 경우
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos2D);
            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                OnBackgroundTouch();
                //hasTouched = true; // 한 번만 처리하고 싶으면 true
            }
        }
    }

    public void OnBackgroundTouch()
    {
        minigame?.OnScreenTouch();
        rope?.PlayStretch(stretchOffset);
    }
}
