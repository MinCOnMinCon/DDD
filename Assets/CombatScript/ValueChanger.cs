using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class ValueChanger : MonoBehaviour, ICombatHook
{
    private Dictionary<CombatPhase, int> orders;
    private List<IValueRelic> valueRelicList;

    private void Awake()
    {
        valueRelicList = new List<IValueRelic>();  
        orders = new Dictionary<CombatPhase, int>();

        orders[CombatPhase.valueChange] = 2;
    }
    private void Start()
    {
        Initialize();
    } 
    private void Initialize()
    {
        foreach (var effect in RelicManager.inst.GetRelicEffects<IValueRelic>())
        {
            valueRelicList.Add(effect);
        }
        CombatManager.inst.HookRegister(this);

    }
    private void ApplyDiceValueToContext(ValueContext ctx)
    {
        
        int sum = 0;

        foreach (var dice in ctx.diceList)
        {
            int sign;
            var stageRelics = valueRelicList.Where(r => r.Stage == ValueStage.ValueApply);
            foreach (var relic in stageRelics)
            {
                if (!relic.CanAffect(ctx.slotRole))
                    continue;
                ctx.dice = dice;
                relic.Activate(ctx);

            }
            sign = dice.diceType == DiceType.penalty ? -1 : 1;
            
            sum += dice.diceValue * sign;
            
        }

            switch (ctx.slotRole)
            {
            case DiceSlotRole.Attack:
                ctx.combatContext.snapshot.calcAttackValue += sum;
                break;

            case DiceSlotRole.Defense:
                ctx.combatContext.snapshot.calcDefenseValue += sum;
                break;

            }
    }

    private void CreateSlotSnapshot(ValueContext ctx)
    {
        var snapshot = new SlotSnapshot();
        
        foreach (var dice in ctx.diceList)
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

        ctx.slotSnapshot = snapshot;
    }
    private void ApplyConditionalRelics(ValueContext ctx)
    {

        var stageRelics = valueRelicList.Where(r => r.Stage == ValueStage.ValueConditionCheck);
        foreach (var relic in stageRelics)
        {
            if (!relic.CanAffect(ctx.slotRole))
                continue;

            relic.Activate(ctx);
        }


    }
    void ApplyFinalValueRelics(ValueContext ctx)
    {
        var stageRelics = valueRelicList.Where(r => r.Stage == ValueStage.FinalValueEffect);
        foreach (var relic in stageRelics)
        {
            relic.Activate(ctx);
        }
    }

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.valueChange:
                ValueContext attackValueContext = new ValueContext(ctx, DiceSlotRole.Attack, ctx.snapshot.attackDice);
                ValueContext defenseValueContext = new ValueContext(ctx, DiceSlotRole.Defense, ctx.snapshot.defenseDice);

                ApplyDiceValueToContext(attackValueContext);
                ApplyDiceValueToContext(defenseValueContext);

                CreateSlotSnapshot(attackValueContext);
                CreateSlotSnapshot(defenseValueContext);

                ApplyConditionalRelics(attackValueContext);
                ApplyConditionalRelics(defenseValueContext);

                ApplyFinalValueRelics(attackValueContext); 
                // 이 함수는 컴뱃 컨텍스트에서 총 공격과 방어 수치만 알면 되기에 굳이 attack, defense value context 두개를 매개로 부를 필요는 없다.
                // 어차피 valueContext에는 combat Context가 있기 때문.

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

public interface IValueRelic : IRelic
{
    ValueStage Stage { get; }
    public void Activate(ValueContext ctx);
}

public class ValueContext
{
    public CombatContext combatContext { get; }
    public DiceSlotRole slotRole { get; }

    public List<DiceData> diceList { get; }
    public DiceData dice; // ValueApply Stage에서 효과를 받을 주사위. diceList의 주사위가 dice에 한번씩 들어감

    public SlotSnapshot slotSnapshot { set;  get; }
    public ValueContext(
        CombatContext combatContext,
        DiceSlotRole slotRole,
        List<DiceData> diceList
        )
    {
        this.combatContext = combatContext;
        this.slotRole = slotRole;
        this.diceList = diceList; 
    }
}

public class SlotSnapshot
{
    
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

public enum ValueStage 
{ 
    ValueApply,
    ValueConditionCheck,
    FinalValueEffect
}

    