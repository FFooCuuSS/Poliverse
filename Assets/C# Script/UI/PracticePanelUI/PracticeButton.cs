using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PracticeButton : MonoBehaviour
{
    [Header("현재 선택된 행성")]
    [SerializeField]
    private PlanetList planetList;


    [Header("Training Manager")]
    [SerializeField]
    private TrainingManager trainingManager;


    [Header("이동할 씬")]
    [SerializeField]
    private string practiceSceneName =
        "PracticeMinigameScene";


    public void TrainingButtonClick()
    {
        // PlanetList 확인
        if (planetList == null)
        {
            Debug.LogError(
                "[PracticeButton] PlanetList가 연결되지 않았습니다."
            );

            return;
        }


        // TrainingManager 확인
        if (trainingManager == null)
        {
            Debug.LogError(
                "[PracticeButton] TrainingManager가 연결되지 않았습니다."
            );

            return;
        }


        // 현재 선택된 행성
        //
        // PlanetList의 index
        // 0 → 행성 1
        // 1 → 행성 2
        // 2 → 행성 3
        int planetId =
            planetList.CallingCurrentIndex() + 1;


        // 현재 선택된 훈련
        // 1 ~ 5
        int trackId =
            trainingManager.GetCurrentTrainingId();


        // 현재 Lobby 씬
        string returnSceneName =
            SceneManager.GetActiveScene().name;


        Debug.Log(
            $"[PracticeButton] 훈련 시작\n" +
            $"Planet = {planetId}\n" +
            $"Training = {trackId}\n" +
            $"Return Scene = {returnSceneName}"
        );


        // 선택 정보 저장
        PlayerPrefs.SetInt(
            "PracticePlanetId",
            planetId
        );

        PlayerPrefs.SetInt(
            "PracticeTrackId",
            trackId
        );

        PlayerPrefs.SetString(
            "PracticeReturnScene",
            returnSceneName
        );

        PlayerPrefs.Save();


        // PracticeMinigameScene 이동
        SceneManager.LoadScene(
            practiceSceneName
        );
    }
}
