using System.Collections.Generic;
using System;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    public static RelicManager inst;
    [SerializeField]
    private RelicData testRelic;
    private List<RelicData> ownedRelics; // 가지고 있는 유물 리스트
    private CombatRelicContext combatRelics;
    private static readonly Dictionary<int, Func<RelicEffect>> factory // 모든 유물의 id와 그 유물의 RelicEffect 객체를 생성하는 함수를 담은 델리게이트 변수의 딕셔너리
        = new()
        {
            // -------------------------
        // 수치계산 - 1
        // -------------------------
        { 6,  () => new Relic_6()  },
        { 22, () => new Relic_22() },
        { 31, () => new Relic_31() },
        { 32, () => new Relic_32() },

        // -------------------------
        // 수치계산 - 2
        // -------------------------
        { 4,  () => new Relic_4()  },
        { 10, () => new Relic_10() },
        { 11, () => new Relic_11() },
        { 20, () => new Relic_20() },
        { 28, () => new Relic_28() },

        // -------------------------
        // 수치계산 - 3
        // -------------------------
        { 26, () => new Relic_26() },
        { 29, () => new Relic_29() },
        };

    private void Awake()
    {
        inst = this;
        combatRelics = new CombatRelicContext();
        ownedRelics = new List<RelicData>();

        //test
        ownedRelics.Add(testRelic);

        RefreshRelicEffects();
    }
    public void RefreshRelicEffects()
    {
        combatRelics.activeEffects.Clear();

        foreach (var relic in ownedRelics)
        {
            if (factory.TryGetValue(relic.id, out var creator))
            {
                combatRelics.activeEffects[relic.id] = creator();
            }
        }
    }

    public IEnumerable<T> GetRelicEffects<T>()
    {
        foreach (var effect in combatRelics.activeEffects.Values)
        {
            if (effect is T t)
                yield return t;
        }
    }
}

public abstract class RelicEffect
{
    public int RelicId { get; }

    protected RelicEffect(int relicId)
    {
        RelicId = relicId;
    }

    public virtual bool CanAffect(DiceSlotRole role) => true;
}
public interface IRelicEffect
{
    bool CanAffect(DiceSlotRole role);
}

public class CombatRelicContext
{
    public Dictionary<int, RelicEffect> activeEffects
        = new Dictionary<int, RelicEffect>(); // 가지고 있는 유물의 효과를 가지는 딕셔너리
}