using UnityEngine;

// 3-8 범인. 뒤돌아보는 시각은 Minigame3_8remake가 노래 시간으로 정해서 SetLook을 부른다.
public class EnemyWatch3_8 : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] private AudioClip clickSound;
    [Header("효과음")]
    [SerializeField] private AudioClip walkSound;
    [Header("스프라이트")]
    public Sprite normalSprite;   // 기본 모습
    public Sprite lookSprite;     // 뒤돌아본 모습

    [Header("시간 설정 (구버전 호환, 사용 안 함)")]
    public float changeTime = 2.4f;
    public float lookDuration = 0.6f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (sr != null && normalSprite != null)
            sr.sprite = normalSprite;
    }

    public void SetLook(bool look)
    {
        if (sr != null)
        {
            Sprite s = look ? lookSprite : normalSprite;
            if (s != null) sr.sprite = s;
        }

        AudioClip clip = look ? clickSound : walkSound;
        if (clip != null && GameRoot.Instance != null && GameRoot.Instance.Audio != null)
            GameRoot.Instance.Audio.PlaySfx(clip);
    }

    // 구버전 호환
    public void PlayWatchRoutine()
    {
    }
}