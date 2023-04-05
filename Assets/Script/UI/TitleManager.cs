using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

        // ランキング表示

    }

    #region ボタンのコールバック
    public void On40LinesSelected()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);
        GameManager.instance.StartWaitForControlls();
    }

    public void OnScoreAttack()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);
        GameManager.instance.StartWaitForControlls();
    }

    public void OnMarathonSelected()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);
        GameManager.instance.StartWaitForControlls();
    }

    public void OnPartySelected()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);
        GameManager.instance.StartWaitForControlls();
    }

    public void OnLeaderboardsSelected() => ChangeWindow(leaderboardsButton, leaderboardsCanvasGroup);
    public void OnControllSelected() => ChangeWindow(controllButton, controllCanvasGroup);

    public void OnOptionSelected() => ChangeWindow(optionButton, optionCanvasGroup);

    public void OnExitSelected() => UIManager.instance.ExitGame();

    public void OnResetUsernameSelected() => UIManager.instance.ChangePanel(UIManager.PanelName.UsernamePanel);
    #endregion

    #region Windows
    private void ChangeWindow(Button button, CanvasGroup canvasGroup)
    {
        if (!changable) return;
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
    #endregion



    private void StartControllerSetting()
    {
        UIManager.instance.ChangePanel(UIManager.PanelName.ControllerSettingPanel);  // UIをコントローラーへ
        ControllerSettingManager.instance.Initialize();  // 表示の初期設定
        GameManager.instance.StartWaitForControlls();  // 新規コントローラー待機状態へ以降
    }

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
