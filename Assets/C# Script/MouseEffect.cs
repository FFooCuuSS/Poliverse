using UnityEngine;

public class MouseEffect : MonoBehaviour
{
    private Animator anim;
    private Camera cam;

    void Awake()
    {
        anim = GetComponent<Animator>();
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;

            transform.position = pos;

            anim.SetTrigger("Click");
        }
    }
}