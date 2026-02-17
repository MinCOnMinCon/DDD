using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using ue = UnityEngine;



public class DiceManager : MonoBehaviour, ICombatHook, ICombatContextProvider, IDiceService
{
    [SerializeField]
    private Dictionary<CombatPhase, int> orders;
    private static int nonLoanSpan = int.MaxValue;
    
    [SerializeField]
    private DiceInitValue div; // 비전투일 때의 diceManager의 데이터. 전투가 시작되면 div로 diceState를 생성해 전투에서 사용되는 스냅샷을 만든다.

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
        orders.Add(CombatPhase.turnEnd, 2);
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
            LoanDice();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            CheckDiceSpan();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            TurnStartDiceGenerate();
        }
    }

    /// <summary>
    /// 굴림 버튼을 눌렀을 때, 실제로 더 필요한 만큼 기본주사위와 패널티주사위, 대출 주사위를 생성하고 
    /// 리스트의 모든 주사위를 돌림
    /// </summary>
    public void TurnStartDiceGenerate()
    {
        int neededBasicDice = diceState.basicDiceNum - diceState.basicDiceList.Count;
        int neededPenaltyDice = diceState.penaltyDiceNum - diceState.penaltyDiceList.Count;
        int neededLoanDice = diceState.loanDiceNum - diceState.loanDiceList.Count;

        CreateDice(neededBasicDice, DiceType.basic, nonLoanSpan);
        CreateDice(neededPenaltyDice, DiceType.penalty, nonLoanSpan);
        CreateDice(neededLoanDice, DiceType.loan, diceState.loanDiceSpan);


        ConfirmDice(diceState.basicDiceList, diceState.basicDiceWeight);
        ConfirmDice(diceState.penaltyDiceList, diceState.penaltyDiceWeight);
        ConfirmDice(diceState.loanDiceList, diceState.loanDiceWeight);
       
    }
    /// <summary>
    /// 주사위 생성함수. 주어진 파라미터로 주사위를 만든다. 그리고 리스트에 추가한다.
    /// </summary>
    /// <param name="neededDice"></param>
    /// <param name="diceType"></param>
    /// <param name="span"></param>
    public void CreateDice(int neededDice, DiceType diceType, int span)
    {
        List<GameObject> diceList;
        switch (diceType) 
        {
            case DiceType.basic:
                diceList = diceState.basicDiceList;
                break;
            case DiceType.penalty:
                diceList = diceState.penaltyDiceList;
                break;
            case DiceType.loan:
                diceList = diceState.loanDiceList;
                break;
            default:
                diceList = null;
                break;

        }

        for (int i = 0; i < neededDice; i++)
        {
            diceList.Add(Instantiate(dicePrefab, diceSpawnPos, Quaternion.identity));
            GameObject dice = diceList[diceList.Count - 1];
            dice.GetComponent<Dice>().DiceInit(span, diceType);
        }
    }
    public void DestroyDice(int destroyedDice, DiceType diceType)
    {
        List<GameObject> diceList;
        switch (diceType)
        {
            case DiceType.basic:
                diceList = diceState.basicDiceList;
                break;
            case DiceType.penalty:
                diceList = diceState.penaltyDiceList;
                break;
            case DiceType.loan:
                diceList = diceState.loanDiceList;
                break;
            default:
                diceList = null;
                break;

        }
        for(int i = diceList.Count - 1; i>= destroyedDice; i--)
        {
            Destroy(diceList[i]);
        }
    }
    // <summary>
    /// 대출 버튼을 눌러 주사위 대출 시 호출되는 함수
    /// diceState에 적힌 대출 시 받는 주사위 수만큼 수명이 loanDiceSpan인 주사위를 만든다.
    /// </summary>
    /// <param name="diceNum"></param>
    /// <param name="isLoan"></param>
    /// <param name="span"></param>
    public void LoanDice()
    {

        List<GameObject> diceList = diceState.loanDiceList;
        
        CreateDice(diceState.loanDicePerLoan, DiceType.loan, diceState.loanDiceSpan);
        List<GameObject> rerollLoanDiceList = new List<GameObject>();

        for(int i = 0; i < diceState.loanDicePerLoan; i++)
        {
            rerollLoanDiceList.Add(diceList[diceList.Count - i - 1]);
        }

        ConfirmDice(rerollLoanDiceList, diceState.loanDiceWeight);

        diceState.loanDiceNum += diceState.loanDicePerLoan;
        diceState.penaltyDiceNum += diceState.curPenaltyDicePerLoan;
        diceState.curLoanNum++;
        
    }
    /// <summary>
    /// type인 주사위를 diceNum만큼 굴려서 (부족하다면 추가함) 주사위 눈을 확정하는 함수
    /// 유의할 것은 diceNum에 basicDiceNum, penaltyDiceNUm 변수만 들어가야 함.
    /// </summary>
    /// <param name="diceNum"></param>
    /// <param name="type"></param>
    private void ConfirmDice(List<GameObject> diceList, int[] weightList)
    {         
        
        for(int idx = 0; idx < diceList.Count; idx++) // 기존에 생성된 주사위를 다시 굴림
        {
            if (diceList[idx].GetComponent<Dice>().diceData.curSlotRole == DiceSlotRole.Saving) continue;
            //위 줄은 주사위가 만약 savingSlot에 있다면 reroll 하지 않게 만듦.
            diceList[idx].transform.localPosition = diceSpawnPos;
            diceList[idx].GetComponent<Dice>().DiceReset(RollDice(weightList));
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
                TurnStartDiceGenerate();
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
        bool canExecute = (phase == CombatPhase.turnStart || phase == CombatPhase.turnEnd);
        return canExecute;
    }
    public void ApplyTo(CombatContext ctx)
    {
        ctx.diceState = diceState;
        ctx.diceFactory = this; // 컴뱃 컨텍스트에 주사위를 생성하고 삭제하는 기능만 제공 
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

    public int basePenaltyDicePerLoan { get; private set; } // 기본 대출 당 패널티 주사위 획득 개수
    public int curPenaltyDicePerLoan { get; set; } // 현재 대출 당 패널티 주사위 획득 개수
    public int loanDicePerLoan { get; set; } // 대출 당 주는 기본 주사위 수
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
        basePenaltyDicePerLoan = div.basePenaltyDicePerLoan;
        curPenaltyDicePerLoan = div.curPenaltyDicePerLoan;
        loanDicePerLoan = div.loanDicePerLoan;
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

public interface IDiceService 
{
    void CreateDice(int neededDice, DiceType diceType, int span);
    void DestroyDice(int destroyedDice, DiceType diceType);
}
