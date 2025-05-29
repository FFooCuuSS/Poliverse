using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffendBox : MonoBehaviour
{
    public bool isOffend = false;
    
    private void OnTriggerStay2D(Collider2D collision)
    {

        if (!isOffend && this.gameObject.name == "OffendPos" && collision.gameObject.name == "PoliceOffend") 
        {
            isOffend = true;
            Debug.Log("offendSuccess");
        }
       

    }
    
}
