using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    private Camera playerCamera;
    private Vector2 deltaVelocity;

    public Vector2 velocity;

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        playerCamera.transform.RotateAround(playerCamera.transform.parent.position, Vector3.up, deltaVelocity.x * velocity.x * Time.deltaTime);
        Vector3 position = playerCamera.transform.localPosition + Vector3.up * deltaVelocity.y * velocity.y * Time.deltaTime;
        position.y = Mathf.Clamp(position.y, 0f, 10f);
        playerCamera.transform.localPosition = position;
    }

    public void SetVelocity(Vector2 velocity) => this.deltaVelocity = velocity;

    public Vector3 GetForwardVector()
    {
        // 向きを取得
        Vector3 forwardVector = playerCamera.transform.parent.position - playerCamera.transform.position;

        // X方向のほうが大きい
        if (Mathf.Abs(forwardVector.x) > Mathf.Abs(forwardVector.z))
        {
            if (forwardVector.x > 0)
                return Vector3.right;
            return Vector3.left;
        }

        // Z方向のほうが大きい
        if (forwardVector.z > 0)
            return Vector3.forward;
        return Vector3.back;
    }

    public void Shake(bool strong)
    {
        float intensity = strong ? 0.2f : 0.1f;
        playerCamera.transform.parent.DOShakePosition(Option.FADE_DURATION, intensity, 100, fadeOut: false);
    }

    public float GetHeight() => playerCamera.transform.localPosition.y + 3f;

}
