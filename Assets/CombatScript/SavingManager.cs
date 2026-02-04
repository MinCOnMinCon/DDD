

using System.Collections.Generic;
using UnityEngine;

public class SavingManager : MonoBehaviour, ICombatHook
{
    private Dictionary<CombatPhase, int> orders;
    private List<ISingleDiceRelic> singleRelics;
    private List<IMultiDiceRelic> multiRelics;
    [SerializeField]
    private int maxSavingValue = 20;
    private void Awake()
    {
        orders = new Dictionary<CombatPhase, int>();
        singleRelics = new List<ISingleDiceRelic>();
        multiRelics = new List<IMultiDiceRelic>();
        orders[CombatPhase.turnEnd] = 0;
    }
    private void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        foreach (var effect in RelicManager.inst.GetRelicEffects<ISingleDiceRelic>())
        {
            singleRelics.Add(effect);
        }
        foreach (var effect in RelicManager.inst.GetRelicEffects<IMultiDiceRelic>())
        {
            multiRelics.Add(effect);
        }
  
        CombatManager.inst.HookRegister(this);
    }


    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.turnEnd:
                List<DiceData> diceList = DiceDataBuilder.BuildDiceDataList(ctx.savingSlotDiceList);
                EvaluateSingleDice(diceList);
                SavingSnapshot snapshot = new SavingSnapshot(diceList);
                EvaluateMultiDice(snapshot);
                break;

        }
    }
    private void EvaluateSingleDice(List<DiceData> diceDataList)
    {


        foreach (var diceData in diceDataList)
        {
            int doubledValue = diceData.diceValue * 2;
            diceData.diceValue = Mathf.Min(diceData.diceValue * 2, maxSavingValue);
            ActivateSingleRelics(diceData);
        }
    }

    private void ActivateSingleRelics(DiceData diceData)
    {
        foreach(var relic in singleRelics)
        {
            relic.Activate(diceData); 
        }
    }
    private void EvaluateMultiDice(SavingSnapshot snapshot)
    {
        foreach(var relic in multiRelics)
        {
            relic.Activate(snapshot);
        }
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = phase == CombatPhase.turnEnd;
        return canExecute;
    }
    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    
}
public class SavingSnapshot
{
    public readonly List<DiceData> DiceList;
    public int DiceCount => DiceList.Count;

    public SavingSnapshot(List<DiceData> source)
    {
        DiceList = new List<DiceData>(source);
    }

    public bool IsFullyFilled(int requiredCount)
    {
        return DiceList.Count == requiredCount;
    }

    public bool IsAllSameEye()
    {
        if (DiceList.Count == 0) return false;

        int eye = DiceList[0].diceEye;
        foreach (var dice in DiceList)
        {
            if (dice.diceEye != eye)
                return false;
        }
        return true;
    }
    public (bool isSame, DiceType type) IsAllSameType()
    {
        if (DiceList.Count == 0) return (false, DiceType.basic);

        DiceType type = DiceList[0].diceType;
        foreach (var dice in DiceList)
        {
            if (dice.diceType != type)
                return (false, DiceType.basic);
        }
        return (true, type);
    }
}
public interface ISingleDiceRelic : IRelicEffect //한개의 주사위를 조건으로 하는 유물
{
    void Activate(DiceData dice);
}
public interface IMultiDiceRelic : IRelicEffect//여러 개의 주사위를 조건으로 하는 유물
{
    void Activate(SavingSnapshot snapshot);
}

