using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameManagement3_8 : MonoBehaviour
{
    public GameObject enemy;
    public GameObject enemyNormal;
    public GameObject enemyWatching;
    public GameObject player;
    bool enemyWatch;
    bool playerBox;

    float timeCnt=0f;

    private void Start()
    {
        enemyWatch = false;
        playerBox = false;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(enemyWatch)
        {
            enemyWatching.SetActive(true);
            enemyNormal.SetActive(false);
        }
        if(!enemyWatch)
        {
            enemyWatching.SetActive(false);
            enemyNormal.SetActive(true);
        }
        if(!playerBox)
        { player.SetActive(true); }
        if(playerBox)
        { player.SetActive(false);}
   
        timeCnt += Time.deltaTime;
        if(timeCnt>2)
        {
            if (enemyWatch)
            {
                enemyWatch = false;
                timeCnt = 0;
            }
            else if (!enemyWatch)
            {
                enemyWatch = true;
                timeCnt = 0;
            }
            
        }
        playerBox = false;
        Vector3 enemyPos = enemy.transform.position;
        if (Input.GetMouseButton(0))
        {
            Debug.Log("MBD");
            playerBox = true;
        }
        if (enemyWatch && !playerBox)
        {
            Debug.Log("Fail");
        }
        else if (!enemyWatch && playerBox)
        {
            enemyPos.x -= 0.02f;
        }
        else if (!enemyWatch && !playerBox)
        {
            enemyPos.x += 0.02f;
        }
        else
        {
            enemyPos.x += 0;
        }

            enemy.transform.position = enemyPos;

    }
   
}
