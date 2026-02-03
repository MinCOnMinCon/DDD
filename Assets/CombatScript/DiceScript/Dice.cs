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

    private BoxCollider2D diceCollider;
    private Rigidbody2D diceRigidbody;

    [SerializeField] private Transform eyesRoot;        // pipsRoot → eyesRoot
    [SerializeField] private GameObject eyePrefab;      // pipPrefab → eyePrefab
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [SerializeField] private float eyeSizeNormal = 0.18f;   // 2~6 공통 크기
    [SerializeField] private float eyeSizeSingle = 0.28f;   // 1 전용 (좀 큼)
    [SerializeField] private float eyeDistant = 0.4f;
    private Color eyeColor;
    private Vector3[][] eyePositions;

    private bool isCursorIn;
    private Vector3 cursorPos;

    private Vector3 prevFramePos; //  드래그했을 때 이전 프레임에서 주사위 위치
    private Vector3 curFramePos; // 드래그가 끝났을 때의 주사위 위치
    [SerializeField]
    private float throwingSpeed; // 주사위 드래그 끝나고 날라가는 속도를 느리게 하기 위한 변수 
   

    public void DiceInit(int eye, DiceType type)
    {
        diceRigidbody.angularVelocity = 0;
        diceRigidbody.linearVelocity = Vector2.zero;
        ClearEyes();
        if(type == DiceType.loan)
        {
            diceData = new DiceData(eye, type, CombatManager.inst.combatContext.diceState.loanDiceSpan);
        }
        else
        {
            diceData = new DiceData(eye, type, int.MaxValue);
        }
        SetType(type);
        SpawnEyes(eye);
       
    }
    private void Awake()
    {
        isCursorIn = false;
        BuildEyePositions();
        diceCollider = GetComponent<BoxCollider2D>();
        diceRigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        DiceDrag();
    }
    
   

    private void DiceDrag()
    {
        
        if (Input.GetMouseButtonDown(0) && diceCollider.OverlapPoint(cursorPos))
        {
            diceRigidbody.gravityScale = 0f;
            diceRigidbody.angularVelocity = 0f;
            diceCollider.isTrigger = true;
            isCursorIn = true;
        }
     
        if (Input.GetMouseButtonUp(0) && isCursorIn)
        {
            diceRigidbody.gravityScale = 1f;
            diceCollider.isTrigger = false;
            isCursorIn = false;
            curFramePos = cursorPos;
            //Debug.Log(curFramePos +" "+ prevFramePos  +" "+Time.deltaTime);
            diceRigidbody.linearVelocityX = (curFramePos.x - prevFramePos.x) / (Time.deltaTime * throwingSpeed);
            diceRigidbody.linearVelocityY = (curFramePos.y - prevFramePos.y) / (Time.deltaTime * throwingSpeed);
            //Debug.Log(diceRigidbody.linearVelocity + "rear");
        }

        if (isCursorIn)
        {

            cursorPos.z = 0;
            gameObject.transform.position = cursorPos;
            prevFramePos = cursorPos;

        }

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
    public DiceData(int eye, DiceType type, int span)
    {
        diceEye = eye;
        diceValue = eye;
        diceType = type;
        diceSpan = span;
    }

    public int diceEye { get; private set; }
    public int diceValue { get; set; }
    public DiceType diceType { get; private set; }

    public int diceSpan;
}
