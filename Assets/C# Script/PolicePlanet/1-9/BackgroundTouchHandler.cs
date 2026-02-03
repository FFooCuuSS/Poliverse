using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTouchHandler : MonoBehaviour
{
    [SerializeField] private Minigame_1_9 minigame;
    [SerializeField] private Rope rope;
    [SerializeField] private Vector3 stretchOffset = new Vector3(2f, 0, 0);
    [SerializeField] private float stretchDuration = 0.3f;

    [Header("Input Cooldown")]
    [SerializeField] private float inputCooldown = 0.5f;

    private float lastInputTime = -999f;

    void Update()
    {
        // ÄðÅ¸ÀÓ Ã¼Å©
        if (Time.time - lastInputTime < inputCooldown)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            Collider2D hitCollider = Physics2D.OverlapPoint(mousePos2D);
            if (hitCollider != null && hitCollider.gameObject == gameObject)
            {
                lastInputTime = Time.time;
                OnBackgroundTouch();
            }
        }
    }

    public void OnBackgroundTouch()
    {
        minigame?.OnScreenTouch();
        rope?.PlayStretch(stretchOffset);
    }
}
