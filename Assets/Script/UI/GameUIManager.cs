using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;
    private void Awake() => instance = this;

    [SerializeField] private Text countDownText;

    public void StartCountDown()
    {
        StartCoroutine(CountDown());
    }
    private IEnumerator CountDown()
    {
        for (int i = 3; i > 0; i--)
        {
            countDownText.text = i.ToString();
            countDownText.DOFade(1f, Option.FADE_DURATION);
            yield return new WaitForSeconds(1f);
            countDownText.color = Color.clear;
        }

        countDownText.text = "Start";
        countDownText.DOFade(1f, Option.FADE_DURATION).WaitForCompletion();
        yield return new WaitForSeconds(1f);
        countDownText.DOFade(0f, Option.FADE_DURATION);
    }
}
