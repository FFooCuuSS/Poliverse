using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeNumber : MonoBehaviour
{
    [SerializeField] private List<GameObject> lives;

    private int currentLives;

    private void Start()
    {
        currentLives = lives.Count;
        UpdateLivesUI();
    }


    public void LoseLife()
    {
        if (currentLives <= 0)
        {
            return;
        }

        currentLives--;
        UpdateLivesUI();
    }

    private void UpdateLivesUI()
    {
        for (int i = 0; i < lives.Count; i++)
        {
            lives[i].SetActive(i < currentLives);
        }
    }
}
