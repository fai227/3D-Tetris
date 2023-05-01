using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DangerZoneController : MonoBehaviour
{
    [SerializeField] private GameObject dangerLinePrefab;
    private GameObject dangerLine;
    void Start()
    {
        StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine()
    {
        // 生成
        dangerLine = Instantiate(dangerLinePrefab);
        dangerLine.transform.position = transform.position;

        MeshRenderer meshRenderer = dangerLine.GetComponent<MeshRenderer>();

        // 初期設定
        dangerLine.transform.localPosition = new Vector3(dangerLine.transform.localPosition.x, -1f, dangerLine.transform.localPosition.z);
        meshRenderer.materials[0].color = new Color(1f, 1f, 1f, 0f);

        // フェード
        meshRenderer.materials[0].DOFade(1f, 0.5f);
        dangerLine.transform.DOLocalMoveY(transform.localPosition.y + transform.localScale.y / 2f, Option.DANGER_LINE_DURATION);

        yield return new WaitForSeconds(Option.DANGER_LINE_DURATION - 1f);

        meshRenderer.materials[0].DOFade(0f, 0.5f);

        yield return new WaitForSeconds(1f);

        Destroy(dangerLine);

        StartCoroutine(Coroutine());
    }

    private void OnDestroy()
    {
        if (dangerLine != null) Destroy(dangerLine);
    }
}
