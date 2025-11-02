using UnityEngine;

public class Throwable2D : MonoBehaviour
{
    private Rigidbody2D rb;


    public StageManager3_11 stageManager3_11;

    public float minForce = 7f;     // 목표 방향으로 가해질 힘 최소
    public float maxForce = 12f;    // 최대
    public float minUpBoost = 2f;   // 살짝 위로 들어올리는 추가 y 가속
    public float maxUpBoost = 5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }
    private void Update()
    {
        if (transform.position.y < -15) 
        {
            Destroy(this.gameObject);
        }
    }

    // 스포너가 호출: 시작위치, 목표(혹은 중심)월드좌표 전달
    public void ThrowFromTo(Vector3 startPos, Vector3 targetPos)
    {
        transform.position = startPos;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 목표 방향
        Vector2 dir = (targetPos - startPos).normalized;
        float forceMag = Random.Range(minForce, maxForce);
        float upBoost = Random.Range(minUpBoost, maxUpBoost);

        // 위로 약간 휘는 포물선 느낌을 주려면 y에 추가
        Vector2 force = (dir * forceMag) + new Vector2(0f, upBoost);
        rb.AddForce(force, ForceMode2D.Impulse);

        
    }
    private void OnMouseDown()
    {
        
        if (this.name=="Bomb(Clone)")
        {
            stageManager3_11.bombCnt++;
            Debug.Log(this.name);
            Debug.Log(stageManager3_11.bombCnt);
        }
        else if (this.name =="Paper(Clone)")
        {
            stageManager3_11.paperCnt++;
            Debug.Log(this.name);
            Debug.Log(stageManager3_11.paperCnt);
        }
        if (stageManager3_11.paperCnt > 10) Debug.Log("Clear");
        else if (stageManager3_11.bombCnt > 3) Debug.Log("Fail");
        Destroy(this.gameObject);
    }
}
