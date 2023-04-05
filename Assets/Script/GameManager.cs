using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum GameMode
    {
        FourtyLines, ScoreAttack, Marathon, Party
    }
    public static GameManager instance;
    public static bool isGameStarted = false;
    public static bool isPaused = false;

    [SerializeField] private PlayerInputManager playerInputManager;

    private void Awake()
    {
        instance = this;
    }

    public void StartWaitForControlls()
    {
        playerInputManager.enabled = true;
    }

    public void EndWaitForControlls()
    {
        playerInputManager.enabled = false;
    }

    public void GameStart() => StartCoroutine(GameStartCoroutine());

    private IEnumerator GameStartCoroutine()
    {
        // コントローラー設定をやめる
        EndWaitForControlls();
        isGameStarted = true;

        // 曲再生
        AudioManager.instance.StartTetrisTheme();

        // プレイヤー取得
        List<PlayerController> players = new();
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            players.Add(player.GetComponent<PlayerController>());
        }

        // 1秒待ってUIを変更
        yield return new WaitForSeconds(1f);
        UIManager.instance.ChangePanel(UIManager.PanelName.GamePanel);

        // ネクスト表示
        foreach (PlayerController player in players)
        {
            player.DisplayNextMinos();
        }

        // カウントダウン開始
        GameUIManager.instance.StartCountDown();
        yield return new WaitForSeconds(4f);

        // ゲームスタート
        foreach (PlayerController player in players)
        {
            player.GenerateNextMino();
        }
    }


    private void OnPlayerJoined(PlayerInput input)
    {
        SetPlayers(null);  // カメラ配置を設定
    }

    private void OnPlayerLeft(PlayerInput input)
    {
        SetPlayers(input.gameObject);
    }

    private void SetPlayers(GameObject exeptPlayerObject)
    {
        // 全員を解除
        PlayerController.UnreadyAll();

        // プレイヤー取得
        List<GameObject> players = new List<GameObject>();
        players.AddRange(GameObject.FindGameObjectsWithTag("Player"));

        // 除外するプレイヤーを抜く
        int index = players.IndexOf(exeptPlayerObject);
        if (index >= 0) players.RemoveAt(index);

        int playerLength = players.Count;

        // カメラ設定
        for (int i = 0; i < playerLength; i++)
        {
            players[i].transform.position = new Vector3(1000 * (i + 1), 0, 0);
            float width = 1f / playerLength;
            Rect cameraRect = new Rect(width * i, 0, width, 1);
            players[i].GetComponentInChildren<Camera>().rect = cameraRect;
        }

        // UI設定
        int keyboard = -1;
        for (int i = 0; i < playerLength; i++)
        {
            if (players[i].GetComponent<PlayerInput>().currentControlScheme == "Keyboard")
            {
                keyboard = i;
                break;
            }
        }
        // キーボードがない
        if (keyboard == -1)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        // キーボードがある
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        ControllerSettingManager.instance.SetInformation(playerLength, keyboard);
    }


}
