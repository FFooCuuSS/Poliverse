using UnityEngine;

public class ChainGenerator : MonoBehaviour
{
    [SerializeField] private GameObject start;                // 시작 오브젝트
    [SerializeField] private GameObject end;                  // 끝 오브젝트

    [SerializeField] private GameObject startCuff;                // 시작 오브젝트
    [SerializeField] private GameObject endCuff;                // 시작 오브젝트

    [SerializeField] private GameObject nodePrefab;           // 중간 노드 프리팹
    [SerializeField] private int numOfNodes = 10;             // 노드 개수

    private float nodeDistance = 0.5f;
    DragAndDrop startDrag;
    DragAndDrop endDrag;
    void Start()
    {
        startDrag = startCuff.GetComponent<DragAndDrop>();
        endDrag = endCuff.GetComponent<DragAndDrop>();

        Vector2 direction = (end.transform.position - start.transform.position).normalized;
        float totalLength = Vector2.Distance(start.transform.position, end.transform.position);
        nodeDistance = totalLength / (numOfNodes + 1);  // +1 to account for spacing

        GameObject previous = start;

        for (int i = 0; i < numOfNodes; i++)
        {
            Vector2 nodePos = (Vector2)start.transform.position + direction * nodeDistance * (i + 1);
            GameObject node = Instantiate(nodePrefab, nodePos, Quaternion.identity, this.transform);

            // 연결
            ConnectWithHinge(previous, node);

            previous = node;
        }

        // 마지막 노드와 end 연결
        ConnectWithHinge(previous, end);
    }

    private void Update()
    {
        float dist = Vector2.Distance(start.transform.position, end.transform.position);
        if (dist > 10.5f)
        {
            float followSpeed = 15f;

            if (startDrag.isDragging)
            {
                Vector3 moveTarget = Vector3.MoveTowards(
                    end.transform.position,
                    start.transform.position,
                    followSpeed * Time.deltaTime
                );

                Vector3 offset = moveTarget - end.transform.position;

                endCuff.transform.position += offset; // 수갑도 함께 이동
            }
            else if (endDrag.isDragging)
            {
                Vector3 moveTarget = Vector3.MoveTowards(
                    start.transform.position,
                    end.transform.position,
                    followSpeed * Time.deltaTime
                );

                Vector3 offset = moveTarget - start.transform.position;

                startCuff.transform.position += offset; // 수갑도 함께 이동
            }
        }
    }


    void ConnectWithHinge(GameObject from, GameObject to)
    {
        Rigidbody2D fromRb = from.GetComponent<Rigidbody2D>();
        if (fromRb == null)
            fromRb = from.AddComponent<Rigidbody2D>();

        Rigidbody2D toRb = to.GetComponent<Rigidbody2D>();
        if (toRb == null)
            toRb = to.AddComponent<Rigidbody2D>();

        // Dynamic 처리: start나 end가 아니라면
        if (to != start && to != end)
            toRb.bodyType = RigidbodyType2D.Dynamic;
        else
            toRb.bodyType = RigidbodyType2D.Kinematic;

        HingeJoint2D hinge = to.GetComponent<HingeJoint2D>();
        if (hinge == null)
            hinge = to.AddComponent<HingeJoint2D>();

        hinge.connectedBody = fromRb;
        hinge.autoConfigureConnectedAnchor = false;
        hinge.anchor = new Vector2(-1, 0);
        hinge.connectedAnchor = new Vector2(1, 0);
    }
}
