using UnityEngine;

public class PlayerLimit_3_14 : MonoBehaviour
{
    void Update()
    {
        Vector3 pos = transform.position;

        if (pos.y > 3.5f)
        {
            pos.y = 3.5f;
        }
        else if (pos.y < -3.5f)
        {
            pos.y = -3.5f;
        }

        transform.position = pos;
    }
}