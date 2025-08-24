using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EnemyVision_3_12 : MonoBehaviour
{
    [Header("����")]
    [SerializeField] private Transform eye;           

    [Header("FOV")]
    [SerializeField] private float viewAngleDeg = 60f;
    [SerializeField] private float viewDistance = 6f;
    [SerializeField, Range(12, 180)] private int rays = 60;

    [Header("����/����")]
    [SerializeField] private LayerMask visionMask;          
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private string playerTag = "Player";

    [Header("���� �ɼ�")]
    [SerializeField] private Color fovColor = new(1f, 0f, 0f, 0.35f);
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 200;
    [SerializeField] private float facingOffsetDeg = 90f;    

    private MeshFilter mf;
    private MeshRenderer mr;
    private Mesh mesh;
    private Quaternion _offset;

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mesh = mf.sharedMesh ?? (mf.sharedMesh = new Mesh { name = "EnemyVisionMesh" });

        if (mr.sharedMaterial == null)
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default")); // �Ǵ� URP/Unlit

        // ��/����/����
        if (mr.sharedMaterial.HasProperty("_Color")) mr.sharedMaterial.color = fovColor;
        else if (mr.sharedMaterial.HasProperty("_BaseColor")) mr.sharedMaterial.SetColor("_BaseColor", fovColor);
        mr.sharedMaterial.renderQueue = 3000;
        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = sortingOrder;

        if (eye == null) eye = transform.parent != null ? transform.parent : transform;
        if (transform.parent != eye) transform.SetParent(eye, false);
        transform.localPosition = Vector3.zero;

        _offset = Quaternion.Euler(0f, 0f, facingOffsetDeg); // ���� 1ȸ ������
        transform.localRotation = _offset;

        // �ڱ� �ڽſ� ���� �� �°�(����)
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    void LateUpdate()
    {
        // �θ�(eye) ȸ���� �ڵ� ����. �޽��� ����
        UpdateSightMesh();
    }

    public bool CanSeePlayer(Collider2D player)
    {
        if (player == null) return false;
        Transform playerRoot = player.transform.root;

        Bounds pb = player.bounds;
        Vector2 c = pb.center;

        return HasLOS(c, playerRoot);
    }

    bool HasLOS(Vector2 target, Transform playerRoot)
    {
        Vector2 origin = transform.position;
        Vector2 dir = target - origin;
        float dist = dir.magnitude;
        if (dist > viewDistance) return false;

        dir /= dist;

        // �þ߰�(���� = transform.right)
        float half = viewAngleDeg * 0.5f;
        if (Vector2.Angle((Vector2)transform.right, dir) > half) return false;

        // ù ��Ʈ�� ����: Wall + Player�� �ĺ�
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, dist, visionMask);
        Debug.DrawRay(origin, dir * Mathf.Min(dist, viewDistance), Color.red);

        if (!hit) return false;

        // ���� ������ ����
        if (hit.collider.CompareTag(wallTag)) return false;

        // �÷��̾�(Ȥ�� �� �ڽ� �ݶ��̴�)�� ����
        if (hit.collider.CompareTag(playerTag)) return true;
        Transform ht = hit.collider.transform;
        return ht == playerRoot || ht.IsChildOf(playerRoot);
    }

    void UpdateSightMesh()
    {
        int vCount = rays + 2; // origin + ������ ��
        if (mesh.vertexCount != vCount)
        {
            mesh.Clear();
            mesh.vertices = new Vector3[vCount];
            mesh.triangles = new int[rays * 3];
        }

        var verts = mesh.vertices;
        var tris = mesh.triangles;

        verts[0] = Vector3.zero; // ���� ����

        float half = viewAngleDeg * 0.5f;
        Vector3 origin = transform.position;

        for (int i = 0; i <= rays; i++)
        {
            float a = -half + (viewAngleDeg * i) / rays;
            Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, a) * transform.right);

            // FOV �ð��� Wall/Player�� ������ ��������
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewDistance, visionMask);
            Vector3 worldPoint = hit ? (Vector3)hit.point
                                     : origin + (Vector3)(dir * viewDistance);

            verts[i + 1] = transform.InverseTransformPoint(worldPoint);

            if (i < rays)
            {
                int t = i * 3;
                tris[t + 0] = 0;
                tris[t + 1] = i + 1;
                tris[t + 2] = i + 2;
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
    }
}
