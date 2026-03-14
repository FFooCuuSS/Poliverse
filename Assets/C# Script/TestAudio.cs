using UnityEngine;

public class TestAudio : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private float delaySeconds = 0f; // 몇 초 뒤 재생

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayDelayed(delaySeconds);
        }
        else
        {
            Debug.LogWarning("AudioSource 또는 Clip이 없습니다.");
        }
    }
}