using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonInput_1_8 : MonoBehaviour
{
    private Minigame_1_8 minigame;
    private PrisonController_1_8 prisonController;

    private void Start()
    {
        minigame = FindObjectOfType<Minigame_1_8>();
        prisonController = GetComponent<PrisonController_1_8>();
    }

    private void OnMouseDown()
    {
        if (minigame == null || prisonController == null)
            return;

        /*
        if (!minigame.CanTap)
            return;
        */
        // 감옥 작동
        prisonController.ActivatePrison();

        // Tap 소비
        minigame.UseTap();
    }
}
