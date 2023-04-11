using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class LongPressedRapidfireInteraction : IInputInteraction
{
#if UNITY_EDITOR
    static LongPressedRapidfireInteraction()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        InputSystem.RegisterInteraction<LongPressedRapidfireInteraction>();
    }

    [Tooltip("連射と判断する間隔")]
    public float duration = 0.2f;
    [Tooltip("連射開始後の間隔")]
    public float interval = 0.1f;
    [Tooltip("ボタンを押したと判断するしきい値")]
    public float pressPoint = 0.5f;

    private bool isPressed = false;

    public void Process(ref InputInteractionContext context)
    {
        if (context.timerHasExpired)
        {
            if (isPressed)
            {
                context.Performed();
                context.SetTimeout(interval);
                return;
            }
        }

        if (context.ControlIsActuated(pressPoint))
        {
            if (isPressed) return;

            context.Performed();
            context.SetTimeout(duration);
            isPressed = true;
            return;

        }

        isPressed = false;
    }

    public void Reset()
    {

    }
}