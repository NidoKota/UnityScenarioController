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
    [SerializeField] private ScenarioInput _input;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X)) _input.PlayScenario();
        if (Input.GetKeyDown(KeyCode.Z)) _input.ScenarioDisplayBase.Next();
        if (Input.GetKeyDown(KeyCode.C)) _input.ScenarioDisplayBase.ForceStop();
    }
}