using UnityEngine;

public class BackgroundTouchHandler : MonoBehaviour
{
    [SerializeField] private Minigame_1_9 minigame;

    [Header("Input Cooldown")]
    [SerializeField] private float inputCooldown = 0.15f;

    private float lastInputTime = -999f;

    private void Update()
    {
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
                minigame?.SubmitPlayerInput("Input");
            }
        }
    }
}