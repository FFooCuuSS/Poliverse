using UnityEngine;

public class ChainGenerator : MonoBehaviour
{
    [SerializeField] private GameObject start;                // ���� ������Ʈ
    [SerializeField] private GameObject end;                  // �� ������Ʈ

    [SerializeField] private GameObject startCuff;                // ���� ������Ʈ
    [SerializeField] private GameObject endCuff;                // ���� ������Ʈ

    [SerializeField] private GameObject nodePrefab;           // �߰� ��� ������
    [SerializeField] private int numOfNodes = 10;             // ��� ����

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

            // ����
            ConnectWithHinge(previous, node);

            previous = node;
        }

        // ������ ���� end ����
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

                endCuff.transform.position += offset; // ������ �Բ� �̵�
            }
            else if (endDrag.isDragging)
            {
                Vector3 moveTarget = Vector3.MoveTowards(
                    start.transform.position,
                    end.transform.position,
                    followSpeed * Time.deltaTime
                );

                Vector3 offset = moveTarget - start.transform.position;

                startCuff.transform.position += offset; // ������ �Բ� �̵�
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

        // Dynamic ó��: start�� end�� �ƴ϶��
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
