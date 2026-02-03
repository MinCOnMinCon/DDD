using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ue = UnityEngine;
public class DiceManager : MonoBehaviour, ICombatHook, ICombatContextProvider
{
    [SerializeField]
    private Dictionary<CombatPhase, int> orders;    
    
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
        orders = new Dictionary<CombatPhase, int>();
        orders.Add(CombatPhase.turnStart, 0);
        orders.Add(CombatPhase.turnEnd, 0);
    }

    private void Start()
    {
        CombatManager.inst.HookRegister(this);
        ApplyTo(CombatManager.inst.combatContext);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ConfirmDice(diceState.basicDicePerLoan, DiceType.loan);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            CheckDiceSpan();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ConfirmDice(diceState.basicDiceNum, DiceType.basic);
            ConfirmDice(diceState.penaltyDiceNum, DiceType.penalty);
        }
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
            case DiceType.loan:
                weightList = diceState.loanDiceWeight;
                diceList = diceState.loanDiceList;
                diceState.penaltyDiceNum += diceState.curPenaltyDicePerLoan;
                diceState.curLoanNum++;
                break;
            default:
                weightList = null;
                diceList = null;
                break;
        }
        
        int idx;
        for(idx = 0; idx < diceList.Count; idx++) // 기존에 생성된 주사위를 다시 굴림
        {
            diceList[idx].transform.localPosition = diceSpawnPos;
            diceList[idx].GetComponent<Dice>().DiceInit(RollDice(weightList), type);
        }
        for (; idx < diceNum; idx++) // 기존에 주사위 수보다 늘어난 수만큼 주사위를 만들고 굴림
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

    private void CheckDiceSpan()
    {

        List<GameObject> list = diceState.loanDiceList;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null) continue;
            
            DiceData diceData = list[i].GetComponent<Dice>().diceData;  
            if(diceData.diceSpan > 0)
            {
                Debug.Log(diceData.diceSpan);
                diceData.diceSpan--;
                
            }
            else
            {
                GameObject dice = list[i];
                diceState.loanDiceList.Remove(list[i]);
                Destroy(dice);
            }
        }
    }

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.turnStart:
                ConfirmDice(diceState.basicDiceNum, DiceType.basic);
                ConfirmDice(diceState.penaltyDiceNum, DiceType.penalty);
                break;
            case CombatPhase.turnEnd:
                CheckDiceSpan();
                break;
        }
       
    }
    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = phase == CombatPhase.turnStart || phase == CombatPhase.turnEnd;
        return canExecute;
    }
    public void ApplyTo(CombatContext ctx)
    {
        ctx.diceState = diceState;
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
    public int curLoanNum { get; set; } // 이번 턴 대출 횟수
    public int loanDiceSpan { get; set; } // 대출 주사위 수명

    public int[] basicDiceWeight { get; set; }
    public int[] penaltyDiceWeight { get; set; }
    public int[] loanDiceWeight { get; set; }
    public List<GameObject> basicDiceList { get; private set; }
    public List<GameObject> penaltyDiceList { get; private set; }

    public List<GameObject> loanDiceList { get; private set; }

    public DiceState(DiceInitValue div)
    {
        basicDiceNum = div.basicDiceNum;
        penaltyDiceNum = div.penaltyDiceNum;
        loanDiceNum = div.loanDiceNum;
        basicPenaltyDicePerLoan = div.basicPenaltyDicePerLoan;
        curPenaltyDicePerLoan = div.curPenaltyDicePerLoan;
        basicDicePerLoan = div.basicDicePerLoan;
        curLoanNum = div.loanNum;
        loanDiceSpan = div.loanDiceSpan;
        basicDiceWeight = new int[div.diceEyeNum + 1];
        penaltyDiceWeight = new int[div.diceEyeNum + 1];
        loanDiceWeight = new int[div.diceEyeNum + 1];
        basicDiceList = new List<GameObject>();
        penaltyDiceList = new List<GameObject>();
        loanDiceList = new List<GameObject>();

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