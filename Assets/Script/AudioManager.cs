using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip tetrisIntro;
    [SerializeField] private AudioClip tetrisTheme;

    [Header("SE")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip cubisAttackClip;
    [SerializeField] private AudioClip cubisClip;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioClip rotateSound;
    [SerializeField] private AudioClip moveSound;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetNextBGM();
    }

    public void SetNextBGM()
    {
        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    public void StartTetrisTheme() => StartCoroutine(TetrisThemeCoroutine());
    private IEnumerator TetrisThemeCoroutine()
    {
        float bgmVolume = bgmSource.volume;
        yield return bgmSource.DOFade(0f, 1f).WaitForCompletion();

        bgmSource.clip = tetrisTheme;
        bgmSource.volume = bgmVolume;

        yield return new WaitForSeconds(3f);
        bgmSource.PlayOneShot(tetrisIntro);
        bgmSource.PlayDelayed(tetrisIntro.length);
    }

    public void AttackSound() => seSource.PlayOneShot(attackClip);

    public void Cubis()
    {
        DOVirtual.DelayedCall(0.1f, () =>
        {
            seSource.PlayOneShot(cubisAttackClip);
        });
        DOVirtual.DelayedCall(0.5f, () =>
        {
            seSource.PlayOneShot(cubisClip);
        });
    }
    public void DropSound() => seSource.PlayOneShot(dropSound);
    public void MoveSound() => seSource.PlayOneShot(moveSound);
    public void RotateSound() => seSource.PlayOneShot(rotateSound);


    public void SetSEVolume(float se) => seSource.volume = se * se;
    public void SetBGMVolume(float bgm) => bgmSource.volume = bgm * bgm;
}
