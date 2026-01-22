using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum DiceType
{
    basic,
    penalty,
    loan
}
public class Dice : MonoBehaviour
{
    public DiceData diceData { get; private set; }
    [SerializeField] private Transform eyesRoot;        // pipsRoot → eyesRoot
    [SerializeField] private GameObject eyePrefab;      // pipPrefab → eyePrefab
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [SerializeField] private float eyeSizeNormal = 0.18f;   // 2~6 공통 크기
    [SerializeField] private float eyeSizeSingle = 0.28f;   // 1 전용 (좀 큼)
    [SerializeField] private float eyeDistant = 0.4f;
    private Color eyeColor;
    private Vector3[][] eyePositions; 

    public void DiceInit(int eye, DiceType type)
    {
        diceData = new DiceData(eye, type);
        SetType(type);
        SpawnEyes(eye);
       
    }
    private void Awake()
    {
        BuildEyePositions();
    }
    private void BuildEyePositions()
    {
        float d = eyeDistant;

        eyePositions = new Vector3[][]
        {
        // 1
        new[]
        {
            Vector3.zero
        },

        // 2
        new[]
        {
            new Vector3(-d,  d),
            new Vector3( d, -d)
        },

        // 3
        new[]
        {
            new Vector3(-d,  d),
            Vector3.zero,
            new Vector3( d, -d)
        },

        // 4
        new[]
        {
            new Vector3(-d,  d),
            new Vector3( d,  d),
            new Vector3(-d, -d),
            new Vector3( d, -d)
        },

        // 5
        new[]
        {
            new Vector3(-d,  d),
            new Vector3( d,  d),
            Vector3.zero,
            new Vector3(-d, -d),
            new Vector3( d, -d)
        },

        // 6
        new[]
        {
            new Vector3(-d,  d),
            new Vector3( d,  d),
            new Vector3(-d,  0),
            new Vector3( d,  0),
            new Vector3(-d, -d),
            new Vector3( d, -d)
        },
        };
    }

    private void SetType(DiceType type)
    {
        switch (type)
        {
            case DiceType.basic:
                backgroundRenderer.color = Color.white;
                eyeColor = Color.black;
                break;

            case DiceType.penalty:
                backgroundRenderer.color = Color.red;
                eyeColor = Color.orange;

                break;

            case DiceType.loan:
                backgroundRenderer.color = Color.yellow;
                eyeColor = Color.white;
                break;
        }
    }
    private void ClearEyes()
    {
        for (int i = eyesRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(eyesRoot.GetChild(i).gameObject);
        }
    }
    private void SpawnEyes(int value)
    {
        var positions = eyePositions[value - 1];

        float eyeSize = (value == 1) ? eyeSizeSingle : eyeSizeNormal;
        Vector3 eyeScale = Vector3.one * eyeSize;

        foreach (var pos in positions)
        {
            var eye = Instantiate(eyePrefab, eyesRoot);
            eye.transform.localPosition = pos;
            eye.transform.localScale = eyeScale;
            var eyeRenderer = eye.GetComponent<SpriteRenderer>();
            eyeRenderer.color = eyeColor;
        }
    }
}

public class DiceData
{
    public DiceData(int eye, DiceType type)
    {
        diceEye = eye;
        diceValue = eye;
        diceType = type;
        
    }

    public int diceEye { get; private set; }
    public int diceValue { get; set; }
    public DiceType diceType { get; private set; }
}
