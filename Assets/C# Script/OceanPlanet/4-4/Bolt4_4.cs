using UnityEngine;


public class Bolt4_4 : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 4f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.left * (speed * Time.deltaTime);
    }
}