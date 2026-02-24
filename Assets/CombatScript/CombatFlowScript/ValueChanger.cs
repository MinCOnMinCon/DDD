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
            
            // Ÿ�Ժ� ī��Ʈ
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

            // Ÿ�Ժ� ����Ʈ �ʱ�ȭ
            if (!snapshot.diceByType.ContainsKey(dice.diceType))
                snapshot.diceByType[dice.diceType] = new List<DiceInfo>();

            // DiceInfo �߰�
            snapshot.diceByType[dice.diceType].Add(new DiceInfo
            {
                eye = dice.diceEye,
                value = dice.diceValue,
                type = dice.diceType
            });

            // EyeSummary �ʱ�ȭ
            if (!snapshot.eyeMap.TryGetValue(dice.diceEye, out var eyeSummary))
            {
                eyeSummary = new EyeSummary
                {
                    diceEye = dice.diceEye
                };
                snapshot.eyeMap[dice.diceEye] = eyeSummary;
            }

            // EyeSummary ����
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
                // �� �Լ��� �Ĺ� ���ؽ�Ʈ���� �� ���ݰ� ��� ��ġ�� �˸� �Ǳ⿡ ���� attack, defense value context �ΰ��� �Ű��� �θ� �ʿ�� ����.
                // ������ valueContext���� combat Context�� �ֱ� ����.
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






}

public interface IValueRelic 
{
    ValueStage Stage { get; }
    public void Activate(ValueContext ctx);
}

public class ValueContext
{
    public CombatContext combatContext { get; }
    public DiceSlotRole slotRole { get; }

    public List<DiceData> diceList { get; }
    public DiceData dice; // ValueApply Stage���� ȿ���� ���� �ֻ���. diceList�� �ֻ����� dice�� �ѹ��� ��

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

    