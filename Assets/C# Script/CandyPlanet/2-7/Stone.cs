using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public Transform jelly;
    [SerializeField] private Minigame_2_7 minigame_2_7;

    public float jumpHeight = 1f;
    public float speed = 10f;

    float originY;
    bool jumping = false;

    Vector2 startPos;
    public float slideThreshold = 100f; // 위로 슬라이드 인식 거리

    void Start()
    {
        originY = transform.position.y;
        minigame_2_7 = GetComponentInParent<Minigame_2_7>();
    }

    void Update()
    {
        if (jelly != null)
        {
            transform.position = new Vector3(
                jelly.position.x,
                transform.position.y,
                0
            );
        }

        // 누르기 시작
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }

        // 누른 상태에서 위로 드래그 체크
        if (Input.GetMouseButton(0) && !jumping)
        {
            float deltaY = Input.mousePosition.y - startPos.y;

            if (deltaY > slideThreshold)
            {
                StartCoroutine(Slide());
            }
        }
    }

    IEnumerator Slide()
    {
        jumping = true;
        Debug.Log("슬라이드 함");
        minigame_2_7.OnPlayerInput("Slide");

        float target = originY + jumpHeight;

        while (transform.position.y < target)
        {
            transform.position += Vector3.up * speed * Time.deltaTime;
            yield return null;
        }

        while (transform.position.y > originY)
        {
            transform.position -= Vector3.up * speed * Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, originY, 0);
        jumping = false;
    }

    public void SetJelly(Transform newJelly)
    {
        jelly = newJelly;
    }
}