using UnityEngine;

// 공통 베이스
public abstract class ValueRelicEffectBase : RelicEffect, IValueRelic
{
    public abstract ValueStage Stage { get; }

    protected ValueRelicEffectBase(int relicId) : base(relicId) { }

    public virtual bool CanAffect(DiceSlotRole slotRole) => true;

    public abstract void Activate(ValueContext ctx);
}

// ---------------- 수치계산 - 1 (ValueApply) ----------------

public class Relic_6 : ValueRelicEffectBase
{
    public Relic_6() : base(6) { }
    public override ValueStage Stage => ValueStage.ValueApply;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.dice.diceType == DiceType.penalty)
        {
            ctx.dice.diceValue = Mathf.Abs(ctx.dice.diceValue);
        }
    }
}

public class Relic_22 : ValueRelicEffectBase
{
    public Relic_22() : base(22) { }
    public override ValueStage Stage => ValueStage.ValueApply;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.dice.diceEye <= 3)
            ctx.dice.diceValue += 2;
    }
}

public class Relic_31 : ValueRelicEffectBase
{
    public Relic_31() : base(31) { }
    public override ValueStage Stage => ValueStage.ValueApply;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.dice.diceEye == 1)
            ctx.dice.diceValue += 10;
        else
            ctx.dice.diceValue -= 2;
    }
}

public class Relic_32 : ValueRelicEffectBase
{
    public Relic_32() : base(32) { }
    public override ValueStage Stage => ValueStage.ValueApply;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.dice.diceType == DiceType.loan)
            ctx.dice.diceValue += 2;
    }
}

// ---------------- 수치계산 - 2 (ValueConditionCheck) ----------------

public class Relic_4 : ValueRelicEffectBase
{
    public Relic_4() : base(4) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.combatContext.turnCount == 1 && ctx.slotRole == DiceSlotRole.Attack)
        {
            ctx.combatContext.snapshot.calcAttackValue += 8;
        }
    }
}

public class Relic_10 : ValueRelicEffectBase
{
    public Relic_10() : base(10) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.slotRole == DiceSlotRole.Attack && ctx.slotSnapshot.totalDiceCount == 0)
            ctx.combatContext.snapshot.calcAttackValue += 10;
    }
}

public class Relic_11 : ValueRelicEffectBase
{
    public Relic_11() : base(11) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.combatContext.snapshot.penaltyDiceCount >= 6)
        {
            int bonus = ctx.slotSnapshot.basicDiceCount + ctx.slotSnapshot.loanDiceCount;
            if (ctx.slotRole == DiceSlotRole.Attack)
                ctx.combatContext.snapshot.calcAttackValue += bonus;
            else
                ctx.combatContext.snapshot.calcDefenseValue += bonus;
        }
    }
}

public class Relic_20 : ValueRelicEffectBase
{
    public Relic_20() : base(20) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.slotSnapshot.eyeMap.Count == 6)
        {
            if (ctx.slotRole == DiceSlotRole.Attack)
                ctx.combatContext.snapshot.calcAttackValue *= 2;
            else
                ctx.combatContext.snapshot.calcDefenseValue *= 2;
        }
    }
}

public class Relic_28 : ValueRelicEffectBase
{
    public Relic_28() : base(28) { }
    public override ValueStage Stage => ValueStage.ValueConditionCheck;

    public override void Activate(ValueContext ctx)
    {
        if (!ctx.slotSnapshot.eyeMap.TryGetValue(4, out var eye)) return;
        ctx.combatContext.snapshot.calcAttackValue += eye.totalCount * 4;
    }
}

// ---------------- 수치계산 - 3 (FinalValueEffect) ----------------

public class Relic_26 : ValueRelicEffectBase
{
    public Relic_26() : base(26) { }
    public override ValueStage Stage => ValueStage.FinalValueEffect;

    public override void Activate(ValueContext ctx)
    {
        if (ctx.slotRole == DiceSlotRole.Attack)
        {
            ctx.combatContext.snapshot.calcAttackValue += ctx.slotSnapshot.totalDiceCount * 4;
        }
    }
}

public class Relic_29 : ValueRelicEffectBase
{
    public Relic_29() : base(29) { }
    public override ValueStage Stage => ValueStage.FinalValueEffect;

    public override void Activate(ValueContext ctx)
    {
        int bonus = ctx.combatContext.snapshot.calcDefenseValue / 3;
        ctx.combatContext.snapshot.calcAttackValue += bonus;
    }
}
