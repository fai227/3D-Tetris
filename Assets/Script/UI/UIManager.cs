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
    [SerializeField] private CanvasGroup usernamePanel;
    [SerializeField] private Image coverImage;

    [Header("Username")]
    [SerializeField] private Text usernameMessageText;
    [SerializeField] private InputField usernameInputField;

    public enum PanelName
    {
        TitlePanel, ControllerSettingPanel, GamePanel, UsernamePanel
    }

    private CanvasGroup previousPanel;

    private void Awake() => instance = this;

    private void Start()
    {
        // すべて非表示
        CanvasGroup[] canvasGroups = new CanvasGroup[] {
            titlePanel, controllerSettingPanel, gamePanel, usernamePanel
        };
        foreach (CanvasGroup canvasGroup in canvasGroups)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
        }

        // UIフェードイン
        coverImage.color = Color.black;
        coverImage.DOFade(0f, Option.FADE_DURATION).OnComplete(() =>
        {
            if (PlayerPrefs.HasKey(Option.USERNAME))
            {
                ChangePanel(PanelName.TitlePanel);
            }
            else
            {
                ChangePanel(PanelName.UsernamePanel);
            }
        });
    }

    public void ExitGame()
    {
        previousPanel.interactable = false;
        previousPanel.DOFade(0f, Option.FADE_DURATION).OnComplete(() =>
        {
            coverImage.DOFade(1f, Option.FADE_DURATION).OnComplete(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
                Application.Quit();//ゲームプレイ終了
#endif
            });
        });
    }

    public void ChangePanel(PanelName panelName)
    {
        CanvasGroup nextPanel = null;
        switch (panelName)
        {
            case PanelName.TitlePanel:
                nextPanel = titlePanel;
                break;

            case PanelName.ControllerSettingPanel:
                nextPanel = controllerSettingPanel;
                break;

            case PanelName.GamePanel:
                nextPanel = gamePanel;
                break;

            case PanelName.UsernamePanel:
                nextPanel = usernamePanel;
                break;
        }

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

        // ひとつ前のパネルがある場合はそれを非表示にしてからフェードイン
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

    public void OnSubmit()
    {
        UnityEngine.UI.Outline outline = usernameMessageText.GetComponentInChildren<UnityEngine.UI.Outline>();

        string username = usernameInputField.text;
        string filteredResult = WordFilter.Filter(username);

        if (filteredResult.Contains('*'))
        {
            usernameMessageText.DOFade(0f, Option.FADE_DURATION).OnComplete(() =>
            {
                outline.effectColor = Color.red;
                usernameMessageText.text = "The username contains\ninappropriate language: " + filteredResult;
                usernameMessageText.DOFade(1f, Option.FADE_DURATION);
            });
            return;
        }

        outline.effectColor = Color.white;
        usernameMessageText.text = "Enter username\nfor the leaderboards.";
        PlayerPrefs.SetString(Option.USERNAME, username);
        ChangePanel(PanelName.TitlePanel);
    }

}
