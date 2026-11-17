using UnityEngine;

public class BubbleUp : MonoBehaviour
{
    private Minigame_4_9 minigame;

    private float moveSpeed;
    private float destroyY;
    private bool isResolved;

    public void Initialize(
        Minigame_4_9 minigame,
        float targetLineY,
        float travelDuration,
        float destroyY)
    {
        this.minigame = minigame;
        this.destroyY = destroyY;

        float distance = targetLineY - transform.position.y;

        moveSpeed =
            distance / Mathf.Max(0.01f, travelDuration);
    }

    private void Update()
    {
        if (isResolved)
        {
            return;
        }

        transform.position +=
            Vector3.up * moveSpeed * Time.deltaTime;

        if (transform.position.y >= destroyY)
        {
            Miss();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DartProjectile dart =
            other.GetComponent<DartProjectile>();

        if (dart == null)
        {
            return;
        }

        Hit();
        dart.DestroyDart();
    }

    public void Hit()
    {
        if (isResolved)
        {
            return;
        }

        isResolved = true;
        minigame.OnBubbleHit(this);
    }

    private void Miss()
    {
        if (isResolved)
        {
            return;
        }

        isResolved = true;
        minigame.OnBubbleMiss(this);
    }
}
