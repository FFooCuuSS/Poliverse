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

    [Header("훈련 번호")]
    [SerializeField, Min(1)]
    private int trackId = 1;

    [Header("이동할 씬")]
    [SerializeField]
    private string practiceSceneName = "PracticeMinigameScene";


    public void trainingButtonClick()
    {
        if (planetList == null)
        {
            Debug.LogError(
                "[PracticeButton] PlanetList가 연결되지 않았습니다."
            );

            return;
        }


        // PlanetList의 현재 선택 행성
        // currentIndex는 0부터 시작하므로 +1
        int planetId =
            planetList.CallingCurrentIndex() + 1;


        // 현재 Lobby 씬
        string returnSceneName =
            SceneManager.GetActiveScene().name;


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


        Debug.Log(
            $"[PracticeButton] 선택 완료 - " +
            $"Planet={planetId}, Track={trackId}"
        );


        // PracticeScene으로 이동
        SceneManager.LoadScene(
            practiceSceneName
        );
    }
}
