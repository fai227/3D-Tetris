using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UIController uIController;

    public bool isReady = false;
    private List<int> minoList = new List<int>();
    private GameObject currentMinoObject;
    private int currentMinoId;
    private int holdMinoId = -1;
    private bool alreadyHeld = false;
    private GameObject ghostMinoObject;
    private Transform[,,] zone = new Transform[Option.ZONE_SIZE, Option.ZONE_HEIGHT, Option.ZONE_SIZE];

    [Header("Positions")]
    [SerializeField] private Transform minoPoistion;
    private static Vector3 spawnPosition = new Vector3(1f, 15f, 1f);

    private void Start()
    {
        // ミノ初期化
        AddMino();
    }

    public void DisplayNextMinos()
    {
        uIController.SetNext(minoList[0], minoList[1], minoList[2], minoList[3], minoList[4]);
    }

    public void GenerateNextMino()
    {
        // ミノを生成
        currentMinoId = minoList[0];
        minoList.RemoveAt(0);

        // ミノを追加する必要がある場合には追加
        if (minoList.Count <= MinoManager.instance.getMinoLength())
        {
            AddMino();
        }

        GenerateCurrentMino();

        alreadyHeld = false;
    }

    private void GenerateCurrentMino()
    {
        // ミノを生成
        currentMinoObject = Instantiate(MinoManager.instance.getMino(currentMinoId), minoPoistion.position + spawnPosition, Quaternion.identity, minoPoistion);
        if (currentMinoId == (int)MinoManager.Mino.I)
        {
            currentMinoObject.transform.position += new Vector3(0.5f, -0.5f, 0.5f);
        }
        else if (currentMinoId == (int)MinoManager.Mino.O)
        {
            currentMinoObject.transform.position += new Vector3(0.5f, -0.5f, 0.5f);
        }

        GenerateGhost();

        // ネクストを表示
        DisplayNextMinos();
    }

    private void AddMino()
    {
        List<int> tmpMinoList = new List<int>();
        int minoLength = MinoManager.instance.getMinoLength();
        for (int i = 0; i < minoLength; i++) tmpMinoList.Add(i);
        tmpMinoList.Shuffle();
        minoList.AddRange(tmpMinoList);
    }

    private bool ValidZone(GameObject minoObject)
    {
        foreach (Transform child in minoObject.transform)
        {
            Vector3 childPosition = minoObject.transform.localPosition + (child.transform.position - minoObject.transform.position);
            int x = Mathf.RoundToInt(childPosition.x);
            int y = Mathf.RoundToInt(childPosition.y);
            int z = Mathf.RoundToInt(childPosition.z);

            // 範囲外チェック
            if (x < 0) return false;
            if (y < 0) return false;
            if (z < 0) return false;

            if (x >= Option.ZONE_SIZE) return false;
            if (y >= Option.ZONE_HEIGHT) return false;
            if (z >= Option.ZONE_SIZE) return false;

            // 置いてあるかチェック
            if (zone[x, y, z] != null)
            {
                return false;
            }
        }

        return true;
    }

    public static void UnreadyAll()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) player.GetComponent<PlayerController>().isReady = false;
    }

    private void Drop() => StartCoroutine(DropCoroutine());

    private IEnumerator DropCoroutine()
    {
        // 音を鳴らす
        AudioManager.instance.DropSound();

        // ミノを登録する
        foreach (Transform child in currentMinoObject.transform)
        {
            Vector3 childPosition = currentMinoObject.transform.localPosition + (child.transform.position - currentMinoObject.transform.position);
            int x = Mathf.RoundToInt(childPosition.x);
            int y = Mathf.RoundToInt(childPosition.y);
            int z = Mathf.RoundToInt(childPosition.z);

            zone[x, y, z] = child;
        }
        currentMinoObject = null;

        // ゴースト削除
        if (ghostMinoObject != null) Destroy(ghostMinoObject);

        // そろっているかチェック
        List<int> deletedLine = new();
        for (int y = 0; y < Option.ZONE_HEIGHT; y++)
        {
            bool allFilled = true;
            for (int x = 0; x < Option.ZONE_SIZE; x++)
            {
                if (!allFilled) break;
                for (int z = 0; z < Option.ZONE_SIZE; z++)
                {
                    if (zone[x, y, z] == null)
                    {
                        allFilled = false;
                        break;
                    }
                }
            }

            // すべて埋まっている
            if (allFilled)
            {
                deletedLine.Add(y);
                // オブジェクトをすべて削除
                for (int x = 0; x < Option.ZONE_SIZE; x++)
                {
                    for (int z = 0; z < Option.ZONE_SIZE; z++)
                    {
                        Transform child = zone[x, y, z];

                        // 最後のミノの場合は親を破壊
                        if (child.parent.childCount <= 1)
                        {
                            Destroy(child.parent.gameObject);
                        }
                        // それ以外はミノを破壊
                        else
                        {
                            Destroy(child.gameObject);
                        }
                    }
                }

                // エフェクトを生成
                GameObject deleteEffect = Instantiate(MinoManager.instance.deleteEffect, minoPoistion.position + new Vector3(1.5f, y, 1.5f), Quaternion.identity, minoPoistion);
            }
        }

        int lineDeleted = deletedLine.Count;
        if (lineDeleted > 0)
        {
            // 効果音
            AudioManager.instance.AttackSound();
            if (lineDeleted == 4) AudioManager.instance.Cubis();

            // カメラシェイク
            cameraController.Shake(lineDeleted == 4);

            // 文字表示
            if (lineDeleted == 4) Generate3DText(MinoManager.instance.cubisObject);

            // エフェクト待機
            yield return new WaitForSeconds(0.75f);

            // 消えたラインの上のブロックをすべて落とす
            for (int i = 0; i < lineDeleted; i++)
            {
                int line = deletedLine[i];

                // ブロックを下に落とす
                for (int x = 0; x < Option.ZONE_SIZE; x++)
                {
                    for (int z = 0; z < Option.ZONE_SIZE; z++)
                    {
                        // 上のブロックをひとつずつ下に下げる
                        for (int y = line; y < Option.ZONE_HEIGHT - 1; y++)
                        {
                            Transform above = zone[x, y + 1, z];
                            if (above != null) above.transform.position += Vector3.down;
                            zone[x, y, z] = above;
                        }
                        // 一番上をnullにする
                        zone[x, Option.ZONE_HEIGHT - 1, z] = null;
                    }
                }

                // 高さを1つ下げる
                for (int s = i + 1; s < lineDeleted; s++)
                {
                    deletedLine[s]--;
                }
            }
        }

        // 少し待ってから次のミノを生成
        yield return new WaitForSeconds(0.2f);
        GenerateNextMino();
    }

    private void GenerateGhost()
    {
        // ひとつ前を削除
        Destroy(ghostMinoObject);

        // ゴースト生成
        ghostMinoObject = Instantiate(currentMinoObject, currentMinoObject.transform.parent);

        // 色を変更
        foreach (Transform child in ghostMinoObject.transform)
        {
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();

            Color materialColor = meshRenderer.materials[0].color;
            materialColor.a = Option.GHOST_ALPHA;
            meshRenderer.materials[0].color = materialColor;
        }

        // アウトライン設定
        Outline outline = ghostMinoObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = MinoManager.GetMinoColor(currentMinoId);
        outline.OutlineWidth = 10f;

        // 下に下げる
        while (ValidZone(ghostMinoObject))
        {
            ghostMinoObject.transform.position += Vector3.down;
        }
        ghostMinoObject.transform.position -= Vector3.down;
    }

    private void Generate3DText(GameObject textObject, bool backToBack = false)
    {
        Vector3 center = new Vector3(2f, cameraController.GetHeight(), 2f);
        if (backToBack) center.y -= 1f;

        Vector3 forward = cameraController.GetForwardVector();
        Vector3 cubisPisition = center - new Vector3(2f * forward.x, 0f, 2f * forward.z);
        float angle = Mathf.Atan2(-forward.z, forward.x) * Mathf.Rad2Deg + 90f;

        GameObject cubisObject = Instantiate(MinoManager.instance.cubisObject, cubisPisition + transform.position, Quaternion.Euler(0f, angle, 0f), transform);
    }

    private void RotateMino(Vector3 angle)
    {
        if (currentMinoObject == null) return;

        // 元の角度を保存
        Quaternion originalRotation = currentMinoObject.transform.rotation;
        Vector3 originalPosition = currentMinoObject.transform.localPosition;

        // 与えられた角度へ回転
        currentMinoObject.transform.rotation = Quaternion.Euler(angle) * currentMinoObject.transform.rotation;

        // 第一チェック失敗
        if (!ValidZone(currentMinoObject))
        {
            bool allFailed = true;

            Vector3 endPosition = originalRotation * Vector3.up;
            Vector2[] alphaMoves;

            // 回転軸がZの時
            if (Mathf.Abs(angle.z) > 45f)
            {
                // 回転の向き
                SRS.Direction direction = SRS.Direction.Left;
                if (angle.z < 0) direction = SRS.Direction.Right;

                // 重心がない場合
                if (Mathf.Abs(endPosition.x) < 0.1f && Mathf.Abs(endPosition.y) < 0.1f)
                {
                    Debug.Log("重心がないチェック");
                }
                else
                {
                    int rotation = Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(endPosition.y, endPosition.x) * Mathf.Rad2Deg - 90f, 360f) / 90f);
                    alphaMoves = SRS.GetAlphaMoves(rotation, direction);

                    // 次のテストを試していく
                    foreach (Vector2 srsDirection in alphaMoves)
                    {
                        Vector3 moveDirection = new Vector3(srsDirection.x, srsDirection.y, 0f);

                        // 回転を考慮して移動
                        currentMinoObject.transform.localPosition = originalPosition + moveDirection;

                        // 移動成功時
                        if (ValidZone(currentMinoObject))
                        {
                            allFailed = false;
                            break;
                        }

                        // 元の座標へ戻す
                        currentMinoObject.transform.localPosition = originalPosition;
                    }
                }
            }

            // 回転軸がXの時
            else if (Mathf.Abs(angle.x) > 45f)
            {
                // 回転の向き
                SRS.Direction direction = SRS.Direction.Left;
                if (angle.x < 0) direction = SRS.Direction.Right;

                // 重心がない場合
                if (Mathf.Abs(endPosition.y) < 0.1f && Mathf.Abs(endPosition.z) < 0.1f)
                {
                    Debug.Log("重心がないチェック");
                }
                else
                {
                    int rotation = Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(endPosition.y, -endPosition.z) * Mathf.Rad2Deg - 90f, 360f) / 90f);
                    alphaMoves = SRS.GetAlphaMoves(rotation, direction);

                    // 次のテストを試していく
                    foreach (Vector2 srsDirection in alphaMoves)
                    {
                        Vector3 moveDirection = new Vector3(0f, srsDirection.y, -srsDirection.x);

                        // 回転を考慮して移動
                        currentMinoObject.transform.localPosition = originalPosition + moveDirection;

                        // 移動成功時
                        if (ValidZone(currentMinoObject))
                        {
                            allFailed = false;
                            break;
                        }

                        // 元の座標へ戻す
                        currentMinoObject.transform.localPosition = originalPosition;
                    }
                }
            }
            // 回転軸がYの時
            else
            {
                // 回転の向き
                SRS.Direction direction = SRS.Direction.Left;
                if (angle.x < 0) direction = SRS.Direction.Right;

                // 重心がない場合
                if (Mathf.Abs(endPosition.y) < 0.1f && Mathf.Abs(endPosition.z) < 0.1f)
                {
                    Debug.Log("重心がないチェック");
                }
                else
                {
                    int rotation = Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(endPosition.y, -endPosition.z) * Mathf.Rad2Deg - 90f, 360f) / 90f);
                    alphaMoves = SRS.GetAlphaMoves(rotation, direction);

                    // 次のテストを試していく
                    foreach (Vector2 srsDirection in alphaMoves)
                    {
                        Vector3 moveDirection = new Vector3(0f, srsDirection.y, -srsDirection.x);

                        // 回転を考慮して移動
                        currentMinoObject.transform.localPosition = originalPosition + moveDirection;

                        // 移動成功時
                        if (ValidZone(currentMinoObject))
                        {
                            allFailed = false;
                            break;
                        }

                        // 元の座標へ戻す
                        currentMinoObject.transform.localPosition = originalPosition;
                    }
                }
            }

            // すべてをテストした際は元の向きに直して終了
            if (allFailed)
            {
                currentMinoObject.transform.rotation = originalRotation;
                return;
            }

        }

        // ゴーストとサウンド
        GenerateGhost();
        AudioManager.instance.RotateSound();
    }

    private void MoveMino(Vector3 vector)
    {
        if (currentMinoObject == null) return;

        currentMinoObject.transform.position += vector;
        if (!ValidZone(currentMinoObject)) currentMinoObject.transform.position -= vector;
        else
        {
            GenerateGhost();
            AudioManager.instance.MoveSound();
        }
    }

    #region Controlls
    private void OnRotateClockwise()
    {
        Vector3 angle = cameraController.GetForwardVector() * -90f;
        RotateMino(angle);
    }

    private void OnRotateCounterClockwise()
    {
        Vector3 angle = cameraController.GetForwardVector() * 90f;
        RotateMino(angle);
    }

    private void OnRotateForward()
    {
        Vector3 forward = cameraController.GetForwardVector();
        Vector3 angle = new Vector3(forward.z, 0, -forward.x) * 90f;
        RotateMino(angle);
    }

    private void OnRotateBackward()
    {
        Vector3 forward = cameraController.GetForwardVector();
        Vector3 angle = new Vector3(-forward.z, 0, forward.x) * 90f;
        RotateMino(angle);
    }

    private void OnRotateRight()
    {
        Vector3 angle = new Vector3(0f, 90f, 0f);
        RotateMino(angle);
    }

    private void OnRotateLeft()
    {
        Vector3 angle = new Vector3(0f, -90f, 0f);
        RotateMino(angle);
    }

    private void OnMoveForward()
    {
        Vector3 forward = cameraController.GetForwardVector();
        MoveMino(forward);
    }

    private void OnMoveBackward()
    {
        Vector3 back = -cameraController.GetForwardVector();
        MoveMino(back);
    }

    private void OnMoveRight()
    {
        Vector3 forward = cameraController.GetForwardVector();
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        MoveMino(right);
    }

    private void OnMoveLeft()
    {
        Vector3 forward = cameraController.GetForwardVector();
        Vector3 left = new Vector3(-forward.z, 0, forward.x);
        MoveMino(left);
    }

    private void OnSoftDrop()
    {
        currentMinoObject.transform.position += Vector3.down;
        if (!ValidZone(currentMinoObject))
        {
            currentMinoObject.transform.position -= Vector3.down;
            Drop();
        }
    }

    private void OnHardDrop()
    {

        while (ValidZone(currentMinoObject))
        {
            currentMinoObject.transform.position += Vector3.down;
        }
        currentMinoObject.transform.position -= Vector3.down;

        // 落とした後のエフェクト生成
        Instantiate(MinoManager.instance.dropParticle).transform.position = currentMinoObject.transform.position - Vector3.down;

        Drop();
    }

    private void OnHold()
    {
        if (alreadyHeld) return;
        alreadyHeld = true;

        // ホールドミノがない場合
        if (holdMinoId == -1)
        {
            holdMinoId = currentMinoId;
            uIController.SetHold(holdMinoId);
            Destroy(currentMinoObject);
            GenerateNextMino();
        }
        // ホールドミノがある場合
        else
        {
            // 数値入れ替え
            (holdMinoId, currentMinoId) = (currentMinoId, holdMinoId);
            uIController.SetHold(holdMinoId);
            Destroy(currentMinoObject);
            GenerateCurrentMino();
        }
    }

    private void OnMove(InputValue inputValue)
    {
        cameraController.SetVelocity(inputValue.Get<Vector2>());
    }

    private void OnPause()
    {
        // ゲーム開始後はポーズする
        if (GameManager.isGameStarted)
        {

            return;
        }

        // ゲーム開始後は準備完了にする
        ControllerSettingManager.instance.SetReady(gameObject, true);
        isReady = true;
    }

    private void OnQuit()
    {
        // ゲーム開始前のみ可能
        if (GameManager.isGameStarted) return;

        // 準備完了中は準備完了を外す
        if (isReady)
        {
            ControllerSettingManager.instance.SetReady(gameObject, false);
            isReady = false;
            return;
        }

        // 準備完了してなければ削除
        Destroy(gameObject);

    }
    #endregion
}
