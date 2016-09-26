using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashImageFader : MonoBehaviour {

    private float _FadeTime;

    [SerializeField]private CanvasGroup _Group;
    [SerializeField]private float _Delay = 1f;
    [SerializeField]private float _Duration = 3f;
    [SerializeField]private float _FadeDuration = .5f;
    [SerializeField]private int _NextLevelIndex = 1;

    private IEnumerator _In;
    private IEnumerator _Out;

    void Awake() 
    {
        if (_Duration < 0f)
            throw new Exception("_Duration most be positive!");

        if (_FadeDuration < 0f)
            throw new Exception("_FadeDuration most be positive!");

        if ((_Duration - (2 * _FadeDuration)) < 0f)
            throw new Exception("_FadeDuration is invalid. Must be less than duration!");

        if (null == _Group)
            throw new Exception("_Group is not set!");
    }

    void OnEnable() 
    {
        _Group.alpha = 0;

        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(_Delay);

        while (_FadeTime < _FadeDuration)
        {
            _FadeTime += Time.deltaTime;

            _Group.alpha = _FadeTime / (1 -_FadeDuration);

            yield return null;
        }

        yield return new WaitForSeconds(_Duration - (2 * _FadeDuration));

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        while (_FadeTime - Time.deltaTime > 0)
        {
            _FadeTime -= Time.deltaTime;

            _Group.alpha = _FadeTime / (1 - _FadeDuration);

            yield return null;
        }

        SceneManager.LoadSceneAsync(_NextLevelIndex);
    }
}
