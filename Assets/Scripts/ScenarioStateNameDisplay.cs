using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScenarioController;

/// <summary>
/// ScenarioDisplayBaseが再生しているScenarioのStateDataを取得して名前を表示する
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class ScenarioStateNameDisplay : MonoBehaviour
{
    [SerializeField] private ScenarioDisplayBase _scenarioDisplay;
    private TMP_Text _text;

    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        _scenarioDisplay.ScenarioStateChangeEvent += OnScenarioStateChange;
    }
    
    private void OnScenarioStateChange(ScenarioDisplayState state)
    {
        ScenarioData scenario = _scenarioDisplay.currentScenario;
        CharacterStateData stateData = scenario.StateData;
        _text.text = stateData ? stateData.ParentAsset.name : null;
    }
}