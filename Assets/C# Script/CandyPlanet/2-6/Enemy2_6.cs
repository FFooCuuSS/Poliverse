using UnityEngine;

public class Enemy2_6 : MonoBehaviour
{
    public MiniGame2_6 _minigame2_6;
    public string _minigameName = "2-6minigame";
    public string targetName = "TargetObject"; // �� �ȿ��� ã�� Ÿ�� ������Ʈ �̸�
    private Transform target;
    public float speed = 5f;   // �̵� �ӵ�

    void Start()
    {
        GameObject found = GameObject.Find(targetName);
        if (found != null)
        {
            target = found.transform;
        }
        else
        {
            Debug.LogWarning($"�� �ȿ��� '{targetName}' ������Ʈ�� ã�� ���߽��ϴ�.");
        }
        GameObject game = GameObject.Find(_minigameName);
        if (found != null)
        {
            _minigame2_6 = game.GetComponent<MiniGame2_6>();
        }
        else
        {
            Debug.LogWarning($"�� �ȿ��� '{_minigameName}' ������Ʈ�� ã�� ���߽��ϴ�.");
        }

    }

    void Update()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject); // �ڱ� �ڽ� ����
            _minigame2_6.Fail();
        }
    }
}
