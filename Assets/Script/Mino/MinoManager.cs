using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinoManager : MonoBehaviour
{
    public enum Mino
    {
        I = 0,
        O = 1,
        S = 2,
        Z = 3,
        J = 4,
        L = 5,
        T = 6,
    }

    [SerializeField] private GameObject[] minoObjects;
    public GameObject getMino(int id) => minoObjects[id];
    public int getMinoLength() => minoObjects.Length;

    [SerializeField] private GameObject[] minoUIs;
    public GameObject getMinoUI(int id) => minoUIs[id];

    [Header("Particles")]
    public GameObject deleteEffect;
    public GameObject dropParticle;

    public GameObject cubisObject;

    public static MinoManager instance;
    private void Awake()
    {
        instance = this;
    }

    public static Color GetMinoColor(int id)
    {
        if (id == (int)Mino.I) return Color.cyan;
        if (id == (int)Mino.O) return Color.yellow;
        if (id == (int)Mino.S) return Color.green;
        if (id == (int)Mino.Z) return Color.red;
        if (id == (int)Mino.J) return Color.blue;
        if (id == (int)Mino.L) return new Color(1f, 0.5f, 0f);
        if (id == (int)Mino.T) return Color.magenta;

        return Color.white;
    }
}

public static class SRS
{
    public enum Direction
    {
        Right, Left
    }

    private static Vector2[][] ALPHA_LEFT_MOVES = new Vector2[][] {
        new Vector2[] {new Vector2( 1, 0), new Vector2( 1,  1), new Vector2( 0, -2), new Vector2( 1,-2)},
        new Vector2[] {new Vector2(-1, 0), new Vector2(-1, -1), new Vector2( 0,  2), new Vector2(-1, 2)},
        new Vector2[] {new Vector2(-1, 0), new Vector2(-1,  1), new Vector2( 0, -2), new Vector2(-1,-2)},
        new Vector2[] {new Vector2( 1, 0), new Vector2( 1, -1), new Vector2( 0,  2), new Vector2( 1, 2)},
    };

    private static Vector2[][] ALPHA_RIGHT_MOVES = new Vector2[][] {
        new Vector2[] {new Vector2(-1, 0), new Vector2(-1,  1), new Vector2(0, -2), new Vector2(-1,-2)},
        new Vector2[] {new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(0,  2), new Vector2(-1, 2)},
        new Vector2[] {new Vector2( 1, 0), new Vector2( 1,  1), new Vector2(0, -2), new Vector2( 1,-2)},
        new Vector2[] {new Vector2( 1, 0), new Vector2( 1, -1), new Vector2(0,  2), new Vector2( 1, 2)},
    };

    private static Vector2[][] BETA_RIGHT_MOVES = new Vector2[][] {
        new Vector2[] {new Vector2(-2, 0), new Vector2( 1, 0), new Vector2(-2, -1), new Vector2( 1, 2)},
        new Vector2[] {new Vector2(-1, 0), new Vector2( 2, 0), new Vector2(-1,  2), new Vector2( 2,-1)},
        new Vector2[] {new Vector2( 2, 0), new Vector2(-1, 0), new Vector2( 2,  1), new Vector2(-1,-2)},
        new Vector2[] {new Vector2( 1, 0), new Vector2(-2, 0), new Vector2( 1, -2), new Vector2(-2, 1)},
    };

    private static Vector2[][] BETA_LEFT_MOVES = new Vector2[][] {
        new Vector2[] {new Vector2(-1, 0), new Vector2( 2, 0), new Vector2(-1,  2), new Vector2( 2,-1)},
        new Vector2[] {new Vector2( 2, 0), new Vector2(-1, 0), new Vector2( 2,  1), new Vector2(-1,-2)},
        new Vector2[] {new Vector2( 1, 0), new Vector2(-2, 0), new Vector2( 1, -2), new Vector2(-2, 1)},
        new Vector2[] {new Vector2(-2, 0), new Vector2( 1, 0), new Vector2(-2, -1), new Vector2( 1, 2)},
    };

    public static Vector2[] GetAlphaMoves(int rotation, Direction direction)
    {
        if (direction == Direction.Right) return ALPHA_RIGHT_MOVES[rotation];
        return ALPHA_LEFT_MOVES[rotation];
    }

    // public static Vector2[] GetHorizontalAlphaMoves()

    public static Vector2[] GetBetaMoves(int rotation, Direction direction)
    {
        if (direction == Direction.Right) return BETA_RIGHT_MOVES[rotation];
        return BETA_LEFT_MOVES[rotation];
    }
}
