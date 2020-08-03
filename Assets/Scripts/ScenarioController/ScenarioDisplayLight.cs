using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

namespace ScenarioController
{
    /// <summary>
    /// Scenarioを画面に表示する(軽量版)
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScenarioDisplayLight : ScenarioDisplayBase
    {
        TMP_TextInfo textInfo;
        CanvasGroup canvasGroup;                              //Scenario全体の親のCanvasGroup
        ScenarioTMPAnimationData animationData;               //現在のScenarioのAnimationData
        List<float> tMPAnimationTimes = new List<float>(100); //Animationの再生時間のList(100個にしたのは何となく)

        TMP_MeshInfo[] cachedMeshInfo;                        //MeshInfoのキャッシュ

        IEnumerable<Scenario> _scenarios;                     //現在処理される複数のScenarioのEnumerator
        IDisposable nextScenarioWaitTimeDis;                  //次のScenarioに移行するまでの待機をキャンセルする

        bool nextOnce;                                        //次のScenarioに1度だけ移動できるようにする
        int characterIndex = -1;                              //今まで表示しているCharacterのIndex
        int characterIndexBefore;                             //前フレームのcharacterIndex
        float charTime;                                       //1文字を表示する時間
        float charTimer;                                      //文字を表示する処理に使用
        float maxAnimationTime;                               //一番長いScenarioTMPAnimationDataの長さ
        //float waitTimer;                                    //待機時間の処理に使用

        void OnEnable()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            scenarioIndex = -1;
        }

        public override void PlayScenario(params Scenario[] scenarios)
        {
            PlayScenario((IEnumerable<Scenario>)scenarios);
        }

        public override void PlayScenario(IEnumerable<Scenario> scenarios)
        {
            //既にScenarioが表示されている場合
            if (State != ScenarioDisplayState.Hide)
            {
                Debug.LogError($"Scenarioの表示中に新たなScenarioの表示が開始されました\nScenarioの内容 : {scenarios.First().text}");
                ForceStop();
            }
            textInfo = tmp.textInfo;
            _scenarios = scenarios;
            scenarioIndex = 0;
            tMPAnimationTimes.Clear();
            SetNextScenario();

            SetScenarioDisplayState(ScenarioDisplayState.Work);

            if (nextScenarioFadeOut) Fade.FadeIn(canvasGroup);
            else canvasGroup.alpha = 1;
        }

        public override void ForceStop()
        {
            Fade.FadeForceStop(canvasGroup);
            Fade.FadeForceStop(tmp);
            nextScenarioWaitTimeDis?.Dispose();
            if(canvasGroup) canvasGroup.alpha = 0;
            scenarioIndex = -1;
            nowScenario = default;

            SetScenarioDisplayState(ScenarioDisplayState.Hide);
        }

        void Update()
        {
            if (State == ScenarioDisplayState.Work)
            {
                nextOnce = false;

                charTimer += Time.deltaTime;

                //charTimerをcharTimeで何回割れるか考えるのと同じ
                while (charTimer >= charTime)
                {
                    charTimer -= charTime;
                    if (characterIndex >= textInfo.characterCount - 1) break;
                    ++characterIndex;
                    if (charSE) charSE.PlayOneShot(charSE.clip);

                    //characterIndexが要素内かつ飛ばす文字なら飛ばす
                    while (textInfo.characterInfo.Within(characterIndex) && tmp.text[textInfo.characterInfo[characterIndex].index] == '\n'
                        || textInfo.characterInfo.Within(characterIndex) && tmp.text[textInfo.characterInfo[characterIndex].index] == ' ')
                    {
                        ++characterIndex;
                    }
                }

                //characterIndexの数が増えすぎないよう調整
                characterIndex = Mathf.Clamp(characterIndex, -1, textInfo.characterCount - 1);
                
                if (characterIndex >= textInfo.characterCount - 1) SetScenarioDisplayState(ScenarioDisplayState.Wait);
            }

            //???アニメーションの処理が必要な状態かどうかを判断すべき？
            if (State == ScenarioDisplayState.Work || State == ScenarioDisplayState.Wait)
            {
                //アニメーションデータがある場合
                if (animationData)
                {
                    //tMPAnimationTimesの要素にアクセスする際、要素番号を飛ばさないために使用
                    int tMPAnimationTimesIndex = 0;

                    //0から今まで表示しているCharacterのIndexまで回す
                    for (int i = 0; textInfo.characterInfo.Within(characterIndex) && i <= characterIndex; i++)
                    {
                        //飛ばす文字なら飛ばす
                        if (tmp.text[textInfo.characterInfo[i].index] == '\n' || tmp.text[textInfo.characterInfo[i].index] == ' ') continue;

                        //前回のIndexより上(更新した分)を回す
                        if (i > characterIndexBefore)
                        {
                            if (!animationData.useColorAlphaAnimation) SetTMPAlpha(i, 1);

                            //現在の時間を格納
                            tMPAnimationTimes.Insert(tMPAnimationTimesIndex, Time.time);
                        }

                        SetTMPAnimation(i, Time.time - tMPAnimationTimes[tMPAnimationTimesIndex]);

                        if (animationData.usePositionYAnimation || animationData.useRotationZAnimation || animationData.useScaleAllAnimation)
                            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);

                        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                        tMPAnimationTimesIndex++;
                    }
                }
                //アニメーションデータがない場合
                else
                {
                    for (int i = 0; 0 <= characterIndex && i <= characterIndex; i++) SetTMPAlpha(i, 1);
                    tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                }

                characterIndexBefore = characterIndex;
            }
        }

        public override bool Next()
        {
            if (State == ScenarioDisplayState.Wait)
            {
                if (!nextOnce)
                {
                    if (nextScenarioSE) nextScenarioSE.PlayOneShot(nextScenarioSE.clip);

                    //次のScenarioを表示する
                    if (scenarioIndex < _scenarios.Count() - 1)
                    {
                        //文字をフェードする
                        if (nextScenarioFadeOut) Fade.FadeOut(tmp).Subscribe(x => EndWait());
                        else nextScenarioWaitTimeDis = Observable.Timer(TimeSpan.FromSeconds(nextScenarioWaitTime)).Subscribe(x => EndWait());

                        void EndWait()
                        {
                            tmp.alpha = 1;

                            scenarioIndex++;
                            SetNextScenario();

                            SetScenarioDisplayState(ScenarioDisplayState.Work);
                        }
                    }
                    //Scenarioの表示を終わらせる
                    else
                    {
                        scenarioIndex = -1;
                        nowScenario = default;
                        if (nextScenarioFadeOut)
                        {
                            Fade.FadeOut(canvasGroup).Subscribe(x =>
                            {
                                SetScenarioDisplayState(ScenarioDisplayState.Hide);
                            });
                        }
                        else
                        {
                            nextScenarioWaitTimeDis = Observable.Timer(TimeSpan.FromSeconds(nextScenarioWaitTime)).Subscribe(x =>
                            {
                                canvasGroup.alpha = 0;
                                SetScenarioDisplayState(ScenarioDisplayState.Hide);
                            });
                        }
                    }

                    nextOnce = true;
                }

                return true;
            }
            //Scenarioを飛ばす
            else if (State == ScenarioDisplayState.Work)
            {
                if (cutScenarioSE) cutScenarioSE.PlayOneShot(cutScenarioSE.clip);
                characterIndex = textInfo.characterCount - 1;

                return false;
            }
            else return false;
        }

        /// <summary>
        /// 次のScenarioを再生するための準備
        /// </summary>
        void SetNextScenario()
        {
            charTimer = 0;
            characterIndex = characterIndexBefore = -1;
            
            nowScenario = _scenarios.Select((data, index) => (data, index)).First(x => x.index == scenarioIndex).data;

            animationData = nowScenario.animationData;
            if(animationData) maxAnimationTime = nowScenario.animationData.GetMaxAnimationTime();
            charTime = nowScenario.charTime;

            tmp.SetText(nowScenario.text);
            //いらん可能性あり
            tmp.ForceMeshUpdate();
            //初めはすべて透明にする
            for (int i = 0; i < tmp.text.Length; i++) SetTMPAlpha(i, 0);
            tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
        }

        void SetScenarioDisplayState(ScenarioDisplayState state)
        {
            if (State != state)
            {
                State = state;
                ScenarioStateChanged(State);
            }
        }

        /// <summary>
        /// ScenarioTMPAnimationDataで設定された値を適応する(indexはcharacter)
        /// </summary>
        void SetTMPAnimation(int index, float time)
        {
            if (!textInfo.characterInfo[index].isVisible) return;
            if (time >= maxAnimationTime) return; //???最初からmaxAnimationTimeを超えていた場合何も表示されない可能性

            int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[index].vertexIndex;

            if (animationData.usePositionYAnimation || animationData.useRotationZAnimation || animationData.useScaleAllAnimation)
            {
                float animPosY = 0;
                float animRotZ = 0;
                float animScaleAll = 0;

                if (animationData.usePositionYAnimation) animPosY = animationData.positionYAnimation.Evaluate(time);
                if (animationData.useRotationZAnimation) animRotZ = animationData.rotationZAnimation.Evaluate(time);
                if (animationData.useScaleAllAnimation) animScaleAll = animationData.scaleAllAnimation.Evaluate(time);

                Matrix4x4 matrix = Matrix4x4.TRS(animationData.addPosition + Vector3.up * animPosY, Quaternion.Euler(animationData.addRotation) * Quaternion.Euler(0, 0, animRotZ), animationData.addScale + Vector3.one * animScaleAll);

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                Vector3 offset = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(sourceVertices[vertexIndex + 0] - offset) + offset;
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(sourceVertices[vertexIndex + 1] - offset) + offset;
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(sourceVertices[vertexIndex + 2] - offset) + offset;
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(sourceVertices[vertexIndex + 3] - offset) + offset;
            }

            if (animationData.useColorAlphaAnimation)
            {
                Color32[] destinationColors = textInfo.meshInfo[materialIndex].colors32;

                float animColorAlpha = animationData.colorAlphaAnimation.Evaluate(time);

                destinationColors[vertexIndex + 0] = new Color(destinationColors[vertexIndex + 0].r, destinationColors[vertexIndex + 0].g, destinationColors[vertexIndex + 0].b, animColorAlpha);
                destinationColors[vertexIndex + 1] = new Color(destinationColors[vertexIndex + 1].r, destinationColors[vertexIndex + 1].g, destinationColors[vertexIndex + 1].b, animColorAlpha);
                destinationColors[vertexIndex + 2] = new Color(destinationColors[vertexIndex + 2].r, destinationColors[vertexIndex + 2].g, destinationColors[vertexIndex + 2].b, animColorAlpha);
                destinationColors[vertexIndex + 3] = new Color(destinationColors[vertexIndex + 3].r, destinationColors[vertexIndex + 3].g, destinationColors[vertexIndex + 3].b, animColorAlpha);
            }
        }

        /// <summary>
        /// TMPの透明度を適応する(indexはcharacter)
        /// </summary>
        void SetTMPAlpha(int index, float alpha)
        {
            if (!textInfo.characterInfo[index].isVisible) return;
            
            int vertexIndex = textInfo.characterInfo[index].vertexIndex;
            Color32[] destinationColors = textInfo.meshInfo[textInfo.characterInfo[index].materialReferenceIndex].colors32;

            //???tmp.colorが白と黒以外の場合、うまく適応されない問題
            destinationColors[vertexIndex + 0] = new Color(destinationColors[vertexIndex + 0].r, destinationColors[vertexIndex + 0].g, destinationColors[vertexIndex + 0].b, alpha);
            destinationColors[vertexIndex + 1] = new Color(destinationColors[vertexIndex + 1].r, destinationColors[vertexIndex + 1].g, destinationColors[vertexIndex + 1].b, alpha);
            destinationColors[vertexIndex + 2] = new Color(destinationColors[vertexIndex + 2].r, destinationColors[vertexIndex + 2].g, destinationColors[vertexIndex + 2].b, alpha);
            destinationColors[vertexIndex + 3] = new Color(destinationColors[vertexIndex + 3].r, destinationColors[vertexIndex + 3].g, destinationColors[vertexIndex + 3].b, alpha);
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            //情報を表示
            if (InfoDebug)
            {
                GUILayout.Box($"State : {State}", GUILayout.ExpandWidth(false));
                GUILayout.Box($"CharTime : {charTime:F3}", GUILayout.ExpandWidth(false));
                GUILayout.Box($"CharTimer : {charTimer:F3}", GUILayout.ExpandWidth(false));
                GUILayout.Box($"ScenarioIndex : {scenarioIndex}", GUILayout.ExpandWidth(false));
                GUILayout.Box($"CharacterIndex : {characterIndex}", GUILayout.ExpandWidth(false));
            }
        }
#endif
    }
}