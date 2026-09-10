using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlanetTrainingData
{
    public string planetName;

    [Header("이 행성의 Sup-Step Sprite")]
    public Sprite[] supSteps;
}

public class TrainingManager : MonoBehaviour
{
    [Header("Planet")]
    [SerializeField] private PlanetList planetList;

    [SerializeField] private PlanetTrainingData[] planets;

    [Header("Sup-Step Images")]
    [SerializeField] private Image[] supStepImages;

    [Header("Training Buttons")]
    [SerializeField] private Button[] trainingButtons;


    private void Start()
    {
        SetupTrainingButtons();
    }

    private void OnEnable()
    {
        if (planetList == null)
        {
            Debug.LogWarning("PlanetList가 연결되지 않았습니다.");
            return;
        }

        int currentPlanetIndex =
            planetList.CallingCurrentIndex();

        Debug.Log(
            "Training 패널 열림 - 현재 행성 : "
            + currentPlanetIndex
        );

        ShowTrainingForPlanet(
            currentPlanetIndex,
            0
        );
    }

    private void SetupTrainingButtons()
    {
        for (int i = 0; i < trainingButtons.Length; i++)
        {
            int trainingIndex = i;

            trainingButtons[i].onClick.AddListener(
                () => ShowTraining(trainingIndex)
            );
        }
    }

    private void ShowTraining(int trainingIndex)
    {
        int currentPlanetIndex =
            planetList.CallingCurrentIndex();

        ShowTrainingForPlanet(
            currentPlanetIndex,
            trainingIndex
        );
    }

    private void ShowTrainingForPlanet(
        int planetIndex,
        int trainingIndex)
    {
        if (planetIndex < 0 ||
            planetIndex >= planets.Length)
        {
            Debug.LogWarning(
                "잘못된 행성 번호 : " + planetIndex
            );

            return;
        }

        Sprite[] supSteps =
            planets[planetIndex].supSteps;

        ClearSupStepImages();

        int startIndex = trainingIndex * 3;

        for (int i = 0; i < 3; i++)
        {
            int supStepIndex = startIndex + i;

            if (supStepIndex < supSteps.Length)
            {
                supStepImages[i].sprite =
                    supSteps[supStepIndex];

                supStepImages[i].enabled = true;
            }
        }
    }

    // Sup-Step 이미지 초기화
    private void ClearSupStepImages()
    {
        for (int i = 0; i < supStepImages.Length; i++)
        {
            supStepImages[i].sprite = null;
            supStepImages[i].enabled = false;
        }
    }
}
