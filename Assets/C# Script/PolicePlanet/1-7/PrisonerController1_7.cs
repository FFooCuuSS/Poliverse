using UnityEngine;

public class PrisonerController1_7 : MonoBehaviour
{
    public float moveSpeed = 2f;
    private bool isSuccess = false;
    private Collider2D[] childColliders;
    public PrisonerSpawner1_7 prisonerSpawner;

    private GameObject prohibitedItem;

    public void SetProhibitedItem(GameObject item)
    {
        prohibitedItem = item;
    }
    private void Start()
    {
        childColliders = GetComponentsInChildren<Collider2D>();
    }

    void Update()
    {
        if (isSuccess)
        {
            Debug.Log("...");
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

            if (transform.position.x < -15f)
            {
                GameManager1_7.instance.IncreaseSuccessCount();
                
                Destroy(gameObject);
                
            }
        }
        else if (transform.position.x > -0f)
        {
            SetChildCollidersActive(false);
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
            
        }
        else
        {
            SetChildCollidersActive(true);
        }
    }

    public void StartSuccessEscape()
    {
        isSuccess = true;
        Debug.Log("...");
    }
    private void SetChildCollidersActive(bool isActive)
    {
        foreach (Collider2D col in childColliders)
        {
            if (col != null)
            {
                col.enabled = isActive;
            }
        }
    }
}
