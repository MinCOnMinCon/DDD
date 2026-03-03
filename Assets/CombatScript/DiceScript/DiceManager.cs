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
    private DiceInitValue div; // ???????? ???? diceManager?? ??????. ?????? ?????? div?? diceState?? ?????? ???????? ????? ???????? ?????.

    private DiceState diceState;

    [SerializeField]
    private GameObject dicePrefab;
    
    private Vector3 diceSpawnPos;
    private void Awake()
    {
        diceState = new DiceState(div); 
        orders = new Dictionary<CombatPhase, int>();
        orders.Add(CombatPhase.turnStart, 0);
        orders.Add(CombatPhase.turnEnd, 2);
        diceSpawnPos = transform.position;
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
    /// ???? ????? ?????? ??, ?????? ?? ????? ??? ????????? ?г???????, ???? ??????? ??????? 
    /// ??????? ??? ??????? ????
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
    /// ????? ???????. ????? ??????? ??????? ?????. ????? ??????? ??????.
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
    /// ???? ????? ???? ????? ???? ?? ????? ???
    /// diceState?? ???? ???? ?? ??? ????? ????? ?????? loanDiceSpan?? ??????? ?????.
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
    /// type?? ??????? diceNum??? ?????? (???????? ?????) ????? ???? ?????? ???
    /// ?????? ???? diceNum?? basicDiceNum, penaltyDiceNUm ?????? ????? ??.
    /// </summary>
    /// <param name="diceNum"></param>
    /// <param name="type"></param>
    private void ConfirmDice(List<GameObject> diceList, int[] weightList)
    {         
        
        for(int idx = 0; idx < diceList.Count; idx++) // ?????? ?????? ??????? ??? ????
        {
            if (diceList[idx].GetComponent<Dice>().diceData.curSlotRole == DiceSlotRole.Saving) continue;
            //?? ???? ??????? ???? savingSlot?? ???? reroll ???? ??? ????.
            diceList[idx].transform.localPosition = diceSpawnPos;
            diceList[idx].GetComponent<Dice>().DiceReset(RollDice(weightList));
        }

    }
    

    private int RollDice(int[] weightList) // ????? ????? ??????? ???????? ????? ?? ????? ??? int?? ????
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
        bool canExecute = orders.ContainsKey(phase);
        return canExecute;
    }
    public void ApplyTo(CombatContext ctx)
    {
        ctx.diceState = new DiceState(diceState);
        ctx.diceFactory = this; // ??? ???????? ??????? ??????? ??????? ???? ???? 
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

    public int basePenaltyDicePerLoan { get; private set; } // ?? ???? ?? ?г?? ????? ??? ????
    public int curPenaltyDicePerLoan { get; set; } // ???? ???? ?? ?г?? ????? ??? ????
    public int loanDicePerLoan { get; set; } // ???? ?? ??? ?? ????? ??
    public int curLoanNum { get; set; } // ??? ?? ???? ???
    public int loanDiceSpan { get; set; } // ???? ????? ????

    public int[] basicDiceWeight { get; set; }
    public int[] penaltyDiceWeight { get; set; }
    public int[] loanDiceWeight { get; set; }
    public List<GameObject> basicDiceList { get; private set; }
    public List<GameObject> penaltyDiceList { get; private set; }

    public List<GameObject> loanDiceList { get; private set; }

    public DiceState(DiceInitValue div) //다이스 매니저 최초 생성자
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

    public DiceState(DiceState other) // 컴뱃 컨텍스트 전달용 복사 생성자
    {
        _basicDiceNum = other._basicDiceNum;
        _penaltyDiceNum = other._penaltyDiceNum;
        _loanDiceNum = other._loanDiceNum;

        basePenaltyDicePerLoan = other.basePenaltyDicePerLoan;
        curPenaltyDicePerLoan = other.curPenaltyDicePerLoan;
        loanDicePerLoan = other.loanDicePerLoan;
        curLoanNum = other.curLoanNum;
        loanDiceSpan = other.loanDiceSpan;

        basicDiceWeight = (int[])other.basicDiceWeight.Clone();
        penaltyDiceWeight = (int[])other.penaltyDiceWeight.Clone();
        loanDiceWeight = (int[])other.loanDiceWeight.Clone();

        basicDiceList = new List<GameObject>(other.basicDiceList);
        penaltyDiceList = new List<GameObject>(other.penaltyDiceList);
        loanDiceList = new List<GameObject>(other.loanDiceList);
    }
}

public interface IDiceService 
{
    void CreateDice(int neededDice, DiceType diceType, int span);
    void DestroyDice(int destroyedDice, DiceType diceType);
}
