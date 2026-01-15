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

        if (!other.CompareTag("Thief")) return;


        Prisoner_1_8 prisoner = other.GetComponent<Prisoner_1_8>();
        if (prisoner == null)
        {
            return;
        }

        PrisonController_1_8 controller = GetComponent<PrisonController_1_8>();

        if (!controller.IsActive) return;
        if (!prisoner.canBeCaptured) return;

        Debug.Log("잡았다!");
        prisoner.Capture();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Enter와 동일한 로직을 계속 검사
        OnTriggerEnter2D(other);
    }




    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Thief"))
    //    {
    //        prisonersInside--;
    //    }
    //}
}
