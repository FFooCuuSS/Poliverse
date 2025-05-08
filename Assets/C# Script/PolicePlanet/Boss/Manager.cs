using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject UpPlatform;
    public GameObject DownPlatform;
    public GameObject Police;
    public GameObject Sinner;

    private GameObject currentPerson;

    private bool sinner;

    public enum PlatformType { Up, Down }

    private void Start()
    {
        sinner = true;

        SpawnPlatform(PlatformType.Up);
        SpawnPlatform(PlatformType.Down);
    }

    public void SpawnPlatform(PlatformType type)
    {
        GameObject prefab = type == PlatformType.Up ? UpPlatform : DownPlatform;
        UpDown upDown = prefab.GetComponent<UpDown>();
        upDown.ManagerObj = this.gameObject;
        Instantiate(prefab);

        SpawnPerson();
    }

    public void SpawnPerson()
    {
        currentPerson = Random.Range(0, 2) == 0 ? Police : Sinner;
        Instantiate(currentPerson);
    }
}
