using UnityEngine;

public class EdgeWalls : MonoBehaviour
{
    public SpriteRenderer wallL;
    public SpriteRenderer wallR;
    public SpriteRenderer ceiling;

    void Start()
    {
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;

        // 왼쪽 벽: 스프라이트 왼쪽 끝을 화면 왼쪽 끝에 맞춤
        float dx = (c.x - halfW) - wallL.bounds.min.x;
        wallL.transform.position += new Vector3(dx, 0, 0);

        // 오른쪽 벽: 스프라이트 오른쪽 끝을 화면 오른쪽 끝에 맞춤
        dx = (c.x + halfW) - wallR.bounds.max.x;
        wallR.transform.position += new Vector3(dx, 0, 0);

        // 천장: 스프라이트 위쪽 끝을 화면 위쪽 끝에 맞춤
        float dy = (c.y + halfH) - ceiling.bounds.max.y;
        ceiling.transform.position += new Vector3(0, dy, 0);
    }
}