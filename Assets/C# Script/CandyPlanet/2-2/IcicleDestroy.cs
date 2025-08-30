using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcicleDestroy : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.collider.tag == "Floor")
        {
            Destroy(gameObject, 0.2f);
        }
    }
}
