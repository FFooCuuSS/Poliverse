using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasScalerNormalizer : MonoBehaviour
{
    [Header("Reference Resolution")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [Header("Screen Match")]
    [Range(0f, 1f)]
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        NormalizeAllCanvases();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        NormalizeAllCanvases();
    }

    private void NormalizeAllCanvases()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null)
                continue;

            // World Space UI는 화면 UI와 계산법이 다르므로 건드리지 않음.
            if (canvas.renderMode == RenderMode.WorldSpace)
                continue;

            // 중첩 Canvas는 부모 Canvas 기준을 따라가는 경우가 많으므로
            // Root Canvas만 통일한다.
            if (!canvas.isRootCanvas)
                continue;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();

            if (scaler == null)
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
            scaler.referencePixelsPerUnit = 100f;

            // Screen Space Canvas의 이상한 Transform Scale 방지.
            RectTransform rect = canvas.transform as RectTransform;

            if (rect != null)
            {
                rect.localScale = Vector3.one;
            }

            Debug.Log(
                $"[CanvasScalerNormalizer] Normalized: {canvas.name} / " +
                $"{canvas.renderMode} / {referenceResolution.x}x{referenceResolution.y}"
            );
        }
    }
}