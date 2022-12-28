using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

namespace ScenarioController
{
    // TODO リファクタリング
    
    /// <summary>
    /// Scenarioを画面に表示する(軽量版)
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScenarioDisplayLight : ScenarioDisplayBase
    {
        TMP_TextInfo textInfo;
        CanvasGroup canvasGroup;                                      //Scenario全体の親のCanvasGroup
        TextAnimationData nowAnimationData;                           //現在のScenarioのAnimationData
        List<float> tMPAnimationTimes = new List<float>(100);         //Animationの再生時間のList
        TMP_MeshInfo[] cachedMeshInfo;                                //MeshInfoのキャッシュ
        IObservable<long> nextScenarioWaitTimer;                      //
        IDisposable nextScenarioWaitTimerDis;                         //次のScenarioに移行するまでの待機をキャンセルする
        bool nextOnce;                                                //次のScenarioに1度だけ移動できるようにする
        bool showComplete;                                            //文字の表示が終わったかどうか(Animationは含まない)
        int characterIndex = -1;                                      //今まで表示しているCharacterのIndex
        int characterIndexBefore;                                     //前フレームのcharacterIndex
        float charTime;                                               //1文字を表示する時間
        float charTimer;                                              //文字を表示する処理に使用
        float maxAnimationTime;                                       //一番長いScenarioTMPAnimationDataの長さ
        //float waitTimer;                                            //待機時間の処理に使用
        
        void OnEnable()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            scenarioIndex = -1;
        }

        void Start()
        {
            nextScenarioWaitTimer = Observable.Timer(TimeSpan.FromSeconds(nextScenarioWaitTime));
            Observable.EveryUpdate().Where(x => State == ScenarioDisplayState.Work).Subscribe(WorkEveryUpdate);
        }

        public override void PlayScenario(params ScenarioData[] scenarios)
        {
            PlayScenario((IEnumerable<ScenarioData>)scenarios);
        }

        public override void PlayScenario(IEnumerable<ScenarioData> scenarios)
        {
            gameObject.SetActive(true);
            
            //既にScenarioが表示されている場合
            if (State != ScenarioDisplayState.Hide)
            {
                Debug.LogError($"A new Scenario is now displayed when the old Scenario is displayed!\nScenario content : {scenarios.First().Text}");
                ForceStop();
            }
            textInfo = tmp.textInfo;
            this.scenarios = scenarios;
            scenarioIndex = 0;
            SetNextScenario();

            SetScenarioDisplayState(ScenarioDisplayState.Work);

            if (nextScenarioFadeOut) Fade.FadeIn(canvasGroup);
            else canvasGroup.alpha = 1;
        }

        public override void ForceStop()
        {
            Fade.FadeForceStop(canvasGroup);
            Fade.FadeForceStop(tmp);
            nextScenarioWaitTimerDis?.Dispose();
            if(canvasGroup) canvasGroup.alpha = 0;
            scenarioIndex = -1;
            currentScenario = default;

            SetScenarioDisplayState(ScenarioDisplayState.Hide);
            gameObject.SetActive(false);
        }

        void WorkEveryUpdate(long l)
        {
            nextOnce = false;

            if (!showComplete)
            {
                charTimer += Time.deltaTime;

                //charTimerをcharTimeで何回割れるか考えるのと同じ
                while (charTimer >= charTime)
                {
                    if (characterIndex >= textInfo.characterCount - 1) break;

                    charTimer -= charTime;
                    characterIndex++;

                    if (charSE) charSE.PlayOneShot(charSE.clip);

                    //書き出すと挙動が変わる謎のバグがあるので外す
                    //characterIndexが要素内かつ飛ばす文字なら飛ばす
                    /*while (TMPCharacterIndexWithin(characterIndex) && tmp.text[textInfo.characterInfo[characterIndex].index] == '\n'
                           || TMPCharacterIndexWithin(characterIndex) && tmp.text[textInfo.characterInfo[characterIndex].index] == ' ')
                    {
                        ++characterIndex;
                    }*/
                }

                //characterIndexの数が増えすぎないよう調整
                characterIndex = Mathf.Clamp(characterIndex, -1, textInfo.characterCount - 1);

                if (characterIndex >= textInfo.characterCount - 1) showComplete = true;
            }

            //アニメーションデータがある場合
            if (nowAnimationData)
            {
                //tMPAnimationTimesの要素にアクセスする際、要素番号を飛ばさないために使用
                int tMPAnimationTimesIndex = 0;

                bool finishAnimation = false;

                //0から今まで表示しているCharacterのIndexまで回す
                for (int i = 0; i <= characterIndex; i++)
                {
                    //書き出すと挙動が変わる謎のバグがあるので外す
                    //飛ばす文字なら飛ばす
                    //if (tmp.text[textInfo.characterInfo[i].index] == '\n' || tmp.text[textInfo.characterInfo[i].index] == ' ') continue;

                    //前回のIndexより上(更新した分)を回す
                    if (i > characterIndexBefore)
                    {
                        if (!nowAnimationData.UseColorAlphaAnimation) SetTMPAlpha(i, 1);

                        //現在の時間を格納
                        tMPAnimationTimes.Insert(tMPAnimationTimesIndex, Time.time);
                    }

                    float animationTime = Time.time - tMPAnimationTimes[tMPAnimationTimesIndex];

                    SetTMPAnimation(i, animationTime);
                    if (nowAnimationData.UsePositionXAnimation || nowAnimationData.UsePositionYAnimation || nowAnimationData.UseRotationZAnimation || nowAnimationData.UseScaleAllAnimation) tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                    tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                    finishAnimation = animationTime >= maxAnimationTime;

                    tMPAnimationTimesIndex++;
                }

                //全ての文字が表示されたかつ全てのアニメーションが終わった時
                if (finishAnimation && showComplete) SetScenarioDisplayState(ScenarioDisplayState.Wait);
            }
            //アニメーションデータがない場合
            else
            {
                for (int i = 0; 0 <= characterIndex && i <= characterIndex; i++) SetTMPAlpha(i, 1);
                tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                if (showComplete) SetScenarioDisplayState(ScenarioDisplayState.Wait);
            }

            characterIndexBefore = characterIndex;
        }

        public override bool Next()
        {
            if (State == ScenarioDisplayState.Wait)
            {
                if (!nextOnce)
                {
                    if (nextScenarioSE) nextScenarioSE.PlayOneShot(nextScenarioSE.clip);

                    //次のScenarioを表示する
                    if (scenarioIndex < scenarios.Count() - 1)
                    {
                        SetScenarioDisplayState(ScenarioDisplayState.Next);

                        //文字をフェードする
                        if (nextScenarioFadeOut) Fade.FadeOut(tmp).Subscribe(x => EndWait());
                        else nextScenarioWaitTimerDis = nextScenarioWaitTimer.Subscribe(x => EndWait());

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
                        currentScenario = default;
                        if (nextScenarioFadeOut)
                        {
                            Fade.FadeOut(canvasGroup).Subscribe(x =>
                            {
                                SetScenarioDisplayState(ScenarioDisplayState.Hide);
                                gameObject.SetActive(false);
                            });
                        }
                        else
                        {
                            nextScenarioWaitTimerDis = nextScenarioWaitTimer.Subscribe(x =>
                            {
                                canvasGroup.alpha = 0;
                                SetScenarioDisplayState(ScenarioDisplayState.Hide);
                                gameObject.SetActive(false);
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

                ScenarioCut();

                return false;
            }
            else return false;
        }

        /// <summary>
        /// 次のScenarioを再生するための準備
        /// </summary>
        void SetNextScenario()
        {
            showComplete = false;

            charTimer = 0;
            characterIndex = characterIndexBefore = -1;
                
            tMPAnimationTimes.Clear();

            currentScenario = scenarios.Select((data, index) => (data, index)).First(x => x.index == scenarioIndex).data;

            nowAnimationData = currentScenario.AnimationData;
            if (nowAnimationData) maxAnimationTime = nowAnimationData.GetMaxAnimationTime();
            charTime = currentScenario.CharTime;

            tmp.SetText(currentScenario.Text);
            tmp.ForceMeshUpdate();

            //初めはすべて透明にする
            for (int i = 0; i < textInfo.characterCount; i++) SetTMPAlpha(i, 0);
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

            int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[index].vertexIndex;

            if (nowAnimationData.UsePositionXAnimation || nowAnimationData.UsePositionYAnimation || nowAnimationData.UseRotationZAnimation || nowAnimationData.UseScaleAllAnimation)
            {
                float animPosX = 0;
                float animPosY = 0;
                float animRotZ = 0;
                float animScaleAll = 0;

                if (nowAnimationData.UsePositionXAnimation) animPosX = nowAnimationData.PositionXAnimation.Evaluate(time);
                if (nowAnimationData.UsePositionYAnimation) animPosY = nowAnimationData.PositionYAnimation.Evaluate(time);
                if (nowAnimationData.UseRotationZAnimation) animRotZ = nowAnimationData.RotationZAnimation.Evaluate(time);
                if (nowAnimationData.UseScaleAllAnimation) animScaleAll = nowAnimationData.ScaleAllAnimation.Evaluate(time);

                Matrix4x4 matrix = Matrix4x4.TRS(nowAnimationData.AddPosition + Vector3.up * animPosY + Vector3.right * animPosX, Quaternion.Euler(nowAnimationData.AddRotation) * Quaternion.Euler(0, 0, animRotZ), nowAnimationData.AddScale + Vector3.one * animScaleAll);

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                Vector3 offset = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(sourceVertices[vertexIndex + 0] - offset) + offset;
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(sourceVertices[vertexIndex + 1] - offset) + offset;
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(sourceVertices[vertexIndex + 2] - offset) + offset;
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(sourceVertices[vertexIndex + 3] - offset) + offset;
            }

            if (nowAnimationData.UseColorAlphaAnimation)
            {
                Color32[] destinationColors = textInfo.meshInfo[materialIndex].colors32;

                float animColorAlpha = nowAnimationData.ColorAlphaAnimation.Evaluate(time);

                //ここで色を保管する必要がある
                //(１つ変更したら全部リセットされる実装っぽい?)
                Color colorBefore0 = destinationColors[vertexIndex + 0];
                Color colorBefore1 = destinationColors[vertexIndex + 1];
                Color colorBefore2 = destinationColors[vertexIndex + 2];
                Color colorBefore3 = destinationColors[vertexIndex + 3];
                
                destinationColors[vertexIndex + 0] = new Color(colorBefore0.r, colorBefore0.g, colorBefore0.b, animColorAlpha);
                destinationColors[vertexIndex + 1] = new Color(colorBefore1.r, colorBefore1.g, colorBefore1.b, animColorAlpha);
                destinationColors[vertexIndex + 2] = new Color(colorBefore2.r, colorBefore2.g, colorBefore2.b, animColorAlpha);
                destinationColors[vertexIndex + 3] = new Color(colorBefore3.r, colorBefore3.g, colorBefore3.b, animColorAlpha);
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
            
            Color colorBefore0 = destinationColors[vertexIndex + 0];
            Color colorBefore1 = destinationColors[vertexIndex + 1];
            Color colorBefore2 = destinationColors[vertexIndex + 2];
            Color colorBefore3 = destinationColors[vertexIndex + 3];

            destinationColors[vertexIndex + 0] = new Color(colorBefore0.r, colorBefore0.g, colorBefore0.b, alpha);
            destinationColors[vertexIndex + 1] = new Color(colorBefore1.r, colorBefore1.g, colorBefore1.b, alpha);
            destinationColors[vertexIndex + 2] = new Color(colorBefore2.r, colorBefore2.g, colorBefore2.b, alpha);
            destinationColors[vertexIndex + 3] = new Color(colorBefore3.r, colorBefore3.g, colorBefore3.b, alpha);
        }

        /// <summary>
        /// indexがcharacterCountの中に含まれているかどうか
        /// </summary>
        bool TMPCharacterIndexWithin(int index)
        {
            return textInfo.characterCount > index && index >= 0;
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