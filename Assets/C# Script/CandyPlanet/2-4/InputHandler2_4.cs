using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler2_4 : MonoBehaviour
{
    [SerializeField] private MiniGame2_4 miniGame;

    void Update()
    {
        if (miniGame == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 touchPosition = Input.mousePosition;

            miniGame.OnPlayerInput();
        }
    }
}
