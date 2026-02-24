using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;
using System.Linq;

public class ComboManager : MonoBehaviour, ICombatHook
{
   
    private Dictionary<CombatPhase, int> orders;
    private List<IComboRelic> comboRelicList;
    private ComboContext attackCtx;
    private ComboContext defenseCtx;
    private void Awake()
    {
        comboRelicList = new List<IComboRelic>();
        orders = new Dictionary<CombatPhase, int>();

        orders[CombatPhase.valueChange] = 0;
        orders[CombatPhase.valueConfirm] = 2;
    }

    private void Start()
    {

        Initialize();


    }

    private void Initialize()
    {
        foreach (var effect in RelicManager.inst.GetRelicEffects<IComboRelic>())
        {
            comboRelicList.Add(effect);
        }
        
        CombatManager.inst.HookRegister(this);
    }
    public void BuildComboSnapshot(ComboContext ctx)
    {
        ctx.eyeCounts = new int[6];
        
        foreach (var dice in ctx.diceList)
        { 
            // 1. �⺻ �� �ݿ�
            ctx.eyeCounts[dice.diceEye - 1]++;
           

        }
        // 2. eyeCounts ���ۿ� �����ϴ� ���� ó��
        // �⺻ �� �ݿ��� �� eyeCounts�� ���� �װ� �����Ѵ�.
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.CreateSnapshot);
        foreach (var relic in stageRelics)
        {
           

            relic.Activate(ctx);

        }

    }

    
    public void BuildComboCandidate(ComboContext ctx)
    {
        // 1. �⺻ �޺� �ĺ� ����
        ctx.candidate = GetBaseCandidate(ctx.eyeCounts);
     
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.BuildCandidate);
        // 2. �޺� ���� ���� ����
        foreach (var relic in stageRelics)
        {
           

            ctx.tempCandidate = ctx.candidate;
            relic.Activate(ctx);

            // 3. �ĺ� �� ���� ������ �� ���� �ĺ��� ������. ������ ���� �� ���� �ĺ��� ������.
            if (ctx.tempCandidate.Count > ctx.candidate.Count)
            {
                ctx.candidate = ctx.tempCandidate;
            }

            if (ctx.tempCandidate.Count == ctx.candidate.Count)
            {
                if(ctx.tempCandidate.Eye > ctx.candidate.Eye)
                {
                    ctx.candidate = ctx.tempCandidate;
                }
            }
        }

    }
    private ComboCandidate GetBaseCandidate(int[] eyeCounts)
    {
        int bestEye = 6;
        int bestCount = eyeCounts[5];

        for (int i = 4; i >= 0; i--)
        {
            if (eyeCounts[i] > bestCount)
            {
                bestCount = eyeCounts[i];
                bestEye = i + 1;
            }
            
        }

        return new ComboCandidate(bestEye, bestCount);
    }
    public void ModifyCandidateAfterBuild(ComboContext ctx)
    {
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.CandidateModify);
        foreach (var relic in stageRelics)
        {
            

            relic.Activate(ctx);

        }
    }
    private void ApplyComboEffect(ComboContext ctx)
    {
        ctx.isBaseComboReplaced = false;
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.EffectApply);
        foreach (var relic in stageRelics)
        {
            

            if (!ctx.isBaseComboReplaced && relic is IComboEffectReplace)
            {
                relic.Activate(ctx);
                Debug.Log(ctx.isBaseComboReplaced);
            }
            else if(relic is not IComboEffectReplace)
            {
                
                relic.Activate(ctx);
                
            }
        }

        if (!ctx.isBaseComboReplaced)
        {
            ApplyBaseCombo(ctx);
        }
    }
    private void ApplyBaseCombo(ComboContext ctx)
    {
        int count = ctx.candidate.Count - 1 < 0 ? 0 : ctx.candidate.Count - 1;
        int value = ctx.candidate.Eye * count;
        if(ctx.slotRole == DiceSlotRole.Attack)
        {
            ctx.combatCtx.snapshot.calcAttackValue += value;
        }
        if(ctx.slotRole == DiceSlotRole.Defense)
        {
            ctx.combatCtx.snapshot.calcDefenseValue += value;
        }
    }
    private void ApplyEffectAfterConfirm(ComboContext ctx)
    {
        var stageRelics = comboRelicList.Where(r => r.Stage == ComboStage.AfterConfirm);
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
                attackCtx = new ComboContext(ctx, DiceSlotRole.Attack, ctx.snapshot.attackDice);
                defenseCtx = new ComboContext(ctx, DiceSlotRole.Defense, ctx.snapshot.defenseDice);

                BuildComboSnapshot(attackCtx);
                BuildComboSnapshot(defenseCtx);

                BuildComboCandidate(attackCtx);
                BuildComboCandidate(defenseCtx);

                ModifyCandidateAfterBuild(attackCtx);
                ModifyCandidateAfterBuild(defenseCtx);

          
                ApplyComboEffect(attackCtx);
                //ApplyComboEffect(defenseCtx);
                break;
            case CombatPhase.valueConfirm:
                if (attackCtx == null && defenseCtx == null) break; // � ���Կ��� �ֻ����� ���� ��� => �׳� �ѱ�
                ApplyEffectAfterConfirm(attackCtx);
                ApplyEffectAfterConfirm(defenseCtx); 
                
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

/// <summary>
/// �⺻ �޺� ȿ���� ��ü�ϴ� ������ ����ϴ� �������̽�
/// </summary>
public interface IComboEffectReplace { } 
public interface IComboRelic
{
    ComboStage Stage { get; }
    
    void Activate(ComboContext ctx);
    
}
public class ComboContext
{
    public DiceSlotRole slotRole;
    public CombatContext combatCtx;
    public IReadOnlyList<DiceData> diceList;
    public int[] eyeCounts;

    public bool isBaseComboReplaced; // �޺� ȿ�� ��ü ������ �ߵ��� �⺻ �޺� ȿ���� ��ü�Ǿ����� üũ�ϴ� ����
    public ComboCandidate candidate; // ���� �ְ��� candidate
    public ComboCandidate tempCandidate; // ���� ȿ���� �߻��� �ӽ� candidate
    public ComboContext(CombatContext combatContext,
        DiceSlotRole slotRole,
        IReadOnlyList<DiceData> diceList)
    {
        this.slotRole = slotRole;
        this.combatCtx = combatContext;
        this.diceList = diceList;
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
public enum ComboStage
{
    CreateSnapshot,
    BuildCandidate, 
    CandidateModify,
    EffectApply,
    AfterConfirm
}


