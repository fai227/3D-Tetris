using System.Collections.Generic;
using UnityEngine;

public static class Option
{
    public static int MAX_NEXTS = 3;
    public static float CAMERA_DISTANCE = 20f;
    public static string BGM_VOLUME = "BGM_VOLUME";
    public static string SE_VOLUME = "SE_VOLUME";
    public static float FADE_DURATION = 0.25f;
    public static int ZONE_HEIGHT = 40;
    public static int ZONE_SIZE = 4;
    public static float GHOST_ALPHA = 0.2f;
    public static string USERNAME = "USERNAME";
    public static string SEACRET_KEY_FOR_40_LINES = "4cd5899c245cc90d96476a1cd973b8b34d05b11ee75bbf6b6eaa12546984c4a3";
    public static string SEACRET_KEY_FOR_SCORE_ATTACK = "e61d12cd6f1b603bc7a2822c997197a8ac310a24899403f2a1656f3914a6c433";
    public static int SCORE_ATTACK_TIME = 200;
    public static int LINE_OF_40_LINES = 40;
    public static int SOFT_DROP_SCORE = 1;
    public static int HARD_DROP_SCORE = 2;
    public static float LOCK_DOWN_TIME = 0.5f;
    public static float MAX_LOCK_DOWN_MOVES = 15;
    public static float SOFT_DROP_RATIO = 20;
    public static float MAX_CAMERA_HEIGHT = 20f;

    public static string ConvertIntToTime(int time)
    {
        string minutes = (time / 6000).ToString("00");
        string seconds = ((time / 100) % 60).ToString("00");
        string centiseconds = (time % 100).ToString();
        return $"{minutes}:{seconds}:{centiseconds}";
    }

    public static Color GetPlayerColor(int playerNum)
    {
        switch (playerNum)
        {
            case 0:
                return Color.cyan;
            case 1:
                return Color.red;
            case 2:
                return Color.green;
            case 3:
                return Color.yellow;
        }
        return Color.white;
    }

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




    public static int GetScore(int lines, SRS.T_Spin tSpin, bool backToBack, int level)
    {
        int result = 0;
        // ライン削除のスコア
        if (tSpin == SRS.T_Spin.None)
        {
            switch (lines)
            {
                case 0:
                    {
                        result = 0;
                        break;
                    }
                case 1:
                    {
                        result = 100 * level;
                        break;
                    }
                case 2:
                    {
                        result = 300 * level;
                        break;
                    }
                case 3:
                    {
                        result = 500 * level;
                        break;
                    }
                default:
                    {
                        result = 800 * level;
                        break;
                    }
            }
        }
        // T-Spin Miniのスコア
        else if (tSpin == SRS.T_Spin.T_Spin_Mini)
        {
            if (lines == 0) result = 100 * level;
            else result = 200 * level;
        }
        // T-Spinのスコア
        else
        {
            result = (lines + 1) * 400;
        }

        if (backToBack)
        {
            result += result / 2;
        }

        return result;
    }

}