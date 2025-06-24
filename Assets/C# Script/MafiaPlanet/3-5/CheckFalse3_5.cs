using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckFalse3_5 : MonoBehaviour
{
    public GameObject Spyers;
    public bool isSpyers=false;
    // Start is called before the first frame update
    void Start()
    {
        Spyers.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(isSpyers) Spyers.SetActive(true);
    }
}
