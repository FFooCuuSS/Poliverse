using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPracticeDemoInput
{
    void SetDemoMode(bool isDemo);
}

public interface IPracticeDemoActionInput : IPracticeDemoInput
{
    void ExecutePracticeAction(int actionIndex, string actionType);
}
