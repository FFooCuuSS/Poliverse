using UnityEngine;

// 3-5: 판정/적 제거는 Minigame_3_5_remake가 한다. 여기는 효과음만 담당.
public class ScopeShooter : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip hitSound;
    [Header("효과음")]
    [SerializeField] private AudioClip missSound;

    [Header("미니게임")]
    [SerializeField] private Minigame_3_5_remake minigame;

    private bool isDemoMode = false;

    public void SetDemoMode(bool isDemo)
    {
        isDemoMode = isDemo;
    }

    public void PlayHit() => PlaySound(hitSound);
    public void PlayMiss() => PlaySound(missSound);

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(clip);
    }
}