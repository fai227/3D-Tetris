using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ThreeDText : MonoBehaviour
{
    [SerializeField] private float duration;
    private Material material;
    void Start()
    {
        material = GetComponent<MeshRenderer>().materials[0];
        material.color = new Color(1f, 1f, 1f, 0f);
        material.DOFade(1f, Option.FADE_DURATION);
        DOVirtual.DelayedCall(duration + Option.FADE_DURATION, () =>
        {
            material.DOFade(0f, Option.FADE_DURATION);
            Destroy(gameObject, Option.FADE_DURATION);
        });
    }
}
