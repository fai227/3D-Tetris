using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;
    private void Awake() => instance = this;

    [Header("Countdown")]
    [SerializeField] private Text countDownText;

    [Header("Pause")]
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private Text pauseText;

    [Header("Result")]
    [SerializeField] private CanvasGroup resultCanvasGroup;
    [SerializeField] private Text winnerText;
    [SerializeField] private Text resultText;
    [SerializeField] private GameObject backButton;

    public void StartCountDown()
    {
        StartCoroutine(CountDown());
    }
    private IEnumerator CountDown()
    {
        countDownText.gameObject.SetActive(true);
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

    public void Pause(int playerNum)
    {
        string pauseString = "<color=#" + ColorUtility.ToHtmlStringRGB(Option.GetPlayerColor(playerNum)) + ">1P</color> Paused";
        pauseText.text = pauseString;

        pauseCanvasGroup.gameObject.SetActive(true);
        pauseCanvasGroup.DOFade(1f, Option.FADE_DURATION).SetUpdate(true);
    }

    public void ShowResult(List<int> winners, int score = 0)
    {
        resultCanvasGroup.gameObject.SetActive(true);
        resultCanvasGroup.alpha = 0f;
        resultCanvasGroup.DOFade(1f, Option.FADE_DURATION);

        string text = "";
        foreach (int winner in winners)
        {
            Color color = Option.GetPlayerColor(winner);
            text += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{winner + 1}P</color> ";
        }
        text += "Win";
        if (winners.Count == 1) text += "s";

        winnerText.text = text;
        if (GameManager.gameMode == GameManager.GameMode.FourtyLines)
        {
            resultText.text = Option.ConvertIntToTime(score);
        }
        else if (GameManager.gameMode == GameManager.GameMode.ScoreAttack)
        {
            resultText.text = score.ToString("00000000");
        }
        else
        {
            resultText.text = "";
        }

        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(backButton);
    }

    public void OnBackSelected()
    {
        // UI設定
        resultCanvasGroup.DOFade(0f, Option.FADE_DURATION).OnComplete(() => resultCanvasGroup.gameObject.SetActive(false));
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(resultCanvasGroup.transform.GetComponentInChildren<Selectable>().gameObject);
        UIManager.instance.ChangePanel(UIManager.PanelName.TitlePanel);

        // BGM
        AudioManager.instance.SetNormalBGM();

        GameManager.instance.SetTitleObject(true);

        // プレイヤー破棄
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) Destroy(player);
    }

    public void Resume() => pauseCanvasGroup.DOFade(0f, Option.FADE_DURATION).SetUpdate(true);
}
