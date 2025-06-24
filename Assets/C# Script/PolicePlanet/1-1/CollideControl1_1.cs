using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideControl1_1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag=="Enemy")
        {
            GetComponent<CheckFalse1_1>().isSpyers = true;
        }
    }
}
