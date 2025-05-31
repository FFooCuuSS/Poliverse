using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardColor : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isTrapCard=false;
    public void IsSetTrap()
    {
        isTrapCard = true;
        GetComponent<SpriteRenderer>().color=Color.red;
    }
    public void IsNotTrap()
    {
        isTrapCard=false;
        GetComponent<SpriteRenderer>().color=Color.white;
    }
}
