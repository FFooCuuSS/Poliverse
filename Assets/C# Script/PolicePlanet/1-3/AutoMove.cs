using UnityEngine;

public class AutoMove : MonoBehaviour
{
    public float speed = 0.5f;

    [Header("Song Sync")]
    [Tooltip("켜면 deltaTime 누적 대신 RhythmManager 노래 시간으로 위치를 계산한다.\n" +
             "노래 시작 전(프리팹 준비 중)에는 움직이지 않는다.")]
    [SerializeField] private bool syncToSong = false;

    [SerializeField] private bool debugLog = false;

    private RhythmManager rhythm;
    private Vector3 startLocalPos;
    private bool started;

    private void Awake()
    {
        startLocalPos = transform.localPosition;
    }

    private void Update()
    {
        if (!syncToSong)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            return;
        }

        if (rhythm == null)
            rhythm = FindObjectOfType<RhythmManager>();

        if (rhythm == null || !rhythm.IsRunning)
            return;

        double t = rhythm.SongTime;
        if (t < 0) t = 0;

        if (!started)
        {
            started = true;
            if (debugLog)
                Debug.Log($"[AutoMove] {name} song 동기화 시작 song={t:F3}");
        }

        // Translate(Vector2.right)와 같은 방향(로컬 축) 유지
        transform.localPosition =
            startLocalPos + transform.localRotation * Vector3.right * (float)(speed * t);
    }
}