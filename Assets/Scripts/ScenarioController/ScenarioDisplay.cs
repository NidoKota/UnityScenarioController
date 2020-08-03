using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UniRx;

namespace ScenarioController
{
    /// <summary>
    /// Scenarioを画面に表示する(ScenarioDisplayLightに比べて重い)
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScenarioDisplay : ScenarioDisplayBase
    {
        IEnumerable<Scenario> _scenarios;                 //現在処理される複数のScenarioのEnumerator
        CanvasGroup canvasGroup;                          //Scenario全体の親のCanvasGroup
        bool submitOnce;                                  //Scenarioを1度だけ飛ばせるようにする
        bool nextOnce;                                    //次のScenarioに1度だけ移動できるようにする
        float charTime;                                   //1文字を表示する時間
        float charTimer;                                  //文字を表示する処理に使用
        float waitTime;                                   //待機時間
        float waitTimer;                                  //待機時間の処理に使用
        int textCount;                                    //表示しているScenarioのTextの文字数
        int textUpdateCount;                              

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

            _scenarios = scenarios;

            scenarioIndex = 0;
            SetNextScenario();

            //最初にコマンドや改行がないか確認
            CmLfCheck(0, 1);

            SetScenarioDisplayState(ScenarioDisplayState.Work);

            tmp.alpha = 1;
            Fade.FadeIn(canvasGroup);
        }

        public override void ForceStop()
        {
            Fade.FadeForceStop(canvasGroup);
            Fade.FadeForceStop(tmp);
            submitOnce = true;
            scenarioIndex = -1;
            nowScenario = default;

            SetScenarioDisplayState(ScenarioDisplayState.Hide);
        }
        
        void Update()
        {
            if (!submitOnce)
            {
                if (State == ScenarioDisplayState.Work)
                {
                    if (waitTime > 0)
                    {
                        waitTimer += Time.deltaTime;
                        if (waitTime <= waitTimer) waitTime = waitTimer = 0;
                    }
                    else charTimer += Time.deltaTime;

                    //前フレームからの経過時間から次に更新すべき文字数を計算する
                    textUpdateCount = (int)(charTimer / charTime);
                    textCount += textUpdateCount;

                    //文字数が更新された時
                    if (textUpdateCount != 0)
                    {
                        //コマンドや改行が含まれていないか検索し、含まれていたら表示を進める
                        //検索範囲を1つ後ろに拡張して、コマンドや改行が最初に表示されないようにする
                        //Debug.Log($"startInex{textCountCache} length{textCount - textCountCache + 1}");
                        CmLfCheck(textCount - textUpdateCount, textUpdateCount + 1);

                        charTimer = 0;//???少し誤差
                        if (charSE) charSE.PlayOneShot(charSE.clip);
                    }
                    textCount = Mathf.Clamp(textCount, 0, nowScenario.text.Length);

                    tmp.text = nowScenario.text.Substring(0, textCount);

                    if (textCount >= nowScenario.text.Length) SetScenarioDisplayState(ScenarioDisplayState.Wait);
                }

                if (State == ScenarioDisplayState.Hide) canvasGroup.alpha = 0;
            }
        }

        public override bool Next()
        {
            if (State == ScenarioDisplayState.Wait)
            {
                //次のScenarioを表示する
                if (scenarioIndex < _scenarios.Count() - 1)
                {
                    if (!nextOnce)
                    {
                        nextOnce = true;
                        if (nextScenarioSE) nextScenarioSE.PlayOneShot(nextScenarioSE.clip);

                        //文字をフェードする
                        if (nextScenarioFadeOut) Fade.FadeOut(tmp).Subscribe(x => EndWait());
                        //ScenarioTMPAnimationを待つ？
                        else Observable.NextFrame().Subscribe(x => EndWait());
                    }

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
                    submitOnce = true;
                    if (nextScenarioSE) nextScenarioSE.PlayOneShot(nextScenarioSE.clip);
                    Fade.FadeOut(canvasGroup).Subscribe(x =>
                    {
                        SetScenarioDisplayState(ScenarioDisplayState.Hide);
                    });
                }

                return true;
            }
            //Scenarioを飛ばす
            else if (State == ScenarioDisplayState.Work)
            {
                if (cutScenarioSE) cutScenarioSE.PlayOneShot(cutScenarioSE.clip);
                textCount = nowScenario.text.Length;
                textUpdateCount = 0;

                return false;
            }
            else return false;
        }

        /// <summary>
        /// 次のScenarioをセットする
        /// </summary>
        void SetNextScenario()
        {
            waitTime = waitTimer = charTimer = textCount = textUpdateCount = 0;
            nextOnce = submitOnce = false;
            nowScenario = _scenarios.Select((data, index) => new { data, index }).First(x => x.index == scenarioIndex).data;
            charTime = nowScenario.charTime;
            //何故か何か文字を入れないとTMPがアップデートされないので空白を入れる
            tmp.text = " ";
            tmp.ForceMeshUpdate();
        }

        void SetScenarioDisplayState(ScenarioDisplayState state)
        {
            if (State != state)
            {
                State = state;
                ScenarioStateChanged(State);
            }
        }

        int stI;              //検索開始位置
        int len;              //検索する長さ
        int allCmLfCount;     //このフレームで表示する全てのコマンドや改行の長さ
        int lfCount;          //使用されている改行の文字数
        int cmCount;          //使用されているコマンドの文字数
        int lfStartIndex;     //改行が始まる位置
        int cmStartIndex;     //コマンドが始まる位置
        int cou;
        float ct;
        Regex lfReg = new Regex("\n");
        Regex cmReg = new Regex("<");
        Regex nReg = new Regex("<.*>");
        Regex ctReg = new Regex("<cT=(?<v>.+)>");
        Regex wtReg = new Regex("<wT=(?<v>.+)>");
        Match lfChecker;
        Match cmChecker;
        Match nChecker;
        Match ctChecker;
        Match wtChecker;

        //???文字送りが早すぎるとwaitTimeを通り抜けるバグ
        //TextMeshProのTxstの表示設定を変えられるコマンド(<size=150%>や<color=blue>など)や、改行を使った場合それらの文字数を飛ばして表示する
        void CmLfCheck(int startIndex, int length)
        {
            stI = startIndex;     //検索開始位置
            len = length;         //検索する長さ
            allCmLfCount = 0;     //このフレームで表示する全てのコマンドや改行の長さ

            while (true)
            {
                //検索が全体の文字数を超えた場合終了
                if (stI >= nowScenario.text.Length || stI + len >= nowScenario.text.Length || len <= 0) break;

                //コマンドや改行が使われているか検索
                lfChecker = lfReg.Match(nowScenario.text.Substring(stI, len));
                cmChecker = cmReg.Match(nowScenario.text.Substring(stI, len));

                //コマンドや改行が検索結果にない場合終了
                if (!lfChecker.Success && !cmChecker.Success) break;

                lfCount = 0; //使用されている改行の文字数
                cmCount = 0; //使用されているコマンドの文字数
                lfStartIndex = 0; //改行が始まる位置
                cmStartIndex = 0; //コマンドが始まる位置

                //使われている改行が何文字なのか計算
                if (lfChecker.Success)
                {
                    lfStartIndex = stI + lfChecker.Index;
                    lfCount = 1;
                }

                //使われているコマンドが何文字なのか計算
                if (cmChecker.Success)
                {
                    cmStartIndex = stI + cmChecker.Index;
                    cou = 1;
                    while (true)
                    {
                        if (cmStartIndex + cou <= nowScenario.text.Length) cou++;
                        else break;
                        nChecker = nReg.Match(nowScenario.text.Substring(cmStartIndex, cou));
                        if (nChecker.Success)
                        {
                            //charTimeやwaitTimeが変更されるか確認
                            ctChecker = ctReg.Match(nChecker.Value);
                            cmCount = nChecker.Value.Length;
                            if (ctChecker.Success || nChecker.Value == "</cT>")
                            {
                                ct = nowScenario.charTime;
                                if (ctChecker.Success) ct = float.Parse(ctChecker.Groups["v"].Value);
                                if (ct != charTime) charTime = ct;
                            }
                            else
                            {
                                wtChecker = wtReg.Match(nChecker.Value);
                                if (wtChecker.Success)
                                {
                                    //コマンドと被った文字更新数は後回し
                                    waitTime = float.Parse(wtChecker.Groups["v"].Value);
                                }
                            }
                            break;
                        }
                    }
                }

                if(waitTime == 0)
                {
                    //次の検索位置を更新
                    len = stI + len - cmStartIndex - lfStartIndex;
                    stI = cmStartIndex + lfStartIndex + lfCount + cmCount;
                }
                else
                {
                    //次の検索位置を更新
                    len = 1;
                    stI = cmStartIndex + lfStartIndex + lfCount + cmCount;
                }

                //コマンドや改行の文字数分を足す
                allCmLfCount += lfCount + cmCount;

            }

            if(waitTime == 0)
            {
                //全てのコマンドや改行の文字数を足す
                textCount += allCmLfCount;
            }
            else
            {
                //Debug.Log("a");
                textCount = ((lfStartIndex > cmStartIndex) ? lfStartIndex + lfCount : cmStartIndex + cmCount);
            }

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
                GUILayout.Box($"WaitTimer : {waitTimer:F3}", GUILayout.ExpandWidth(false));
                GUILayout.Box($"TextCount : {textCount}", GUILayout.ExpandWidth(false));
                GUILayout.Box($"ScenarioIndex : {scenarioIndex}", GUILayout.ExpandWidth(false));
            }
        }
#endif
    }
}