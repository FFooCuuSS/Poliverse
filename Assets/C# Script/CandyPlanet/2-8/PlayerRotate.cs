using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float maxAngle = 45f;
    [SerializeField] private float rotatePower = 5f;

    private Rigidbody2D rb;
    private Minigame_2_8 minigame_2_8;
    public GameObject stage_2_8;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        minigame_2_8 = stage_2_8.GetComponent<Minigame_2_8>();

        rb.AddTorque(-0.3f, ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 clickPos = Input.mousePosition;
            float centerX = Screen.width / 2f;

            if (clickPos.x < centerX)
            {
                RotateRight();
            }
            else
            {
                RotateLeft();
            }
        }

        CheckFailAngle();
    }

    private void RotateLeft()
    {
        rb.AddTorque(rotatePower, ForceMode2D.Impulse);
    }

    private void RotateRight()
    {
        rb.AddTorque(-rotatePower, ForceMode2D.Impulse);
    }

    private void CheckFailAngle()
    {
        float curAngle = transform.eulerAngles.z;
        if (curAngle > 180f) curAngle -= 360f;
        if(Mathf.Abs(curAngle) > maxAngle)
        {
            minigame_2_8.Failure();
        }
    }
 }
