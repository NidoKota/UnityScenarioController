using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ScenarioController;

/// <summary>
/// ScenarioDisplayBaseが再生しているScenarioのstateSpriteを取得してImageに表示する
/// </summary>
[RequireComponent(typeof(Image))]
public class ScenarioStateSpriteDisplay : MonoBehaviour
{
    [SerializeField] private ScenarioDisplayBase _scenarioDisplay;
    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
        _scenarioDisplay.ScenarioStateChangeEvent += OnScenarioStateChange;
    }

    private void OnScenarioStateChange(ScenarioDisplayState state)
    {
        ScenarioData scenario = _scenarioDisplay.currentScenario;
        CharacterStateData stateData = scenario.StateData;
        _image.sprite = stateData ? stateData.StateSprite : null;
    }
}