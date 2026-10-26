using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotMove_4_2 : MonoBehaviour
{
    public float moveInterval = 1f;
    public float moveDistance = 2f;

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveInterval);

            transform.position += Vector3.left * moveDistance;
        }
    }
}
