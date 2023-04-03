using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DeleteEffect : MonoBehaviour
{
    [SerializeField] private GameObject lineObject;
    private void Start()
    {
        lineObject.transform.DOScale(lineObject.transform.localScale * 1.1f, 0.75f).OnComplete(() =>
        {
            lineObject.GetComponent<MeshRenderer>().materials[0].DOFade(0f, Option.FADE_DURATION);
            Destroy(gameObject, Option.FADE_DURATION);
        });
    }
}
