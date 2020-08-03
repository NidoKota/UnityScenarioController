using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using ScenarioController;

/// <summary>
/// Scenarioが表示されるかテストする
/// </summary>
public class ScenarioTest : MonoBehaviour
{
    public ScenarioInput input;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) input.PlayScenario();
        if (Input.GetKeyDown(KeyCode.Z)) input.scenarioDisplayBase.Next();
        if (Input.GetKeyDown(KeyCode.C)) input.scenarioDisplayBase.ForceStop();
    }
}
