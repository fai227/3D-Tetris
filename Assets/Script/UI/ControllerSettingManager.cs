using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ControllerSettingManager : MonoBehaviour
{
    public static ControllerSettingManager instance;

    [SerializeField] private GameObject[] keyboardImages;
    [SerializeField] private GameObject[] controllerImages;
    [SerializeField] private GameObject[] playerObjects;
    [SerializeField] private Text[] playerNameTexts;
    [SerializeField] private GameObject joinImage;

    private void Awake()
    {
        instance = this;
    }

    public void SetInformation(int playerNum, int whoIsKeyboard = -1)
    {
        // 全員の設定
        for (int i = 0; i < playerObjects.Length; i++)
        {
            playerObjects[i].SetActive(i < playerNum);
            bool isKeyboard = whoIsKeyboard == i;
            keyboardImages[i].SetActive(isKeyboard);
            controllerImages[i].SetActive(!isKeyboard);
            playerNameTexts[i].text = (i + 1).ToString() + "P";
        }

        joinImage.SetActive(playerNum < 4);
    }

    public void SetReady(GameObject playerObject, bool isReady)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int index = Array.IndexOf(players, playerObject);

        if (index < 0) return;

        playerNameTexts[index].text = isReady ? "Ready" : (index + 1).ToString() + "P";

        // 全員のReadyチェック
        bool allReady = true;
        for (int i = 0; i < players.Length; i++)
        {
            if (playerNameTexts[i].text != "Ready")
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            GameManager.instance.GameStart();
        }
    }

    public void Initialize() => SetInformation(0, -1);
}
