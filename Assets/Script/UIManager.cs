using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Panels")]
    [SerializeField] private CanvasGroup titlePanel;
    [SerializeField] private CanvasGroup controllerSettingPanel;
    [SerializeField] private CanvasGroup gamePanel;
    [SerializeField] private Image coverImage;

    public enum PanelName
    {
        TitlePanel, ControllerSettingPanel, GamePanel
    }

    private CanvasGroup previousPanel;

    [Header("Options")]
    [SerializeField] private Slider seSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("UI")]
    [SerializeField] private Text countDownText;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // すべて非表示
        CanvasGroup[] canvasGroups = new CanvasGroup[] {
            titlePanel, controllerSettingPanel, gamePanel
        };
        foreach (CanvasGroup canvasGroup in canvasGroups)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }

        // UIに反映
        float seVolume = PlayerPrefs.GetFloat(Option.SE_VOLUME, 1f);
        float bgmVolume = PlayerPrefs.GetFloat(Option.BGM_VOLUME, 1f);
        bool fullscreen = Screen.fullScreen;

        seSlider.value = seVolume;
        bgmSlider.value = bgmVolume;
        fullscreenToggle.isOn = fullscreen;

        // UIフェードイン
        coverImage.color = Color.black;
        FadeIn(null);
        ChangePanel(titlePanel);
    }

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

    public void OnEndlessSelected()
    {
        StartControllerSetting();
    }

    public void OnPartySelected()
    {
        StartControllerSetting();
    }

    public void OnBackSelected()
    {
        ChangePanel(titlePanel);
    }

    private void StartControllerSetting()
    {
        ChangePanel(controllerSettingPanel);  // UIをコントローラーへ
        ControllerSettingManager.instance.Initialize();  // 表示の初期設定
        GameManager.instance.StartWaitForControlls();  // 新規コントローラー待機状態へ以降

    }

    public void OnExitSelected()
    {
        UnityAction unityAction = () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
    Application.Quit();//ゲームプレイ終了
#endif
        };

        titlePanel.interactable = false;
        titlePanel.DOFade(0f, Option.FADE_DURATION).OnComplete(() => FadeOut(unityAction));
    }

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












    #region 総合UI
    public void FadeIn(UnityAction unityAction)
    {
        coverImage.DOFade(0f, Option.FADE_DURATION).OnComplete(() =>
        {
            unityAction?.Invoke();
        });
    }

    public void FadeOut(UnityAction unityAction)
    {
        coverImage.DOFade(1f, Option.FADE_DURATION).OnComplete(() =>
        {
            unityAction?.Invoke();
        });
    }

    public void ChangePanel(CanvasGroup nextPanel)
    {
        UnityAction changeToNextPanel = () =>
        {
            if (nextPanel == null) return;

            previousPanel = nextPanel;
            nextPanel.gameObject.SetActive(true);
            nextPanel.DOFade(1f, Option.FADE_DURATION).OnComplete(() =>
            {
                nextPanel.interactable = true;
                nextPanel.blocksRaycasts = true;

                // UI選択
                Selectable[] selectables = nextPanel.GetComponentsInChildren<Selectable>();
                if (selectables.Length > 0) EventSystem.current.SetSelectedGameObject(selectables[0].gameObject);
            });
        };
        // ひとつ前のパネルがある場合はそれを非行事にしてからフェードイン
        if (previousPanel != null)
        {
            previousPanel.gameObject.SetActive(true);
            previousPanel.interactable = false;
            previousPanel.blocksRaycasts = false;
            previousPanel.DOFade(0f, Option.FADE_DURATION).OnComplete(() =>
            {
                changeToNextPanel.Invoke();
            });
        }
        else
        {
            changeToNextPanel.Invoke();
        }
    }

    public void ChangePanel(PanelName panelName)
    {
        CanvasGroup canvasGroup = null;
        switch (panelName)
        {
            case PanelName.TitlePanel:
                canvasGroup = titlePanel;
                break;

            case PanelName.ControllerSettingPanel:
                canvasGroup = controllerSettingPanel;
                break;

            case PanelName.GamePanel:
                canvasGroup = gamePanel;
                break;
        }
        ChangePanel(canvasGroup);
    }
    #endregion
}
