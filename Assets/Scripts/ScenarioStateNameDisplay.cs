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
    public ScenarioDisplayBase scenarioDisplay;

    TMP_Text text;
    int beforeScenarioIndex = -1;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        //再生しているScenarioが変わった時
        if(beforeScenarioIndex != scenarioDisplay.scenarioIndex)
        {
            try
            {
                //現在再生しているScenarioからStateDataの名前を取得する
                text.text = scenarioDisplay.nowScenario.stateData.ParentAsset.name;
            }
            catch { }

            beforeScenarioIndex = scenarioDisplay.scenarioIndex;
        }
    }
}
