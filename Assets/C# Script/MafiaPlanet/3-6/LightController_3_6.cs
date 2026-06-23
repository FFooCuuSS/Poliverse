using UnityEngine;

public class LightController_3_6 : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private GameObject greenLight;
    [SerializeField] private GameObject yellowLight;
    [SerializeField] private GameObject redLight;

    public void ShowGreen()
    {
        TurnOffAll();
        greenLight.SetActive(true);
    }

    public void ShowYellow()
    {
        TurnOffAll();
        yellowLight.SetActive(true);
    }

    public void ShowRed()
    {
        TurnOffAll();
        redLight.SetActive(true);
    }

    public void TurnOffAll()
    {
        if (greenLight != null) greenLight.SetActive(false);
        if (yellowLight != null) yellowLight.SetActive(false);
        if (redLight != null) redLight.SetActive(false);
    }
}