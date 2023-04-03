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


    public void Process(ref InputInteractionContext context)
    {

        //連射判断タイミングの処理
        if (context.timerHasExpired)
        {
            context.Canceled();
            if (context.ControlIsActuated(pressPoint))
            {
                //context.Started();
                //context.Performed();
                context.SetTimeout(duration);
            }
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Waiting:
                if (context.ControlIsActuated(pressPoint))
                {
                    //実行状態にする
                    //context.Started();
                    context.Performed();
                    //連射の振る舞いをするタイミング
                    context.SetTimeout(duration);
                }
                break;

            case InputActionPhase.Performed:

                if (!context.ControlIsActuated(pressPoint))
                {
                    //ボタンが離れたらキャンセルにする。
                    context.Canceled();
                    return;
                }

                break;
        }
    }

    public void Reset()
    {

    }
}