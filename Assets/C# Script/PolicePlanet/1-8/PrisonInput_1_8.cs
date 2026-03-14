using UnityEngine;

public class PrisonInput_1_8 : MonoBehaviour
{
    private PrisonController_1_8 prisonController;

    private void Awake()
    {
        prisonController = GetComponent<PrisonController_1_8>();
    }
}