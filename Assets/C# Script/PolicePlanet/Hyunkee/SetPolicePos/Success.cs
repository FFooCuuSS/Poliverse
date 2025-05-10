using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Success : MonoBehaviour
{
    public GameObject isDefend;
    public GameObject isOffend;
    public GameObject isOfficer;
    bool isSuccessed = false;
    private void Update()
    {
        if(!isSuccessed&&isSuccess())
        {
            Debug.Log("success");
            isSuccessed = true;
        }

    }
    bool isSuccess()
    {
        var defend = isDefend.GetComponent<DefendBox>();
        var offend = isOffend.GetComponent<OffendBox>();
        var officer = isOfficer.GetComponent<OfficeBox>();
        if(defend.isDefense&&offend.isOffend&&officer.isOfficer)
        {
            return true;
        }
        return false;
    }
    /*public GameObject Defend;
    public GameObject Offend;
    public GameObject Officer;
    public GameObject Enemy;
    bool isSuccess = false;

    private void Update()
    {

        float yDefend =Defend.transform.position.y;
        float yOffend=Offend.transform.position.y;
        float yOfficer=Officer.transform.position.y;
        //var DefendScript = Defend.GetComponent<DragMouseIngame>();
        //DefendScript.
        if (!isSuccess)
        {
            if (yDefend > yOffend && yOffend > yOfficer)
            {
                isSuccess = true;
                Debug.Log("success");
            }
        }
            
        
    }*/

}
