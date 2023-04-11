using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Dan.Main;

public class TitleManager : MonoBehaviour
{
    public static TitleManager instance;
    private void Awake() => instance = this;
    private Color fadeColor = new Color(0f, 0f, 0f, 0.5f);

    [Header("Windows Buttons")]
    [SerializeField] private Button leaderboardsButton;
    [SerializeField] private Button controllButton;
    [SerializeField] private Button optionButton;

    [Header("Windows Canvas Group")]
    [SerializeField] private CanvasGroup leaderboardsCanvasGroup;
    [SerializeField] private CanvasGroup controllCanvasGroup;
    [SerializeField] private CanvasGroup optionCanvasGroup;

    private Button previousButton;
    private CanvasGroup previousCanvasGroup;
    private bool changable = true;

    [Header("Leaderboards")]
    [SerializeField] private RectTransform contentOf40Lines;
    [SerializeField] private RectTransform contentOfScoreAttack;
    [SerializeField] private GameObject rankContent;


    [Header("Options")]
    [SerializeField] private Slider seSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        // Window初期化
        previousButton = leaderboardsButton;
        previousCanvasGroup = leaderboardsCanvasGroup;
        ChangeButtonColor(previousButton, Color.cyan);
        previousCanvasGroup.gameObject.SetActive(true);
        previousCanvasGroup.alpha = 1f;

        // オプション初期化
        float se = PlayerPrefs.GetFloat(Option.SE_VOLUME, 1f);
        AudioManager.instance.SetSEVolume(se);
        seSlider.value = se;

        float bgm = PlayerPrefs.GetFloat(Option.BGM_VOLUME, 1f);
        AudioManager.instance.SetBGMVolume(bgm);
        bgmSlider.value = bgm;

        fullscreenToggle.isOn = Screen.fullScreen;
    }

    public void ResetLeaderboards()
    {
        UpdateLeaderboard(true);
        UpdateLeaderboard(false);
    }

    public void UpdateLeaderboard(bool is40Line)
    {
        string publicKey = is40Line ? Option.SEACRET_KEY_FOR_40_LINES : Option.SEACRET_KEY_FOR_SCORE_ATTACK;
        LeaderboardCreator.GetLeaderboard(publicKey, is40Line, (entries) => SetLeaderboard(is40Line, entries));
    }

    #region ボタンのコールバック
    public void On40LinesSelected()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);
        GameManager.gameMode = GameManager.GameMode.FourtyLines;
        ControllerSettingManager.instance.Initialize();
        GameManager.instance.StartWaitForControlls();
    }

    public void OnScoreAttack()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);
        GameManager.gameMode = GameManager.GameMode.ScoreAttack;
        ControllerSettingManager.instance.Initialize();
        GameManager.instance.StartWaitForControlls();
    }

    public void OnMarathonSelected()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);
        GameManager.gameMode = GameManager.GameMode.Marathon;
        ControllerSettingManager.instance.Initialize();
        GameManager.instance.StartWaitForControlls();
    }

    public void OnPartySelected()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);
        GameManager.gameMode = GameManager.GameMode.Party;
        ControllerSettingManager.instance.Initialize();
        GameManager.instance.StartWaitForControlls();
    }

    public void OnLeaderboardsSelected() => ChangeWindow(leaderboardsButton, leaderboardsCanvasGroup);
    public void OnControllSelected() => ChangeWindow(controllButton, controllCanvasGroup);

    public void OnOptionSelected() => ChangeWindow(optionButton, optionCanvasGroup);

    public void OnExitSelected() => UIManager.instance.ExitGame();

    public void OnResetUsernameSelected()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.UsernamePanel);
    }
    #endregion

    #region Windows
    private void ChangeWindow(Button button, CanvasGroup canvasGroup)
    {
        if (!changable) return;
        if (canvasGroup == previousCanvasGroup) return;
        changable = false;

        // 色を変更
        ChangeButtonColor(previousButton, Color.white);
        ChangeButtonColor(button, Color.cyan);

        previousCanvasGroup.DOFade(0f, Option.FADE_DURATION).OnComplete(() =>
        {
            previousCanvasGroup.gameObject.SetActive(false);

            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, Option.FADE_DURATION);

            previousButton = button;
            previousCanvasGroup = canvasGroup;

            // ボタンを有効化
            leaderboardsButton.interactable = true;
            controllButton.interactable = true;
            optionButton.interactable = true;

            changable = true;
        });
    }

    private void ChangeButtonColor(Button button, Color color)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = color;
        button.colors = colorBlock;
    }

    private void SetLeaderboard(bool is40Line, Dan.Models.Entry[] entries)
    {
        RectTransform parent = is40Line ? contentOf40Lines : contentOfScoreAttack;
        foreach (RectTransform child in parent) Destroy(child.gameObject);

        // 高さ設定
        Vector2 size = parent.sizeDelta;
        size.y = 60f * entries.Length - 10f;
        parent.sizeDelta = size;

        int i = 1;
        foreach (Dan.Models.Entry entry in entries)
        {
            GameObject content = Instantiate(rankContent, parent);

            // 順位
            content.transform.GetChild(0).GetComponent<Text>().text = i.ToString();

            // 名前
            content.transform.GetChild(1).GetComponent<Text>().text = entry.Username;

            // スコア
            Text scoreText = content.transform.GetChild(2).GetComponent<Text>();
            if (is40Line)
            {
                scoreText.text = Option.ConvertIntToTime(entry.Score);
            }
            else
            {
                scoreText.text = entry.Score.ToString();
            }
            i++;
        }
    }
    #endregion

    #region Options
    public void OnSEChange(float value)
    {
        PlayerPrefs.SetFloat(Option.SE_VOLUME, value);
        AudioManager.instance.SetSEVolume(value);
    }

    public void OnBGMChange(float value)
    {
        PlayerPrefs.SetFloat(Option.BGM_VOLUME, value);
        AudioManager.instance.SetBGMVolume(value);
    }

    public void OnFullscreenChange(bool flag)
    {
        Screen.fullScreen = flag;
        if (flag)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
    }
    #endregion
}
