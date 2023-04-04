using System.Collections.Generic;
using UnityEngine;

public static class Option
{
    public static int MAX_NEXTS = 3;
    public static float CAMERA_DISTANCE = 20f;
    public static string BGM_VOLUME = "BGM_VOLUME";
    public static string SE_VOLUME = "SE_VOLUME";
    public static float FADE_DURATION = 0.25f;
    public static int ZONE_HEIGHT = 20;
    public static int ZONE_SIZE = 4;
    public static float GHOST_ALPHA = 0.2f;
    public static void Shuffle<T>(this IList<T> array)
    {
        for (var i = array.Count - 1; i > 0; --i)
        {
            // 0以上i以下のランダムな整数を取得
            // Random.Rangeの最大値は第２引数未満なので、+1することに注意
            var j = Random.Range(0, i + 1);

            // i番目とj番目の要素を交換する
            var tmp = array[i];
            array[i] = array[j];
            array[j] = tmp;
        }
    }

}