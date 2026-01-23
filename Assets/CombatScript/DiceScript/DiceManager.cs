using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ue = UnityEngine;

public class DiceManager : MonoBehaviour
{
    

    
    [SerializeField]
    private DiceInitValue div;

    private DiceState diceState;

    [SerializeField]
    private GameObject dicePrefab;
    [SerializeField]
    private Vector3 diceSpawnPos;
    private void Awake()
    {
        diceState = new DiceState(div); 
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            LoanDice();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ConfirmDice(diceState.basicDiceNum, DiceType.basic);
            ConfirmDice(diceState.penaltyDiceNum, DiceType.penalty);
        }
    }
    public void LoanDice()
    {
        List <(GameObject, int)> diceList = diceState.loanDiceList;
        int[] weightList = diceState.loanDiceWeight;
        for(int i =  diceState.basicDicePerLoan; i > 0; i--)
        {
            diceList.Add((Instantiate(dicePrefab, diceSpawnPos, Quaternion.identity), 1));
            diceList[diceList.Count - 1].Item1.GetComponent<Dice>().DiceInit(RollDice(weightList), DiceType.loan);
        }
        diceState.penaltyDiceNum += diceState.curPenaltyDicePerLoan;
        diceState.loanNum++;
    }
    public void ConfirmDice(int diceNum, DiceType type)
    {
        
        int[] weightList;
        List<GameObject> diceList;
        switch (type) 
        {
            case DiceType.basic:
               
                weightList = diceState.basicDiceWeight;
                diceList = diceState.basicDiceList;
                break;
            case DiceType.penalty:
                weightList = diceState.penaltyDiceWeight;
                diceList = diceState.penaltyDiceList;

                break;
            default:
                weightList = null;
                diceList = null;
                break;
        }
        
        int idx;
        for(idx = 0; idx < diceList.Count; idx++)
        {
            diceList[idx].transform.localPosition = diceSpawnPos;
            diceList[idx].GetComponent<Dice>().DiceInit(RollDice(weightList), type);
        }
        for (; idx < diceNum; idx++)
        {
            diceList.Add(Instantiate(dicePrefab, diceSpawnPos, Quaternion.identity));
            diceList[diceList.Count - 1].GetComponent<Dice>().DiceInit(RollDice(weightList), type);
        }

    }

    private int RollDice(int[] weightList) // 주어진 가중치 리스트로 무작위로 주사위 눈 하나를 뽑아 int로 리턴
    {
        int result = ue.Random.Range(0, weightList[0]);
        int num = 0;
        for(int i =  1; i < div.diceEyeNum+1; i++)
        {
            num += weightList[i];
            if(result < num)
            {
                return i;
            }
            
        }
        return 0;
    }
}

public class DiceState
{
    private int _basicDiceNum;
    private int _penaltyDiceNum;
    private int _loanDiceNum;

    public int basicDiceNum
    {
        get => _basicDiceNum;
        set => _basicDiceNum = value;
    }
    public int penaltyDiceNum
    {
        get => _penaltyDiceNum;
        set => _penaltyDiceNum = value;
    }
    public int loanDiceNum
    {
        get => _loanDiceNum;
        set => _loanDiceNum = value;
    }

    public int basicPenaltyDicePerLoan { get; private set; } // 기본 대출 당 패널티 주사위 획득 개수
    public int curPenaltyDicePerLoan { get; set; } // 현재 대출 당 패널티 주사위 획득 개수
    public int basicDicePerLoan { get; set; } // 대출 당 주는 기본 주사위 수
    public int loanNum { get; set; } // 이번 턴 대출 횟수

    public int[] basicDiceWeight { get; set; }
    public int[] penaltyDiceWeight { get; set; }
    public int[] loanDiceWeight { get; set; }
    public List<GameObject> basicDiceList { get; private set; }
    public List<GameObject> penaltyDiceList { get; private set; }

    public List<(GameObject dice, int duration)> loanDiceList { get; private set; }

    public DiceState(DiceInitValue div)
    {
        basicDiceNum = div.basicDiceNum;
        penaltyDiceNum = div.penaltyDiceNum;
        loanDiceNum = div.loanDiceNum;
        basicPenaltyDicePerLoan = div.basicPenaltyDicePerLoan;
        curPenaltyDicePerLoan = div.curPenaltyDicePerLoan;
        basicDicePerLoan = div.basicDicePerLoan;
        loanNum = div.loanNum;
        basicDiceWeight = new int[div.diceEyeNum + 1];
        penaltyDiceWeight = new int[div.diceEyeNum + 1];
        loanDiceWeight = new int[div.diceEyeNum + 1];
        basicDiceList = new List<GameObject>();
        penaltyDiceList = new List<GameObject>();
        loanDiceList = new List<(GameObject, int)>();

        for (int i = div.diceEyeNum; i > 0; i--)
        {
            basicDiceWeight[i] = 1;
            penaltyDiceWeight[i] = 1;
            loanDiceWeight[i] = 1;
        }
        basicDiceWeight[0] = div.diceEyeNum;
        penaltyDiceWeight[0] = div.diceEyeNum;
        loanDiceWeight[0] = div.diceEyeNum;
    }
}