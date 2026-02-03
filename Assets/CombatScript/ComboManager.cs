using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class ComboManager : MonoBehaviour, ICombatHook
{
    [SerializeField]
    private Dictionary<CombatPhase, int> orders;
    private List<ISnapshotRelic> snapshotRelics;
    private List<IComboConditionRelic> conditionRelics;
    private List<IComboEffectRelic> effectRelics;
    private void Awake()
    {
        snapshotRelics = new List<ISnapshotRelic>();
        conditionRelics = new List<IComboConditionRelic>();
        effectRelics = new List<IComboEffectRelic>();
        orders = new Dictionary<CombatPhase, int>();

        orders[CombatPhase.valueChange] = 0;
    }

    private void Start()
    {
        Debug.Log("dafdas1");

        CombatManager.inst.HookRegister(this);
    }
    public int[] BuildComboSnapshot(List<DiceData> diceObjects, DiceSlotRole slotRole)
    {
        int[] eyeCounts = new int[6];
        foreach (var dice in diceObjects)
        { 
            // 1. 기본 눈 반영
            eyeCounts[dice.diceEye - 1]++;

            // 2. 복사본 제작에 관여하는 유물 처리
            foreach (var relic in snapshotRelics)
            {
                if (!relic.CanAffect(slotRole))
                    continue;

                relic.Activate(dice, eyeCounts);
                    
            }
        }

        return eyeCounts;
    }

    
    public ComboCandidate BuildComboCandidate(int[] eyeCounts,DiceSlotRole slotRole)
    {
        // 1. 기본 콤보 후보 선정
        ComboCandidate bestCandidate = GetBaseCandidate(eyeCounts);

        // 2. 콤보 관여 유물 적용
        foreach (var relic in conditionRelics)
        {
            if (!relic.CanAffect(slotRole))
                continue;

            ComboCandidate relicCandidate =
                relic.Activate(eyeCounts);

            // 3. 더 높은 개수만 채택
            if (relicCandidate.Count > bestCandidate.Count)
            {
                bestCandidate = relicCandidate;
            }
        }

        return bestCandidate;
    }
    private ComboCandidate GetBaseCandidate(int[] eyeCounts)
    {
        int bestEye = 1;
        int bestCount = eyeCounts[0];

        for (int i = 1; i < 6; i++)
        {
            if (eyeCounts[i] > bestCount)
            {
                bestCount = eyeCounts[i];
                bestEye = i + 1;
            }
        }

        return new ComboCandidate(bestEye, bestCount);
    }
    public void ApplyComboEffect(ComboCandidate candidate, DiceSlotRole slotRole,CombatContext ctx)
    {
        bool isReplaced = false;

        foreach (var relic in effectRelics)
        {
            if (!relic.CanAffect(slotRole))
                continue;

            if (relic.MatchCandidate(candidate))
            {
                relic.Activate(candidate, ctx, slotRole);
                isReplaced = true;
                break; // 하나만 적용
            }
        }

        if (!isReplaced)
        {
            ApplyBaseCombo(candidate, ctx, slotRole);
        }
    }
    private void ApplyBaseCombo(ComboCandidate candidate, CombatContext ctx, DiceSlotRole slotRole)
    {
        int count = candidate.Count - 1 < 0 ? 0 : candidate.Count - 1;
        int value = candidate.Eye * count;
        if(slotRole == DiceSlotRole.Attack)
        {
            ctx.attackValue += value;
        }
        if(slotRole == DiceSlotRole.Defense)
        {
            ctx.defenseValue += value;
        }
    }

    public void OnCombatPhase(CombatPhase phase, CombatContext ctx)
    {
        switch (phase)
        {
            case CombatPhase.valueChange:
                int[] attackEyeCounts = BuildComboSnapshot(DiceDataBuilder.BuildDiceDataList(ctx.attackSlotDiceList), DiceSlotRole.Attack);
                int[] defenseEyeCounts = BuildComboSnapshot(DiceDataBuilder.BuildDiceDataList(ctx.defenseSlotDiceList), DiceSlotRole.Defense);

                ComboCandidate attackCandidate = BuildComboCandidate(attackEyeCounts, DiceSlotRole.Attack);
                ComboCandidate defenseCandidate =  BuildComboCandidate(defenseEyeCounts, DiceSlotRole.Defense);

                ApplyComboEffect(attackCandidate, DiceSlotRole.Attack, ctx);
                ApplyComboEffect(defenseCandidate, DiceSlotRole.Defense, ctx);
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

public struct ComboCandidate
{
    public int Eye;// 1~6
    public int Count;
   
    public ComboCandidate(int eye, int count)
    {
        Eye = eye;
        Count = count;
    }
}
public interface ISnapshotRelic 
{
    void Activate(DiceData dice, int[] eyeCoutns);
    bool CanAffect(DiceSlotRole slotRole) { return true; }
}
public interface IComboConditionRelic 
{
    ComboCandidate Activate(int[] eyeCounts);
    bool CanAffect(DiceSlotRole slotRole) { return true; }
}

public interface IComboEffectRelic 
{
    void Activate(ComboCandidate candidate, CombatContext ctx, DiceSlotRole slotRole);
    bool MatchCandidate(ComboCandidate candidate);
    bool CanAffect(DiceSlotRole slotRole) { return true; }
}
