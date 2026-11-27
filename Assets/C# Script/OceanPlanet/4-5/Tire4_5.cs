using UnityEngine;

public class Tire4_5 : MonoBehaviour
{
    [Header("Ã¹ ¹øÂ° Æ¨±è")]
    public float hop1Duration = 0.5f;
    public float hop1Arc = 1f;

    [Header("µÎ ¹øÂ° Æ¨±è")]
    public float hop2Duration = 0.5f;
    public float hop2Arc = 1f;

    [Header("ÂøÁö ÈÄ ÀÜ¹Ù¿î½º")]
    public float idleHeight = 0.3f;
    public float idlePeriod = 0.35f;
    [Range(0f, 1f)] public float idleDecay = 0.5f;

    [Header("È¸Àü")]
    public float spinSpeed = 0f;

    public int HopCount { get; private set; }

    private bool hopping;
    private Vector3 from;
    private Vector3 to;
    private Vector3 restPos;
    private double startDsp;
    private double landDsp;
    private float duration;
    private float arc;

    public void NextHop(Vector3 target)
    {
        HopCount++;
        duration = HopCount == 1 ? hop1Duration : hop2Duration;
        arc = HopCount == 1 ? hop1Arc : hop2Arc;
        from = transform.position;
        to = target;
        startDsp = AudioSettings.dspTime;
        hopping = true;
    }

    private void Update()
    {
        if (HopCount == 0) return;

        if (hopping)
        {
            float u = Mathf.Clamp01((float)(AudioSettings.dspTime - startDsp) / duration);
            Vector3 p = Vector3.Lerp(from, to, u);
            p.y += 4f * arc * u * (1f - u);
            transform.position = p;
            transform.Rotate(0f, 0f, -spinSpeed * Time.deltaTime);

            if (u >= 1f)
            {
                hopping = false;
                restPos = to;
                landDsp = AudioSettings.dspTime;
            }
            return;
        }

        float t = (float)(AudioSettings.dspTime - landDsp);
        int n = Mathf.FloorToInt(t / idlePeriod);
        float v = t / idlePeriod - n;
        float h = idleHeight * Mathf.Pow(idleDecay, n);

        Vector3 rest = restPos;
        rest.y += 4f * h * v * (1f - v);
        transform.position = rest;
    }
}