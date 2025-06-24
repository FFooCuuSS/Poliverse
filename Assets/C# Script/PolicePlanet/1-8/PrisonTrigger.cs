using UnityEngine;

public class PrisonTrigger : MonoBehaviour
{
    public int totalPrisoners;
    public Manager_1_8 manager;

    private int prisonersInside = 0;

    private void Update()
    {
        //Debug.Log(prisonersInside);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("�浹");

        if (other.CompareTag("Thief"))
        {
            prisonersInside++;
            if (prisonersInside == totalPrisoners)
            {
                manager.GameSuccess();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Thief"))
        {
            prisonersInside--;
        }
    }
}
