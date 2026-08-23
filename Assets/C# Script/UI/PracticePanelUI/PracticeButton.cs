using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PracticeButton : MonoBehaviour
{
    public void trainingButtonClick()
    {
        Invoke("trainingLoading", 1f);
    }

    void trainingLoading()
    {
        SceneManager.LoadScene("PracticeMinigame");
    }
}
