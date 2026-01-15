using UnityEngine;

public class timetest : MonoBehaviour
{
    float timeAcc = 0f;
    public GameObject hand;

    float[] targets = { 2f, 3f, 4f, 5f };
    int idx = 0;

    void Update()
    {
        timeAcc += Time.deltaTime;

        if (idx < targets.Length && timeAcc >= targets[idx])
        {
            Debug.Log("current time: " + timeAcc + " hand pos : " + hand.transform.position.x);
            idx++;
        }
    }
}
