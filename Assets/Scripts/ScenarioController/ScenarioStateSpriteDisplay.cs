using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ScenarioController;

/// <summary>
/// ScenarioDisplayBaseが再生しているScenarioのstateSpriteを取得してImageに表示する
/// </summary>
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
        try
        {
            image.sprite = scenarioDisplay.nowScenario.stateData.stateSprite;
        }
        catch { }
    }
}
