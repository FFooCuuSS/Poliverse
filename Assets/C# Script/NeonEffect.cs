using UnityEngine;

public enum InteractionType
{
    Touch,
    Drag,
    Swipe
}

[RequireComponent(typeof(SpriteRenderer))]
public class NeonEffect : MonoBehaviour
{
    [Header("Interaction Type")]
    [SerializeField]
    private InteractionType interactionType;

    // 모든 오브젝트 공통.
    // 화면상 정확히 이 정도의 두께로 보이게 한다.
    private const float OutlineWidthPixels = 2f;

    private const float OutlineAlpha = 1f;

    private SpriteRenderer originalRenderer;

    private SpriteRenderer[] outlineRenderers;
    private Transform[] outlineTransforms;

    private Material outlineMaterial;
    private Camera targetCamera;


    private static readonly Vector2[] Directions =
    {
        new Vector2( 1f,  0f),
        new Vector2(-1f,  0f),
        new Vector2( 0f,  1f),
        new Vector2( 0f, -1f),

        new Vector2( 0.7071f,  0.7071f),
        new Vector2(-0.7071f,  0.7071f),
        new Vector2( 0.7071f, -0.7071f),
        new Vector2(-0.7071f, -0.7071f)
    };


    private void Awake()
    {
        originalRenderer =
            GetComponent<SpriteRenderer>();

        targetCamera =
            Camera.main;

        CreateOutlineMaterial();
        CreateOutlineRenderers();
    }


    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
                return;
        }

        SyncRenderer();
        UpdateScreenSpaceOutline();
    }


    private void CreateOutlineMaterial()
    {
        Shader shader =
            Shader.Find(
                "Custom/SpriteOutlineSilhouette"
            );

        if (shader == null)
        {
            Debug.LogError(
                "SpriteOutlineSilhouette Shader를 찾을 수 없습니다.",
                this
            );

            return;
        }

        outlineMaterial =
            new Material(shader);
    }


    private void CreateOutlineRenderers()
    {
        outlineRenderers =
            new SpriteRenderer[Directions.Length];

        outlineTransforms =
            new Transform[Directions.Length];


        for (int i = 0; i < Directions.Length; i++)
        {
            GameObject child =
                new GameObject(
                    $"Outline_{i}"
                );

            child.transform.SetParent(
                transform,
                false
            );

            SpriteRenderer renderer =
                child.AddComponent<SpriteRenderer>();

            renderer.sprite =
                originalRenderer.sprite;

            renderer.material =
                outlineMaterial;

            renderer.sortingLayerID =
                originalRenderer.sortingLayerID;

            // 원본 바로 뒤
            renderer.sortingOrder =
                originalRenderer.sortingOrder - 1;

            outlineRenderers[i] =
                renderer;

            outlineTransforms[i] =
                child.transform;
        }

        ApplyColor();
    }


    private void SyncRenderer()
    {
        Color outlineColor =
            GetInteractionColor();

        float alpha =
            originalRenderer.color.a *
            OutlineAlpha;


        for (int i = 0; i < outlineRenderers.Length; i++)
        {
            SpriteRenderer renderer =
                outlineRenderers[i];

            renderer.enabled =
                originalRenderer.enabled;

            renderer.sprite =
                originalRenderer.sprite;

            renderer.flipX =
                originalRenderer.flipX;

            renderer.flipY =
                originalRenderer.flipY;

            renderer.sortingLayerID =
                originalRenderer.sortingLayerID;

            renderer.sortingOrder =
                originalRenderer.sortingOrder - 1;

            renderer.color =
                new Color(
                    outlineColor.r,
                    outlineColor.g,
                    outlineColor.b,
                    alpha
                );
        }
    }


    private void UpdateScreenSpaceOutline()
    {
        Vector3 screenPosition =
            targetCamera.WorldToScreenPoint(
                transform.position
            );

        Vector3 originalWorld =
            targetCamera.ScreenToWorldPoint(
                screenPosition
            );


        for (int i = 0; i < Directions.Length; i++)
        {
            Vector2 direction =
                Directions[i];

            Vector3 offsetScreen =
                screenPosition +
                new Vector3(
                    direction.x * OutlineWidthPixels,
                    direction.y * OutlineWidthPixels,
                    0f
                );


            Vector3 offsetWorld =
                targetCamera.ScreenToWorldPoint(
                    offsetScreen
                );


            Vector3 worldDifference =
                offsetWorld -
                originalWorld;


            /*
             * 핵심.
             *
             * 원하는 건 "localPosition 0.02"가 아니라
             * "화면에서 정확히 2 pixel 이동"이다.
             *
             * InverseTransformVector가
             * 부모/자기 Scale과 Rotation을 역산해서
             * 필요한 localPosition으로 변환한다.
             */
            Vector3 localOffset =
                transform.InverseTransformVector(
                    worldDifference
                );


            outlineTransforms[i].localPosition =
                localOffset;
        }
    }


    private void ApplyColor()
    {
        Color color =
            GetInteractionColor();

        if (outlineRenderers == null)
            return;

        foreach (SpriteRenderer renderer
                 in outlineRenderers)
        {
            if (renderer == null)
                continue;

            renderer.color = color;
        }
    }


    private Color GetInteractionColor()
    {
        switch (interactionType)
        {
            case InteractionType.Touch:
                return new Color(
                    0.0f,
                    0.9f,
                    1.0f,
                    1.0f
                );

            case InteractionType.Drag:
                return new Color(
                    1.0f,
                    0.15f,
                    0.65f,
                    1.0f
                );

            case InteractionType.Swipe:
                return new Color(
                    1.0f,
                    0.65f,
                    0.0f,
                    1.0f
                );
        }

        return Color.white;
    }


    private void OnDestroy()
    {
        if (outlineMaterial != null)
        {
            Destroy(outlineMaterial);
        }
    }
}