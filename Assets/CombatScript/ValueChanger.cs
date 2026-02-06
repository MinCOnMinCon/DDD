using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ValueChanger : MonoBehaviour, ICombatHook
{
    private Dictionary<CombatPhase, int> orders;
    private List<IDiceValueChangeRelic> valueChangeRelics;
    private List<ISnapshotConditionRelic> conditionRelics;
    private List<IFinalValueRelic> finalValueRelics;

    private void Awake()
    {
        valueChangeRelics = new List<IDiceValueChangeRelic>();
        conditionRelics = new List<ISnapshotConditionRelic>();
        finalValueRelics = new List<IFinalValueRelic>();    
        orders = new Dictionary<CombatPhase, int>();

        orders[CombatPhase.valueChange] = 1;
    }
    private void Start()
    {
        Initialize();
    } 
    private void Initialize()
    {
        foreach (var effect in RelicManager.inst.GetRelicEffects<IDiceValueChangeRelic>())
        {
            valueChangeRelics.Add(effect);
        }
        foreach (var effect in RelicManager.inst.GetRelicEffects<ISnapshotConditionRelic>())
        {
            conditionRelics.Add(effect);
        }
        foreach (var effect in RelicManager.inst.GetRelicEffects<IFinalValueRelic>())
        {
            finalValueRelics.Add(effect);
        }
        CombatManager.inst.HookRegister(this);

    }
    private void ApplyDiceValueToContext(List<DiceData> diceList, CombatContext ctx, DiceSlotRole slotRole)
    {
        
        int sum = 0;

        foreach (var dice in diceList)
        {
            int sign;
            foreach (var relic in valueChangeRelics)
            {
                if (!relic.CanAffect(slotRole))
                    continue;

                relic.Activate(dice);

            }
            sign = dice.diceType == DiceType.penalty ? -1 : 1;
            
            sum += dice.diceValue * sign;
            
        }

            switch (slotRole)
            {
            case DiceSlotRole.Attack:
                ctx.calcAttackValue += sum;
                break;

            case DiceSlotRole.Defense:
                ctx.calcDefenseValue += sum;
                break;

            }
    }

    private SlotSnapshot CreateSlotSnapshot(List<DiceData> diceList, DiceSlotRole slotRole)
    {
        var snapshot = new SlotSnapshot();
        snapshot.slotRole = slotRole;
        foreach (var dice in diceList)
        {
            snapshot.totalDiceCount++;
            
            // 타입별 카운트
            switch (dice.diceType)
            {
                case DiceType.basic:
                    snapshot.basicDiceCount++;
                    break;
                case DiceType.penalty:
                    snapshot.penaltyDiceCount++;
                    break;
                case DiceType.loan:
                    snapshot.loanDiceCount++;
                    break;
            }

            // 타입별 리스트 초기화
            if (!snapshot.diceByType.ContainsKey(dice.diceType))
                snapshot.diceByType[dice.diceType] = new List<DiceInfo>();

            // DiceInfo 추가
            snapshot.diceByType[dice.diceType].Add(new DiceInfo
            {
                eye = dice.diceEye,
                value = dice.diceValue,
                type = dice.diceType
            });

            // EyeSummary 초기화
            if (!snapshot.eyeMap.TryGetValue(dice.diceEye, out var eyeSummary))
            {
                eyeSummary = new EyeSummary
                {
                    diceEye = dice.diceEye
                };
                snapshot.eyeMap[dice.diceEye] = eyeSummary;
            }

            // EyeSummary 집계
            eyeSummary.totalCount++;
            eyeSummary.totalValueSum += dice.diceValue;

            switch (dice.diceType)
            {
                case DiceType.basic:
                    eyeSummary.basicCount++;
                    break;
                case DiceType.penalty:
                    eyeSummary.penaltyCount++;
                    break;
                case DiceType.loan:
                    eyeSummary.loanCount++;
                    break;
            }
        }

        return snapshot;
    }
    private void ApplyConditionalRelics( SlotSnapshot snapshot, CombatContext ctx)
    {
       

        foreach (var relic in conditionRelics)
        {
            if (!relic.CanAffect(snapshot.slotRole))
                continue;

            relic.Activate(snapshot, ctx);
        }


    }
    void ApplyFinalValueRelics(CombatContext ctx)
    {
        foreach (var relic in finalValueRelics)
        {
            relic.Activate(ctx);
        }
    }

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.valueChange:
                var attackDiceList = DiceDataBuilder.BuildDiceDataList(ctx.attackSlotDiceList);
                var defenseDiceList = DiceDataBuilder.BuildDiceDataList(ctx.defenseSlotDiceList);
                ApplyDiceValueToContext(attackDiceList, ctx, DiceSlotRole.Attack);
                ApplyDiceValueToContext(defenseDiceList, ctx, DiceSlotRole.Defense);

                ApplyConditionalRelics(CreateSlotSnapshot(attackDiceList, DiceSlotRole.Attack), ctx);
                ApplyConditionalRelics(CreateSlotSnapshot(defenseDiceList, DiceSlotRole.Defense), ctx);

                ApplyFinalValueRelics(ctx);

                break;
        }

    }

    public int GetOrder(CombatPhase phase)
    {
        return orders.TryGetValue(phase, out int order) ? order : int.MaxValue;
    }
    public bool CanExecute(CombatPhase phase)
    {
        bool canExecute = phase == CombatPhase.valueChange;
        return canExecute;
    }






}

public interface IDiceValueChangeRelic : IRelicEffect
{
    void Activate(DiceData dice);
}
interface ISnapshotConditionRelic : IRelicEffect
{
    int Activate(SlotSnapshot snapshot, CombatContext ctx);
}
interface IFinalValueRelic : IRelicEffect
{
    void Activate(CombatContext ctx);
}
public class SlotSnapshot
{
    public DiceSlotRole slotRole;
    public int totalDiceCount;
    public int basicDiceCount;
    public int penaltyDiceCount;
    public int loanDiceCount;

    public Dictionary<int, EyeSummary> eyeMap = new();
    public Dictionary<DiceType, List<DiceInfo>> diceByType = new();
}

public class EyeSummary
{
    public int diceEye;
    public int totalCount;
    public int basicCount;
    public int penaltyCount;
    public int loanCount;

    public int totalValueSum;
}

public class DiceInfo
{
    public int eye;
    public int value;
    public DiceType type;
}
