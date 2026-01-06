using UnityEngine;

public class PrisonTrigger : MonoBehaviour
{
    public int totalPrisoners;
    public Manager_1_8 manager;

    private PrisonController_1_8 prisonController;
    private int prisonersInside = 0;

    private void Update()
    {
        //Debug.Log(prisonersInside);
        prisonController = GetComponent<PrisonController_1_8>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Ãæµ¹");
        if (!prisonController.IsActive) return;

        if (other.CompareTag("Thief"))
        {
            prisonersInside++;
            Destroy(other.gameObject);

            if (prisonersInside >= totalPrisoners)
            {
                //manager.GameSuccess();
                manager.SendRhythmInput();
            }
        }
    }

    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Thief"))
    //    {
    //        prisonersInside--;
    //    }
    //}
}
