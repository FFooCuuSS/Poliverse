using UnityEngine;

public class DartProjectile : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 8f;

    [SerializeField]
    private float directionX = 1f;

    [SerializeField]
    private float destroyX = 10f;

    public void SetDirection(float directionX, float moveSpeed)
    {
        this.directionX = Mathf.Sign(directionX);
        this.moveSpeed = moveSpeed;
    }

    private void Update()
    {
        transform.position +=
            Vector3.right * directionX * moveSpeed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x) >= destroyX)
        {
            Destroy(gameObject);
        }
    }

    public void DestroyDart()
    {
        Destroy(gameObject);
    }
}
