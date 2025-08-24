using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficeBox : MonoBehaviour
{
    public bool isOfficer = false;
    private void Update()
    {
        isOfficer = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (!isOfficer && this.gameObject.name == "OfficerPos" && collision.gameObject.name == "PrisonOfficer") 
        {
            isOfficer = true;
            Debug.Log("OfficerSuccess");
        }
        else
        {
            isOfficer = false ;
        }
        

    }
    
}
