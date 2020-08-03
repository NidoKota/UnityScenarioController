using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ScenarioController
{
    /// <summary>
    /// ScenarioDisplayで文字のアニメーションを行う
    /// </summary>
    [RequireComponent(typeof(ScenarioDisplay))]
    public class ScenarioTMPAnimation : MonoBehaviour
    {
        ScenarioDisplay scenarioDisplay;
        TextMeshProUGUI tmp;
        TMP_TextInfo textInfo;
        ScenarioTMPAnimationData animationData;
        List<float> times = new List<float>(100);

        Matrix4x4 matrix;
        Vector3 offset;
        TMP_MeshInfo[] cachedMeshInfo;
        Vector3[] sourceVertices;
        Vector3[] destinationVertices;
        Color32[] destinationColors;

        string textCache;
        int characterCountCache;
        int materialIndex;
        int vertexIndex;
        float animPosY;
        float animRotZ;
        float animScaleAll;
        float animColorAlpha;
        float clacNowTime;

        void Start()
        {
            scenarioDisplay = GetComponent<ScenarioDisplay>();
            tmp = scenarioDisplay.tmp;
            textInfo = tmp.textInfo;

            scenarioDisplay.ScenarioStateChangeEvent += OnScenarioStateChange;
        }

        void OnScenarioStateChange(ScenarioDisplayState state)
        {
            if (state == ScenarioDisplayState.Work)
            {
                animationData = scenarioDisplay.nowScenario.animationData;
                //???メモリアクセス多すぎ？
                times.Clear();
                cachedMeshInfo = null;
                textCache = null;
                characterCountCache = 0;
                animPosY = animRotZ = animScaleAll = 0;
                animColorAlpha = 1;
            }
        }

        void Update()
        {
            //animationDataがセットされたScenarioが再生されている時
            if (scenarioDisplay.State != ScenarioDisplayState.Hide && animationData)
            {
                //Textが更新された時
                if (tmp.text != textCache)
                {
                    tmp.ForceMeshUpdate();
                    cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                    for (int i = characterCountCache; i < textInfo.characterCount; i++) times.Add(Time.time);
                    textCache = tmp.text;
                }

                //1文字ずつ処理
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible) continue;

                    materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    sourceVertices = cachedMeshInfo[materialIndex].vertices;

                    // Determine the center point of each character at the baseline.
                    //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                    // Determine the center point of each character.
                    offset = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                    destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                    destinationColors = textInfo.meshInfo[materialIndex].colors32;

                    destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                    destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                    destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                    destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                    clacNowTime = Time.time - times[i];
                    if (animationData.usePositionYAnimation) animPosY = animationData.positionYAnimation.Evaluate(clacNowTime);
                    if (animationData.useRotationZAnimation) animRotZ = animationData.rotationZAnimation.Evaluate(clacNowTime);
                    if (animationData.useScaleAllAnimation) animScaleAll = animationData.scaleAllAnimation.Evaluate(clacNowTime);
                    if (animationData.useColorAlphaAnimation) animColorAlpha = animationData.colorAlphaAnimation.Evaluate(clacNowTime);

                    matrix = Matrix4x4.TRS(animationData.addPosition + Vector3.up * animPosY, Quaternion.Euler(animationData.addRotation) * Quaternion.Euler(0, 0, animRotZ), animationData.addScale + Vector3.one * animScaleAll);

                    destinationColors[vertexIndex + 0] = new Color(destinationColors[vertexIndex + 0].r, destinationColors[vertexIndex + 0].g, destinationColors[vertexIndex + 0].b, animColorAlpha);
                    destinationColors[vertexIndex + 1] = new Color(destinationColors[vertexIndex + 1].r, destinationColors[vertexIndex + 1].g, destinationColors[vertexIndex + 1].b, animColorAlpha);
                    destinationColors[vertexIndex + 2] = new Color(destinationColors[vertexIndex + 2].r, destinationColors[vertexIndex + 2].g, destinationColors[vertexIndex + 2].b, animColorAlpha);
                    destinationColors[vertexIndex + 3] = new Color(destinationColors[vertexIndex + 3].r, destinationColors[vertexIndex + 3].g, destinationColors[vertexIndex + 3].b, animColorAlpha);

                    destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                    destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                    destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                    destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                    destinationVertices[vertexIndex + 0] += offset;
                    destinationVertices[vertexIndex + 1] += offset;
                    destinationVertices[vertexIndex + 2] += offset;
                    destinationVertices[vertexIndex + 3] += offset;
                }

                tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                if (animationData.useColorAlphaAnimation) tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                characterCountCache = textInfo.characterCount;
            }
        }
    }
}