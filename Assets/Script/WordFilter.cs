using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class WordFilter
{
    public static string Filter(string input)
    {
        // 大文字の場所を記録
        List<int> upperList = new();
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input, i))
            {
                upperList.Add(i);
            }
        }

        // 小文字に変換
        string result = String.Copy(input).ToLower();

        // 単語リストを照合
        foreach (string swearWord in swearWords)
        {
            // 文字が含まれている限り続ける
            while (true)
            {
                int index = result.IndexOf(swearWord);
                if (index < 0) break;

                string asterisk = new String('*', swearWord.Length);
                result = result.Replace(swearWord, asterisk);
            }
        }

        // 大文字の場所を元に戻す
        char[] chars = result.ToCharArray();
        foreach (int upperIndex in upperList)
        {
            chars[upperIndex] = char.ToUpper(chars[upperIndex]);
        }

        return new string(chars);
    }

    private static string[] swearWords = new string[]{
        "anal",
        "anus",
        "arse",
        "ass",
        "ballsack",
        "balls",
        "bastard",
        "bitch",
        "biatch",
        "bloody",
        "blowjob",
        "blow job",
        "bollock",
        "bollok",
        "boner",
        "boob",
        "bugger",
        "bum",
        "butt",
        "buttplug",
        "clitoris",
        "cock",
        "coon",
        "crap",
        "cunt",
        "damn",
        "dick",
        "dildo",
        "dyke",
        "fag",
        "feck",
        "fellate",
        "fellatio",
        "felching",
        "fuck",
        "f u c k",
        "fudgepacker",
        "fudge packer",
        "flange",
        "Goddamn",
        "God damn",
        "hell",
        "homo",
        "jerk",
        "jizz",
        "knobend",
        "knob end",
        "labia",
        "lmao",
        "lmfao",
        "muff",
        "nigger",
        "nigga",
        "omg",
        "penis",
        "piss",
        "poop",
        "prick",
        "pube",
        "pussy",
        "queer",
        "scrotum",
        "sex",
        "shit",
        "s hit",
        "sh1t",
        "slut",
        "smegma",
        "spunk",
        "tit",
        "tosser",
        "turd",
        "twat",
        "vagina",
        "wank",
        "whore",
        "wtf",
    };
}
