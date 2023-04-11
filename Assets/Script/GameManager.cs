using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;
using Dan.Main;

public class GameManager : MonoBehaviour
{
    public enum GameMode
    {
        FourtyLines, ScoreAttack, Marathon, Party
    }
    public static GameManager instance;
    public static string username;
    public static bool isGameStarted = false;
    public static bool isPaused = false;
    private List<PlayerController> players = new();
    private List<int> finishedPlayers = new();
    public static GameMode gameMode = GameMode.FourtyLines;

    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private GameObject titleObject;

    private Coroutine coroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        isGameStarted = false;
        isPaused = false;
    }

    public void StartWaitForControlls()
    {
        players.Clear();
        playerInputManager.enabled = true;
        playerInputManager.EnableJoining();
    }

    public void EndWaitForControlls()
    {
        playerInputManager.enabled = false;
    }

    public void GameStart() => coroutine = StartCoroutine(GameStartCoroutine());

    private IEnumerator GameStartCoroutine()
    {
        // コントローラー設定をやめる
        EndWaitForControlls();
        isGameStarted = true;

        // プレイヤー取得
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            players.Add(player.GetComponent<PlayerController>());
        }

        // 1秒待ってUIを変更
        yield return new WaitForSeconds(1f);
        UIManager.instance.ChangePanel(UIManager.PanelName.GamePanel);
        AudioManager.instance.Pause();

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

        // 曲再生
        AudioManager.instance.StartTetrisTheme();

        if (gameMode == GameMode.ScoreAttack) StartCoroutine(ScoreAttackCountdownCoroutine());
    }

    private IEnumerator ScoreAttackCountdownCoroutine()
    {
        for (int i = Option.SCORE_ATTACK_TIME; i >= 0; i--)
        {
            foreach (PlayerController playerController in players)
            {
                playerController.uIController.SetNumber(i);
            }
            yield return new WaitForSeconds(1f);
        }

        foreach (PlayerController playerController in players)
        {
            playerController.gameover = true;
        }
    }

    public void Pause(PlayerController playerController)
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioManager.instance.Pause();
        GameUIManager.instance.Pause(players.IndexOf(playerController));
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioManager.instance.Resume();
        GameUIManager.instance.Resume();
    }

    public void EndGame()
    {
        isPaused = false;
        isGameStarted = false;
        Time.timeScale = 1f;
        GameUIManager.instance.Resume();

        titleObject.SetActive(true);

        // キーマウ設定
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AudioManager.instance.SetNormalBGM();
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) Destroy(player);
        UIManager.instance.ChangePanel(UIManager.PanelName.TitlePanel);
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

        // タイトル設定
        titleObject.SetActive(playerLength == 0);

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

    public void OnFinished(PlayerController playerController)
    {
        int index = players.IndexOf(playerController);
        if (!finishedPlayers.Contains(index))
            finishedPlayers.Add(index);

        // ゲーム終了判定
        if (gameMode == GameMode.Party)
        {
            if (players.Count - 1 > finishedPlayers.Count) return;
        }
        else
        {
            if (players.Count > finishedPlayers.Count) return;
        }

        isGameStarted = false;

        List<PlayerController> winPlayers = new();
        switch (gameMode)
        {
            // 初めにクリアした人が勝ち
            case GameMode.FourtyLines:
                {
                    PlayerController winner = players[finishedPlayers[0]];
                    winPlayers.Add(winner);
                    break;
                }

            // 生き残った人が勝ち
            case GameMode.Party:
                {
                    foreach (PlayerController player in players)
                    {
                        if (!player.gameover)
                        {
                            winPlayers.Add(player);
                            break;
                        }
                    }
                    break;
                }

            // その他はスコアが一番高い人が勝ち
            default:
                {
                    int highestScore = -1;

                    // 一番高い数字を取得
                    foreach (PlayerController player in players)
                    {
                        if (player.score > highestScore) highestScore = player.score;
                    }

                    // 一番高い数値の人を勝者に認定
                    foreach (PlayerController player in players)
                    {
                        if (player.score == highestScore)
                        {
                            winPlayers.Add(player);
                        }
                    }
                    break;
                }
        }

        List<int> winners = new();
        foreach (PlayerController winPlayer in winPlayers)
        {
            winners.Add(players.IndexOf(winPlayer));
        }

        // 操作不可能に
        foreach (PlayerController player in players) player.playerInput.enabled = false;

        // 一応コルーチン停止
        StopCoroutine(coroutine);

        // キーマウ設定
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 結果表示
        GameUIManager.instance.ShowResult(winners);

        // ランキングに反映
        if (gameMode == GameMode.FourtyLines)
        {

        }
        else if (gameMode == GameMode.ScoreAttack)
        {
            try
            {
                LeaderboardCreator.UploadNewEntry(Option.SEACRET_KEY_FOR_SCORE_ATTACK, username, players[winners[0]].score, (message) =>
                {
                    TitleManager.instance.UpdateLeaderboard(false);
                });
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
            }
        }
    }
}
