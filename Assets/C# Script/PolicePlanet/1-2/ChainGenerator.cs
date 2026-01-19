using UnityEngine;

public class ChainGenerator : MonoBehaviour
{
    [Header("Chain Ends")]
    [SerializeField] private GameObject left;      // 체인 시작 (왼손 기준점)
    [SerializeField] private GameObject right;     // 체인 끝 (오른손 기준점)

    [Header("Cuffs")]
    [SerializeField] private GameObject leftCuff;
    [SerializeField] private GameObject rightCuff;

    [Header("Chain")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private int numOfNodes = 10;

    [Header("State")]
    public bool isLeftCuffLocked = false; // 왼손 시스템 고정 여부

    private float nodeDistance = 0.5f;
    private DragAndDrop leftDrag;
    private DragAndDrop rightDrag;

    void Start()
    {
        leftDrag = leftCuff.GetComponent<DragAndDrop>();
        rightDrag = rightCuff.GetComponent<DragAndDrop>();

        Vector2 direction = (right.transform.position - left.transform.position).normalized;
        float totalLength = Vector2.Distance(left.transform.position, right.transform.position);
        nodeDistance = totalLength / (numOfNodes + 1);

        GameObject previous = left;

        for (int i = 0; i < numOfNodes; i++)
        {
            Vector2 nodePos = (Vector2)left.transform.position + direction * nodeDistance * (i + 1);
            GameObject node = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
            ConnectWithHinge(previous, node);
            previous = node;
        }

        ConnectWithHinge(previous, right);
    }

    void Update()
    {
        float dist = Vector2.Distance(left.transform.position, right.transform.position);
        if (dist <= 10.5f) return;

        float followSpeed = 15f;

        // ▶ 오른손을 드래그 중
        if (rightDrag.isDragging)
        {
            // ❗ 왼손이 잠겨 있으면 절대 이동 금지
            if (isLeftCuffLocked) return;

            Vector3 moveTarget = Vector3.MoveTowards(
                left.transform.position,
                right.transform.position,
                followSpeed * Time.deltaTime
            );

            Vector3 offset = moveTarget - left.transform.position;
            leftCuff.transform.position += offset;
        }
        // ▶ 왼손을 드래그 중 (이 경우만 오른손이 따라옴)
        else if (leftDrag.isDragging)
        {
            Vector3 moveTarget = Vector3.MoveTowards(
                right.transform.position,
                left.transform.position,
                followSpeed * Time.deltaTime
            );

            Vector3 offset = moveTarget - right.transform.position;
            rightCuff.transform.position += offset;
        }
    }

    void ConnectWithHinge(GameObject from, GameObject to)
    {
        Rigidbody2D fromRb = from.GetComponent<Rigidbody2D>() ?? from.AddComponent<Rigidbody2D>();
        Rigidbody2D toRb = to.GetComponent<Rigidbody2D>() ?? to.AddComponent<Rigidbody2D>();

        if (to != left && to != right)
            toRb.bodyType = RigidbodyType2D.Dynamic;
        else
            toRb.bodyType = RigidbodyType2D.Kinematic;

        HingeJoint2D hinge = to.GetComponent<HingeJoint2D>() ?? to.AddComponent<HingeJoint2D>();
        hinge.connectedBody = fromRb;
        hinge.autoConfigureConnectedAnchor = false;
        hinge.anchor = new Vector2(-1.2f, 0);
        hinge.connectedAnchor = new Vector2(1.2f, 0);
    }
}
