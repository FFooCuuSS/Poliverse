using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddInitialForce : MonoBehaviour
{
    [SerializeField] private CircleCollider2D jelly;

    private Rigidbody2D rb;
    public float power;

    private void Start()
    {
        rb = jelly.GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.left * power);
    }
}
