using UnityEngine;

public class Jelly : MonoBehaviour
{
    public JellyType data;

    Rigidbody2D rb;
    float currentX;
    public float destroyX = 13f;
    [SerializeField] private JellySpawner spawner;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentX = transform.position.x;

        rb.velocity = new Vector2(data.moveSpeed, data.bounceForce);
    }

    public void Init(JellySpawner sp)
    {
        spawner = sp;
    }

    private void Update()
    {
        if (transform.position.x > destroyX)
        {
            Debug.Log("일정 x 좌표 넘어감");
            spawner.Spawn();
            Debug.Log("다음 젤리 스폰");
            Destroy(gameObject); 
            Debug.Log("젤리 삭제");

        }

    }

    void FixedUpdate()
    {
        currentX += data.moveSpeed * Time.fixedDeltaTime;

        float wave = Mathf.Sin(Time.time * data.waveSpeed) * data.waveAmount;

        transform.position = new Vector3(
            currentX + wave,
            transform.position.y,
            0
        );
    }
}