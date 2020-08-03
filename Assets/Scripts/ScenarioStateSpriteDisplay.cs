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
    public ScenarioDisplayBase scenarioDisplay;
    Image image;
    int beforeScenarioIndex = -1;

    void Start()
    {
        image = GetComponent<Image>();
        image.enabled = false;
    }

    void Update()
    {
        //再生しているScenarioが変わった時
        if(beforeScenarioIndex != scenarioDisplay.scenarioIndex)
        {
            try
            {
                //現在再生しているScenarioからImageを取得する
                image.sprite = scenarioDisplay.nowScenario.stateData.stateSprite;

                if (image.sprite == null) image.enabled = false;
                else image.enabled = true;
            }
            catch
            {
                image.sprite = null;
                image.enabled = false;
            }

            beforeScenarioIndex = scenarioDisplay.scenarioIndex;
        }
    }
}
