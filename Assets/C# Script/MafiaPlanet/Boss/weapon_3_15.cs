using UnityEngine;

public class weapon_3_15 : MonoBehaviour
{
    [SerializeField] private bool isMagicWand = false;
    [SerializeField] public manager_3_15 manager;     

    private float xMoving;
    [SerializeField] private float movingSpeed = 1f;

    private void Start()
    {
        Destroy(gameObject, 10f);
    }


    void Update()
    {
        transform.position += Vector3.right * xMoving * Time.deltaTime * movingSpeed;
    }

    private void OnMouseDown()
    {
        // 오브젝트 비활성화
        gameObject.SetActive(false);

        // 만약 마법봉이라면 manager의 함수 실행
        if (isMagicWand && manager != null)
        {
            manager.OnMagicWandCollected();
        }
    }

    public void GetXMoving(float xMove)
    {
        xMoving = xMove;
    }
}
