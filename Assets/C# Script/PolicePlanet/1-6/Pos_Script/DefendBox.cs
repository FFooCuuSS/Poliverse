using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendBox : MonoBehaviour
{

    public bool isDefense = false;
    private void Update()
    {
        isDefense = false;
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isDefense && this.gameObject.name == "DefendPos" && collision.gameObject.name == "PoliceDefend") 
        {
            isDefense = true;
            Debug.Log("DefendSuccess");
            //this.GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            isDefense = false;
        }
       

        
    }
    
}
