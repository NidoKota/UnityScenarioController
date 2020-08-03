using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ScenarioController;

public class ScenarioStateSpriteDisplay : MonoBehaviour
{
    public ScenarioDisplayBase scenarioDisplay;
    Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        image.sprite = scenarioDisplay.nowScenario == null ? null : scenarioDisplay.nowScenario.stateData == null ? null : scenarioDisplay.nowScenario.stateData.stateSprite;
    }
}
