using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SupStepData
{
    [Header("Sup-Step 이미지")]
    public Sprite image;

    [Header("단계 텍스트")]
    [TextArea]
    public string text1;

    [Header("타이틀 텍스트")]
    [TextArea]
    public string text2;
}

[System.Serializable]
public class PlanetTrainingData
{
    public string planetName;

    [Header("이 행성의 Sup-Step")]
    public SupStepData[] supSteps;
}


public class TrainingManager : MonoBehaviour
{
    [Header("Planet")]
    [SerializeField] private PlanetList planetList;

    [SerializeField] private PlanetTrainingData[] planets;

    [Header("Sup-Step Images")]
    [SerializeField] private Image[] supStepImages;

    [Header("Sup-Step Text")]
    [SerializeField] private TMP_Text[] supStepTexts1;

    [Header("Sup-Step Title Text")]
    [SerializeField] private TMP_Text[] supStepTexts2;

    [Header("Training Buttons")]
    [SerializeField] private Button[] trainingButtons;

    [Header("Training Button Images")]
    [SerializeField] private Sprite normalButtonSprite;
    [SerializeField] private Sprite selectedButtonSprite;


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

        // 패널이 열리면 항상 Training 1 표시
        ShowTrainingForPlanet(
            currentPlanetIndex,
            0
        );

        UpdateTrainingButtonImages(0);
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

        UpdateTrainingButtonImages(trainingIndex);
    }

    private void UpdateTrainingButtonImages(int selectedIndex)
    {
        for (int i = 0; i < trainingButtons.Length; i++)
        {
            Image buttonImage =
                trainingButtons[i].GetComponent<Image>();

            if (buttonImage == null)
                continue;

            if (i == selectedIndex)
            {
                buttonImage.sprite = selectedButtonSprite;
            }
            else
            {
                buttonImage.sprite = normalButtonSprite;
            }
        }
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

        SupStepData[] supSteps =
            planets[planetIndex].supSteps;

        ClearSupStep();


        // Training 1 = 0,1,2
        // Training 2 = 3,4,5
        // Training 3 = 6,7,8
        int startIndex = trainingIndex * 3;


        for (int i = 0; i < 3; i++)
        {
            int supStepIndex = startIndex + i;

            if (supStepIndex < supSteps.Length)
            {
                // 이미지
                supStepImages[i].sprite =
                    supSteps[supStepIndex].image;

                supStepImages[i].enabled = true;


                // 텍스트 1
                supStepTexts1[i].text =
                    supSteps[supStepIndex].text1;

                supStepTexts1[i].enabled = true;


                // 텍스트 2
                supStepTexts2[i].text =
                    supSteps[supStepIndex].text2;

                supStepTexts2[i].enabled = true;
            }
        }
    }


    // Sup-Step 이미지 + 텍스트 초기화
    private void ClearSupStep()
    {
        // 이미지
        for (int i = 0; i < supStepImages.Length; i++)
        {
            supStepImages[i].sprite = null;
            supStepImages[i].enabled = false;
        }


        // 텍스트 1
        for (int i = 0; i < supStepTexts1.Length; i++)
        {
            supStepTexts1[i].text = "";
            supStepTexts1[i].enabled = false;
        }


        // 텍스트 2
        for (int i = 0; i < supStepTexts2.Length; i++)
        {
            supStepTexts2[i].text = "";
            supStepTexts2[i].enabled = false;
        }
    }
}
