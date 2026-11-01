using UnityEngine;

// '여기를 눌렀다'를 보여주는 터치 연출 (현재 3-6 마이크 버튼에서 사용).
// Assets/Resources/DemoTouch.png (Texture Type = Sprite) 를 불러와서 누른 위치에 띄운다.
//
//  DemoTouchFx.Tap(위치, 부모)            : 톡 튀어나왔다가 사라짐 (탭/클릭/사격)
//  var m = DemoTouchFx.Hold(위치, 부모);  : 떼기 전까지 유지 (홀드)
//  m.Release();                           : 홀드 끝 → 사라짐
//  DemoTouchFx.ToWorld(transform)         : UI(RectTransform) 위치도 월드 좌표로 바꿔줌
public static class DemoTouchFx
{
    private const string SpritePath = "DemoTouch";
    private const int SortingOrder = 5000;

    private static Sprite cached;

    private static Sprite GetSprite()
    {
        if (cached == null)
        {
            cached = Resources.Load<Sprite>(SpritePath);
            if (cached == null)
                Debug.LogWarning("[DemoTouchFx] Resources/DemoTouch 스프라이트가 없음 (Texture Type을 Sprite로 설정했는지 확인)");
        }
        return cached;
    }

    public static DemoTouchMarker Tap(Vector3 worldPos, Transform parent = null, float size = 1.6f, float life = 0.35f)
    {
        var m = Create(worldPos, parent, size);
        if (m != null) m.Init(false, life);
        return m;
    }

    public static DemoTouchMarker Hold(Vector3 worldPos, Transform parent = null, float size = 1.6f)
    {
        var m = Create(worldPos, parent, size);
        if (m != null) m.Init(true, 0f);
        return m;
    }

    private static DemoTouchMarker Create(Vector3 worldPos, Transform parent, float size)
    {
        Sprite sp = GetSprite();
        if (sp == null) return null;

        var go = new GameObject("DemoTouch");
        if (parent != null) go.transform.SetParent(parent, true);

        worldPos.z = 0f;
        go.transform.position = worldPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp;
        sr.sortingOrder = SortingOrder;

        // 스프라이트 가로 크기를 size(월드 단위)에 맞춘다
        float w = sp.bounds.size.x;
        float baseScale = w > 0f ? size / w : 1f;

        // 부모 스케일 보정 (부모가 커도 화면에서 같은 크기)
        if (parent != null && Mathf.Abs(parent.lossyScale.x) > 0.0001f)
            baseScale /= Mathf.Abs(parent.lossyScale.x);

        var m = go.AddComponent<DemoTouchMarker>();
        m.SetBaseScale(baseScale);
        return m;
    }

    // UI 버튼 위치도 화면 위 같은 자리의 월드 좌표로 변환
    public static Vector3 ToWorld(Transform t)
    {
        if (t == null) return Vector3.zero;

        if (t is RectTransform)
        {
            Canvas canvas = t.GetComponentInParent<Canvas>();
            Camera main = Camera.main;

            if (canvas != null && main != null && canvas.renderMode != RenderMode.WorldSpace)
            {
                Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, t.position);
                Vector3 w = main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(main.transform.position.z)));
                w.z = 0f;
                return w;
            }
        }

        Vector3 p = t.position;
        p.z = 0f;
        return p;
    }
}

public class DemoTouchMarker : MonoBehaviour
{
    private const float PopTime = 0.12f;
    private const float FadeTime = 0.2f;

    private SpriteRenderer sr;
    private float baseScale = 1f;
    private bool hold;
    private float life;
    private float t;
    private bool releasing;
    private float releaseT;

    public void SetBaseScale(float s) => baseScale = s;

    public void Init(bool isHold, float lifeTime)
    {
        sr = GetComponent<SpriteRenderer>();
        hold = isHold;
        life = Mathf.Max(lifeTime, PopTime + FadeTime);
        t = 0f;
        Apply(0.7f, 1f);
    }

    private bool releaseRequested;

    // 팝이 끝난 뒤에 사라지기 시작 (짧은 탭도 팝은 보이게)
    public void Release()
    {
        if (this == null) return;
        releaseRequested = true;
    }

    private void Update()
    {
        t += Time.deltaTime;

        // 팝: 0.7 → 1.1 → 1.0
        float scale;
        if (t < PopTime)
        {
            float k = t / PopTime;
            scale = Mathf.Lerp(0.7f, 1.1f, k);
        }
        else if (t < PopTime * 2f)
        {
            float k = (t - PopTime) / PopTime;
            scale = Mathf.Lerp(1.1f, 1f, k);
        }
        else
        {
            // 홀드 중에는 살짝 숨쉬듯
            scale = hold && !releasing ? 1f + 0.04f * Mathf.Sin((t - PopTime * 2f) * 12f) : 1f;
        }

        float alpha = 1f;

        if (!hold && t > life - FadeTime)
            releasing = true;

        if (releaseRequested && !releasing && t >= PopTime * 2f)
        {
            releasing = true;
            releaseT = 0f;
        }

        if (releasing)
        {
            releaseT += Time.deltaTime;
            float k = Mathf.Clamp01(releaseT / FadeTime);
            alpha = 1f - k;
            scale *= Mathf.Lerp(1f, 1.15f, k);

            if (k >= 1f)
            {
                Destroy(gameObject);
                return;
            }
        }

        Apply(scale, alpha);
    }

    private void Apply(float scale, float alpha)
    {
        float s = baseScale * scale;
        transform.localScale = new Vector3(s, s, 1f);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}