using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

/// <summary>
/// 簡単にFade処理を行えるClass
/// </summary>
public class Fade
{
    /// <summary>
    /// デフォルトのFadeの速さ
    /// </summary>
    public static float defaultSpeed { get { return 3f; } }

    /// <summary>
    /// デフォルトでフェードするImage(画面暗転用)を取得
    /// </summary>
    public static Image GetDefaultImage()
    {
        return GameObject.Find("ForwardCamCanvas/FadeImage").GetComponent<Image>();
    }

    //強制停止できるようにdisposableを保管する
    static Dictionary<object, IDisposable> disposables = new Dictionary<object, IDisposable>();

    /// <summary>
    /// デフォルトのImageをデフォルトの速さでFadeIn
    /// </summary>
    public static IObservable<Unit> FadeIn(bool unscaledTime = false, bool saveDisposable = true)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, GetDefaultImage(), defaultSpeed, 0, 1, unscaledTime, saveDisposable)).Subscribe(sub);
        if (saveDisposable)
        {
            if (disposables.ContainsKey(GetDefaultImage()))
            {
                disposables[GetDefaultImage()].Dispose();
                disposables[GetDefaultImage()] = dis;
            }
            else disposables.Add(GetDefaultImage(), dis);
        }
        return sub;
    }

    /// <summary>
    /// 指定したオブジェクトをデフォルトの速さでFadeIn
    public static IObservable<Unit> FadeIn<T>(T fadeObject, bool unscaledTime = false, bool saveDisposable = true)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, defaultSpeed, 0, 1, unscaledTime, saveDisposable)).Subscribe(sub);
        if (saveDisposable)
        {
            if (disposables.ContainsKey(fadeObject))
            {
                disposables[fadeObject].Dispose();
                disposables[fadeObject] = dis;
            }
            else disposables.Add(fadeObject, dis);
        }
        return sub;
    }

    /// <summary>
    /// 指定したオブジェクトを指定した速さでFadeIn
    /// </summary>
    public static IObservable<Unit> FadeIn<T>(T fadeObject, float fadeSpeed, bool unscaledTime = false, bool saveDisposable = true)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, fadeSpeed, 0, 1, unscaledTime, saveDisposable)).Subscribe(sub);
        if (saveDisposable)
        {
            if (disposables.ContainsKey(fadeObject))
            {
                disposables[fadeObject].Dispose();
                disposables[fadeObject] = dis;
            }
            else disposables.Add(fadeObject, dis);
        }
        return sub;
    }

    /// <summary>
    /// デフォルトのImageをデフォルトの速さでFadeOut
    /// </summary>
    public static IObservable<Unit> FadeOut(bool unscaledTime = false, bool saveDisposable = true)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, GetDefaultImage(), defaultSpeed, 1, 0, unscaledTime, saveDisposable)).Subscribe(sub);
        if (saveDisposable)
        {
            if (disposables.ContainsKey(GetDefaultImage()))
            {
                disposables[GetDefaultImage()].Dispose();
                disposables[GetDefaultImage()] = dis;
            }
            else disposables.Add(GetDefaultImage(), dis);
        }
        return sub;
    }

    /// <summary>
    /// 指定したオブジェクトをデフォルトの速さでFadeOut
    /// </summary>
    public static IObservable<Unit> FadeOut<T>(T fadeObject, bool unscaledTime = false, bool saveDisposable = true)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, defaultSpeed, 1, 0, unscaledTime, saveDisposable)).Subscribe(sub);
        if (saveDisposable)
        {
            if (disposables.ContainsKey(fadeObject))
            {
                disposables[fadeObject].Dispose();
                disposables[fadeObject] = dis;
            }
            else disposables.Add(fadeObject, dis);
        }
        return sub;
    }

    /// <summary>
    /// 指定したオブジェクトを指定した速さでFadeOut
    /// </summary>
    public static IObservable<Unit> FadeOut<T>(T fadeObject, float fadeSpeed, bool unscaledTime = false, bool saveDisposable = true)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, fadeSpeed, 1, 0, unscaledTime, saveDisposable)).Subscribe(sub);
        if (saveDisposable)
        {
            if (disposables.ContainsKey(fadeObject))
            {
                disposables[fadeObject].Dispose();
                disposables[fadeObject] = dis;
            }
            else disposables.Add(fadeObject, dis);
        }
        return sub;
    }

    /// <summary>
    /// オブジェクト 速さ 最初と最後の透明度を自由に決定できる
    /// </summary>
    public static IObservable<Unit> FadeCustom<T>(T fadeObject, float fadeSpeed, float startAlpha, float endAlpha, bool unscaledTime = false, bool saveDisposable = true)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, fadeSpeed, startAlpha, endAlpha, unscaledTime, saveDisposable)).Subscribe(sub);
        if (saveDisposable)
        {
            if (disposables.ContainsKey(fadeObject))
            {
                disposables[fadeObject].Dispose();
                disposables[fadeObject] = dis;
            }
            else disposables.Add(fadeObject, dis);
        }
        return sub;
    }

    /// <summary>
    /// Fadeを強制停止する
    /// </summary>
    public static void FadeForceStop(object key)
    {
        if (disposables.ContainsKey(key))
        {
            disposables[key].Dispose();
            disposables.Remove(key);
        }
    }

    //Fadeさせる
    static IEnumerator Fe<T>(IObserver<Unit> observer, T fadeObject, float speed, float firstAlfa, float endAlfa, bool unscaledTime, bool saveDisposable)
    {
        float t = 0;
        while (true)
        {
            try
            {
                if (unscaledTime) t += Time.unscaledDeltaTime * speed;
                else t += Time.deltaTime * speed;
                if (typeof(T) == typeof(Image)) (fadeObject as Image).color = new Color((fadeObject as Image).color.r, (fadeObject as Image).color.g, (fadeObject as Image).color.b, Mathf.Lerp(firstAlfa, endAlfa, t));
                else if (typeof(T) == typeof(CanvasGroup)) (fadeObject as CanvasGroup).alpha = Mathf.Lerp(firstAlfa, endAlfa, t);
                else if (typeof(T) == typeof(SpriteRenderer)) (fadeObject as SpriteRenderer).color = new Color((fadeObject as SpriteRenderer).color.r, (fadeObject as SpriteRenderer).color.g, (fadeObject as SpriteRenderer).color.b, Mathf.Lerp(firstAlfa, endAlfa, t));
                else if (typeof(T) == typeof(TextMeshProUGUI)) (fadeObject as TextMeshProUGUI).alpha = Mathf.Lerp(firstAlfa, endAlfa, t);
                else if (typeof(T) == typeof(Renderer)) (fadeObject as Renderer).material.color = new Color((fadeObject as Renderer).material.color.r, (fadeObject as Renderer).material.color.g, (fadeObject as Renderer).material.color.b, Mathf.Lerp(firstAlfa, endAlfa, t));
                else if (typeof(T) == typeof(Text)) (fadeObject as Text).color = new Color((fadeObject as Text).color.r, (fadeObject as Text).color.g, (fadeObject as Text).color.b, Mathf.Lerp(firstAlfa, endAlfa, t));
                else if (typeof(T) == typeof(AudioSource)) (fadeObject as AudioSource).volume = Mathf.Lerp(firstAlfa, endAlfa, t);
                else throw new Exception("未実装の型で実行されました");
            }
            catch { }

            if (t >= 1)
            {
                if (saveDisposable) disposables.Remove(fadeObject);
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }
            yield return null;
        }
    }
}