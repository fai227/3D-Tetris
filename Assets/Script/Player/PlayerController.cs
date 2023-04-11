using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    public CameraController cameraController;
    public UIController uIController;
    public PlayerInput playerInput;

    public bool isReady = false;
    private List<int> minoList = new List<int>();
    private GameObject currentMinoObject;
    private int currentMinoId;
    private int holdMinoId = -1;
    private bool alreadyHeld = false;
    public int score = 0;
    public int level = 1;
    public bool gameover = false;
    private SRS.T_Spin tSpin = SRS.T_Spin.None;
    private Coroutine dropCoroutine;
    private bool backToBack;
    private bool softDrop = false;
    private GameObject ghostMinoObject;
    private int movedTime = 0;
    private Transform[,,] zone = new Transform[Option.ZONE_SIZE, Option.ZONE_HEIGHT, Option.ZONE_SIZE];

    [Header("Positions")]
    [SerializeField] private Transform minoPoistion;
    private static Vector3 spawnPosition = new Vector3(1f, 20f, 1f);

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
            currentMinoObject.transform.position += new Vector3(0.5f, -0.5f, -0.5f);
        }
        else if (currentMinoId == (int)MinoManager.Mino.O)
        {
            currentMinoObject.transform.position += new Vector3(0.5f, -0.5f, -0.5f);
        }

        // ネクストを表示
        DisplayNextMinos();

        // 埋まってないかチェック
        if (!ValidZone(currentMinoObject))
        {
            gameover = true;
            Drop(true);
            return;
        }

        // ゴースト生成
        GenerateGhost();

        // 落とすコルーチンを開始
        dropCoroutine = StartCoroutine(SoftDropCoroutine());
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

    private void Drop(bool forceGameOver = false) => StartCoroutine(DropCalculateCoroutine(forceGameOver));

    private IEnumerator DropCalculateCoroutine(bool forceGameOver)
    {
        // 音を鳴らす
        AudioManager.instance.DropSound();

        // 自動で落とすコルーチンを停止
        StopCoroutine(dropCoroutine);

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
        if (!forceGameOver)
        {
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
        }

        int lineDeleted = deletedLine.Count;
        if (lineDeleted > 0)
        {
            // 効果音
            AudioManager.instance.AttackSound();
            if (lineDeleted == 4)

                // カメラシェイク
                cameraController.Shake(lineDeleted == 4);

            // 文字表示
            if (lineDeleted == 4)
            {
                AudioManager.instance.Cubis();
                Generate3DText(MinoManager.instance.cubisObject);
            }
            else if (tSpin != SRS.T_Spin.None)
            {
                if (lineDeleted == 1)
                {
                    AudioManager.instance.TSpinSingle();
                    Generate3DText(MinoManager.instance.tSpinSingle);
                }
                else if (lineDeleted == 2)
                {
                    AudioManager.instance.TSpinDouble();
                    Generate3DText(MinoManager.instance.tSpinDouble);
                }
                else
                {
                    AudioManager.instance.TSpinTriple();
                    Generate3DText(MinoManager.instance.tSpinTriple);
                }
            }

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

        if (gameover)
        {
            GameManager.instance.OnFinished(this);
        }
        else
        {
            if (lineDeleted < 4 && tSpin == SRS.T_Spin.None) backToBack = false;

            // スコア計算
            int addedScore = Option.GetScore(lineDeleted, tSpin, backToBack, level);
            AddScore(addedScore);

            // BackToBack判定
            if (lineDeleted == 4 || tSpin != SRS.T_Spin.None) backToBack = true;

            // ミノ生成
            GenerateNextMino();
        }
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

        GameObject cubisObject = Instantiate(textObject, cubisPisition + transform.position, Quaternion.Euler(0f, angle, 0f), transform);
    }

    private void GenerateTSpin3DText()
    {
        Vector3 position = currentMinoObject.transform.localPosition;
        Vector3 forward = cameraController.GetForwardVector();
        float angle = Mathf.Atan2(-forward.z, forward.x) * Mathf.Rad2Deg + 90f;

        // 端まで動かす
        while (true)
        {
            position -= forward;

            if (position.x < 0 || position.z < 0) break;
            if (position.x >= Option.ZONE_SIZE || position.z >= Option.ZONE_SIZE) break;
        }

        position += forward / 2f;

        GameObject threeDText = tSpin == SRS.T_Spin.T_Spin_Mini ? MinoManager.instance.tSpinMini : MinoManager.instance.tSpin;
        Instantiate(threeDText, minoPoistion.position + position, Quaternion.Euler(0f, angle, 0f), transform);
    }

    private void GenerateSoftDropEffect()
    {
        GameObject effectObject = Instantiate(currentMinoObject, currentMinoObject.transform.position, currentMinoObject.transform.rotation);

        // 色を変更
        foreach (Transform child in effectObject.transform)
        {
            child.GetComponent<MeshRenderer>().materials[0].DOFade(0f, 1f);
        }

        effectObject.transform.DOScale(Vector3.one * 1.1f, 0.5f).OnComplete(() =>
        {
            effectObject.transform.DOScale(Vector3.one, 0.5f).OnComplete(() => Destroy(effectObject));
        });
    }

    private void RotateMino(Vector3 angle)
    {
        if (currentMinoObject == null) return;

        // 元の角度を保存
        Quaternion originalRotation = currentMinoObject.transform.rotation;
        Vector3 originalPosition = currentMinoObject.transform.localPosition;

        // T-Spinを初期化
        tSpin = SRS.T_Spin.None;
        bool used5Rotation = false;
        bool oOrI = currentMinoId == (int)MinoManager.Mino.I || currentMinoId == (int)MinoManager.Mino.O;

        // 与えられた角度へ回転
        currentMinoObject.transform.rotation = Quaternion.Euler(angle) * currentMinoObject.transform.rotation;

        // 第一チェック失敗
        if (!ValidZone(currentMinoObject))
        {
            // 必要定数定義
            bool allFailed = true;
            Vector3 endPosition = originalRotation * Vector3.up;
            Vector2[] alphaMoves;

            // 回転軸がZの時
            if (Mathf.Abs(angle.z) > 45f)
            {
                // 回転の向き
                SRS.Direction direction = SRS.Direction.Left;
                if (angle.z < 0) direction = SRS.Direction.Right;

                // 重心がない場合のSRS取得
                if (!oOrI && Mathf.Abs(endPosition.x) < 0.1f && Mathf.Abs(endPosition.y) < 0.1f)
                {
                    Vector3 sidePosition = originalRotation * Vector3.forward;
                    if (sidePosition.x == 0) alphaMoves = SRS.GetHorizontalAlphaMoves(direction);
                    else alphaMoves = SRS.GetVerticalAlphaMoves(direction);
                }
                // 重心がある場合のSRS取得
                else
                {
                    int rotation = Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(endPosition.y, endPosition.x) * Mathf.Rad2Deg - 90f, 360f) / 90f);
                    alphaMoves = oOrI ? SRS.GetBetaMoves(rotation, direction) : SRS.GetAlphaMoves(rotation, direction);
                }

                // 次のテストを試していく
                for (int i = 0; i < alphaMoves.Length; i++)
                {
                    Vector2 srsDirection = alphaMoves[i];
                    Vector3 moveDirection = new Vector3(srsDirection.x, srsDirection.y, 0f);

                    // 回転を考慮して移動
                    currentMinoObject.transform.localPosition = originalPosition + moveDirection;

                    // 移動成功時
                    if (ValidZone(currentMinoObject))
                    {
                        if (i == alphaMoves.Length - 1)
                        {
                            used5Rotation = true;
                        }

                        allFailed = false;
                        break;
                    }

                    // 元の座標へ戻す
                    currentMinoObject.transform.localPosition = originalPosition;
                }
            }

            // 回転軸がXの時
            else if (Mathf.Abs(angle.x) > 45f)
            {
                // 回転の向き
                SRS.Direction direction = SRS.Direction.Left;
                if (angle.x < 0) direction = SRS.Direction.Right;

                // 重心がない場合のSRS取得
                if (!oOrI && Mathf.Abs(endPosition.y) < 0.1f && Mathf.Abs(endPosition.z) < 0.1f)
                {
                    Vector3 sidePosition = originalRotation * Vector3.forward;
                    if (sidePosition.z == 0) alphaMoves = SRS.GetHorizontalAlphaMoves(direction);
                    else alphaMoves = SRS.GetVerticalAlphaMoves(direction);
                }
                // 重心がある場合のSRS取得
                else
                {
                    int rotation = Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(endPosition.y, -endPosition.z) * Mathf.Rad2Deg - 90f, 360f) / 90f);
                    alphaMoves = oOrI ? SRS.GetBetaMoves(rotation, direction) : SRS.GetAlphaMoves(rotation, direction);
                }

                // 次のテストを試していく
                for (int i = 0; i < alphaMoves.Length; i++)
                {
                    Vector2 srsDirection = alphaMoves[i];
                    Vector3 moveDirection = new Vector3(0f, srsDirection.y, -srsDirection.x);

                    // 回転を考慮して移動
                    currentMinoObject.transform.localPosition = originalPosition + moveDirection;

                    // 移動成功時
                    if (ValidZone(currentMinoObject))
                    {
                        if (i == alphaMoves.Length - 1)
                        {
                            used5Rotation = true;
                        }

                        allFailed = false;
                        break;
                    }

                    // 元の座標へ戻す
                    currentMinoObject.transform.localPosition = originalPosition;
                }
            }
            // 回転軸がYの時
            else
            {
                Vector3 forward = cameraController.GetForwardVector();
                float cameraAngle = Mathf.Repeat(Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg, 360f);
                int rotation = Mathf.RoundToInt(Mathf.Repeat(Mathf.Atan2(endPosition.z, endPosition.x) * Mathf.Rad2Deg - 90f, 360f) / 90f);


                // 回転の向き
                SRS.Direction direction = SRS.Direction.Left;
                if (angle.y > 0) direction = SRS.Direction.Right;

                // 重心がない場合のSRS取得
                if (!oOrI && Mathf.Abs(endPosition.x) < 0.1f && Mathf.Abs(endPosition.z) < 0.1f)
                {
                    Vector3 sidePosition = originalRotation * Vector3.forward;

                    int checkNum = Mathf.RoundToInt(sidePosition.x);  // 奇数で縦
                    checkNum = (checkNum + Mathf.RoundToInt(cameraAngle / 90f) + 4) % 2;  // 奇数で縦

                    if (checkNum == 1) alphaMoves = SRS.GetVerticalAlphaMoves(direction);
                    else alphaMoves = SRS.GetHorizontalAlphaMoves(direction);
                }
                // 重心がある場合のSRS取得
                else
                {
                    rotation = (rotation + Mathf.RoundToInt(cameraAngle / 90f)) % 4;
                    alphaMoves = oOrI ? SRS.GetBetaMoves(rotation, direction) : SRS.GetAlphaMoves(rotation, direction);
                }

                // 次のテストを試していく
                for (int i = 0; i < alphaMoves.Length; i++)
                {
                    Vector2 srsDirection = alphaMoves[i];
                    Vector3 moveDirection = new Vector3(srsDirection.x, 0f, srsDirection.y);
                    moveDirection = Quaternion.Euler(0f, cameraAngle, 0f) * moveDirection;

                    // 回転を考慮して移動
                    currentMinoObject.transform.localPosition = originalPosition + moveDirection;

                    // 移動成功時
                    if (ValidZone(currentMinoObject))
                    {
                        if (i == alphaMoves.Length - 1)
                        {
                            used5Rotation = true;
                        }

                        allFailed = false;
                        break;
                    }

                    // 元の座標へ戻す
                    currentMinoObject.transform.localPosition = originalPosition;
                }
            }

            // すべてをテストした際は元の向きに直して終了
            if (allFailed)
            {
                currentMinoObject.transform.rotation = originalRotation;
                return;
            }

        }

        // T-Spin判定
        if (currentMinoId == (int)MinoManager.Mino.T)
        {
            // 5テストでT-Spin判定されているときは除く
            if (tSpin == SRS.T_Spin.None)
            {
                // 端のミノを入れるリスト
                List<Vector3> openPos = new();
                List<Vector3> abcd = new()
                {
                    currentMinoObject.transform.rotation * new Vector3(-1f, 1f, 0f),
                    currentMinoObject.transform.rotation * new Vector3(1f, 1f, 0f),
                    currentMinoObject.transform.rotation * new Vector3(-1f, -1f, 0f),
                    currentMinoObject.transform.rotation * new Vector3(1f, -1f, 0f),
                };

                // 端が埋まってるかのチェック
                foreach (Vector3 checkPosition in abcd)
                {
                    int x = Mathf.RoundToInt(currentMinoObject.transform.localPosition.x + checkPosition.x);
                    int y = Mathf.RoundToInt(currentMinoObject.transform.localPosition.y + checkPosition.y);
                    int z = Mathf.RoundToInt(currentMinoObject.transform.localPosition.z + checkPosition.z);

                    if (x < 0) continue;
                    if (y < 0) continue;
                    if (z < 0) continue;

                    if (x >= Option.ZONE_SIZE) continue;
                    if (y >= Option.ZONE_HEIGHT) continue;
                    if (z >= Option.ZONE_SIZE) continue;

                    if (zone[x, y, z] == null) openPos.Add(checkPosition);
                }

                // T-Spinの可能性あり
                if (openPos.Count <= 1)
                {
                    if (openPos.Contains(abcd[0]) || openPos.Contains(abcd[1]))
                    {
                        tSpin = SRS.T_Spin.T_Spin_Mini;
                    }
                    else
                    {
                        tSpin = SRS.T_Spin.T_Spin;
                        // 第5テストが行われている場合は、Miniを昇格
                        if (used5Rotation) tSpin = SRS.T_Spin.T_Spin;
                    }
                }
            }
        }
        else
        {
            tSpin = SRS.T_Spin.None;
        }

        movedTime++;

        // ゴースト
        GenerateGhost();

        // T-Spin判定
        if (tSpin != SRS.T_Spin.None) // T-Spinの時は音を変える
        {
            GenerateTSpin3DText(); // UI表示
            AudioManager.instance.PlugSound();
        }
        else
        {
            AudioManager.instance.RotateSound();
        }
    }

    private void MoveMino(Vector3 vector)
    {
        if (currentMinoObject == null) return;

        tSpin = SRS.T_Spin.None;

        currentMinoObject.transform.position += vector;
        if (!ValidZone(currentMinoObject))
        {
            currentMinoObject.transform.position -= vector;
        }
        else
        {
            movedTime++;
            GenerateGhost();
            AudioManager.instance.MoveSound();
        }
    }

    private void AddScore(int value)
    {
        score += value;
        uIController.SetScore(score);
    }

    private IEnumerator SoftDropCoroutine()
    {
        int heighestY = Mathf.RoundToInt(currentMinoObject.transform.localPosition.y);
        float elapsedTime = 0f;

        // 落とすためのループ
        while (true)
        {
            float duration = Mathf.Pow(0.8f - ((level - 1) * 0.007f), level - 1);
            bool previous = softDrop;

            // フレームチェック
            while (true)
            {
                // 変わったかチェック
                if (previous != softDrop)
                {
                    previous = softDrop;
                    elapsedTime = 0f;
                }

                // 時間経過を測る
                if (softDrop)
                {
                    if (elapsedTime > duration / Option.SOFT_DROP_RATIO)
                    {
                        elapsedTime -= duration / Option.SOFT_DROP_RATIO;
                        break;
                    }
                }
                else
                {
                    if (elapsedTime > duration)
                    {
                        elapsedTime -= duration;
                        break;
                    }
                }

                yield return null;
                elapsedTime += Time.deltaTime;
            }

            // 一つ下に落とす
            currentMinoObject.transform.localPosition += Vector3.down;

            // 下に動かせないときはロックダウン待ち
            if (!ValidZone(currentMinoObject))
            {
                currentMinoObject.transform.localPosition -= Vector3.down;

                // 最大回転数を超えている場合は、即設置
                if (movedTime > Option.MAX_LOCK_DOWN_MOVES)
                {
                    break;
                }
                // ロックダウン待ち
                else
                {
                    int previousMovedTime = movedTime;

                    // ロックダウン待ち
                    yield return new WaitForSeconds(Option.LOCK_DOWN_TIME);

                    // 0.5秒前から動いているかチェック
                    if (movedTime == previousMovedTime)
                    {
                        break;
                    }
                }
            }
            else
            {
                int nowHeightY = Mathf.RoundToInt(currentMinoObject.transform.localPosition.y);
                if (nowHeightY < heighestY)
                {
                    heighestY = nowHeightY;
                    movedTime = 0;
                }
            }
        }

        GenerateSoftDropEffect();
        Drop();
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

    private void OnSoftDrop(InputValue inputValue)
    {
        float value = inputValue.Get<float>();
        if (value > 0.5f)
        {
            softDrop = true;
        }
        else
        {
            softDrop = false;
        }
    }

    private void OnHardDrop()
    {
        if (currentMinoObject == null) return;
        int height = 0;
        while (ValidZone(currentMinoObject))
        {
            currentMinoObject.transform.position += Vector3.down;
            height++;
        }
        currentMinoObject.transform.position -= Vector3.down;
        height--;

        // 落とした後のエフェクト生成
        Instantiate(MinoManager.instance.dropParticle).transform.position = currentMinoObject.transform.position - Vector3.down;
        AddScore(height * Option.HARD_DROP_SCORE);

        Drop();
    }

    private void OnHold()
    {
        if (currentMinoObject == null) return;
        if (alreadyHeld) return;
        alreadyHeld = true;

        // 自動で落とすコルーチンを停止
        StopCoroutine(dropCoroutine);

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
        // ゲーム開始後かつゲームオーバーでなければポーズする
        if (GameManager.isGameStarted && !gameover)
        {
            // ミノがないときはポーズできない
            if (currentMinoObject == null) return;

            if (GameManager.isPaused)
            {
                GameManager.instance.Resume();
            }
            else
            {
                GameManager.instance.Pause(this);
            }
            return;
        }

        // ゲーム開始前は準備完了にする
        ControllerSettingManager.instance.SetReady(gameObject, true);
        isReady = true;
    }

    private void OnQuit()
    {
        // ゲーム開始後かつゲームオーバーでなければポーズする
        if (GameManager.isGameStarted)
        {
            if (!GameManager.isPaused)
            {
                GameManager.instance.Pause(this);
            }
            else
            {
                GameManager.instance.EndGame();
            }
        }

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
