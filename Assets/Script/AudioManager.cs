using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private static float bgmVolume = 1f;
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
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioClip rotateSound;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip plugSound;

    [Header("Voide")]
    [SerializeField] private AudioClip cubisClip;
    [SerializeField] private AudioClip tSpinSingle;
    [SerializeField] private AudioClip tSpinDouble;
    [SerializeField] private AudioClip tSpinTriple;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetNormalBGM();
    }

    public void SetNormalBGM()
    {
        bgmSource.clip = bgmClip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void Pause()
    {
        bgmSource.DOFade(0f, Option.FADE_DURATION).SetUpdate(true);
    }

    public void Resume()
    {
        bgmSource.UnPause();
        bgmSource.DOFade(bgmVolume, Option.FADE_DURATION).SetUpdate(true);
    }

    public void StartTetrisTheme()
    {
        bgmSource.Pause();
        bgmSource.clip = tetrisTheme;
        bgmSource.volume = bgmVolume;

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
    public void TSpinSingle() => seSource.PlayOneShot(tSpinSingle);
    public void TSpinDouble() => seSource.PlayOneShot(tSpinDouble);
    public void TSpinTriple() => seSource.PlayOneShot(tSpinTriple);
    public void DropSound() => seSource.PlayOneShot(dropSound);
    public void MoveSound() => seSource.PlayOneShot(moveSound);
    public void RotateSound() => seSource.PlayOneShot(rotateSound);
    public void PlugSound() => seSource.PlayOneShot(plugSound);

    public void SetSEVolume(float se) => seSource.volume = se * se;
    public void SetBGMVolume(float bgm)
    {
        bgmVolume = bgm * bgm;
        bgmSource.volume = bgmVolume;
    }
}
