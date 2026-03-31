using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float stepDistance = 1.5f;

    private void OnEnable()
    {
        BeatManager.Instance.OnBeat += MoveStep;
    }

    private void OnDisable()
    {
        BeatManager.Instance.OnBeat -= MoveStep;
    }

    void MoveStep()
    {
        transform.position += Vector3.right * stepDistance;
    }

    private void Update()
    {
        if (transform.position.x > 12f)
        {
            Destroy(gameObject);
        }
    }
}