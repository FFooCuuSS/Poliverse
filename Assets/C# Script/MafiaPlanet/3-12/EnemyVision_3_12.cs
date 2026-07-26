using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EnemyVision_3_12 : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("적의 방향을 담당하는 루트. 비워두면 부모 사용")]
    [SerializeField] private Transform eye;

    [Header("FOV")]
    [SerializeField] private float viewAngleDeg = 60f;
    [SerializeField] private float viewDistance = 6f;
    [SerializeField, Range(12, 180)] private int rays = 60;

    [Header("Raycast")]
    [Tooltip("Wall과 Player 레이어만 포함")]
    [SerializeField] private LayerMask visionMask;

    [Header("Rendering")]
    [SerializeField]
    private Color fovColor =
        new Color(1f, 0f, 0f, 0.35f);

    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 200;

    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private Material runtimeMaterial;

    private void Awake()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh
        {
            name = "EnemyVisionMesh"
        };

        meshFilter.sharedMesh = mesh;

        if (meshRenderer.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            meshRenderer.sharedMaterial = new Material(shader);
        }

        runtimeMaterial = meshRenderer.material;

        if (runtimeMaterial.HasProperty("_Color"))
            runtimeMaterial.color = fovColor;
        else if (runtimeMaterial.HasProperty("_BaseColor"))
            runtimeMaterial.SetColor("_BaseColor", fovColor);

        runtimeMaterial.renderQueue = 3000;

        meshRenderer.sortingLayerName = sortingLayerName;
        meshRenderer.sortingOrder = sortingOrder;

        if (eye == null)
            eye = transform.parent != null ? transform.parent : transform;

        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    private void LateUpdate()
    {
        UpdateSightMesh();
    }

    public bool CanSeePlayer(Collider2D playerCollider)
    {
        if (playerCollider == null || eye == null)
            return false;

        Vector2 origin = eye.position;
        Vector2 target = playerCollider.bounds.center;
        Vector2 toPlayer = target - origin;

        float distance = toPlayer.magnitude;

        if (distance <= 0.001f)
            return true;

        if (distance > viewDistance)
            return false;

        Vector2 direction = toPlayer / distance;
        Vector2 forward = eye.up;

        float halfAngle = viewAngleDeg * 0.5f;

        if (Vector2.Angle(forward, direction) > halfAngle)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            distance,
            visionMask
        );

        Debug.DrawRay(origin, direction * distance, Color.red);

        if (!hit)
            return false;

        Transform hitTransform = hit.collider.transform;
        Transform playerRoot = playerCollider.transform.root;

        return hitTransform == playerRoot ||
               hitTransform.IsChildOf(playerRoot);
    }

    private void UpdateSightMesh()
    {
        if (eye == null)
            return;

        int vertexCount = rays + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[rays * 3];

        Vector3 worldOrigin = eye.position;
        vertices[0] = transform.InverseTransformPoint(worldOrigin);

        float halfAngle = viewAngleDeg * 0.5f;

        for (int i = 0; i <= rays; i++)
        {
            float angle =
                -halfAngle + viewAngleDeg * i / rays;

            Vector2 direction =
                Quaternion.Euler(0f, 0f, angle) * eye.up;

            RaycastHit2D hit = Physics2D.Raycast(
                worldOrigin,
                direction,
                viewDistance,
                visionMask
            );

            Vector3 worldPoint = hit
                ? hit.point
                : worldOrigin + (Vector3)(direction * viewDistance);

            vertices[i + 1] =
                transform.InverseTransformPoint(worldPoint);

            if (i >= rays)
                continue;

            int triangleIndex = i * 3;

            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        if (mesh != null)
            Destroy(mesh);

        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }
}