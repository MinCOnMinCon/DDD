using System.Collections.Generic;
using System;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    public static RelicManager inst;
    [SerializeField]
    private List<RelicData> ownedRelics; // 가지고 있는 유물 리스트
    private CombatRelicContext combatRelics;
    private static readonly Dictionary<int, Func<RelicEffect>> factory // 모든 유물의 id와 그 유물의 RelicEffect 객체를 생성하는 함수를 담은 델리게이트 변수의 딕셔너리
        = new()
        {
          // ======================
        // Value Relics
        // ======================
        { 4,  () => new Relic_4() },
        { 6,  () => new Relic_6() },
        { 10, () => new Relic_10() },
        { 11, () => new Relic_11() },
        { 20, () => new Relic_20() },
        { 22, () => new Relic_22() },
        { 26, () => new Relic_26() },
        { 28, () => new Relic_28() },
        { 29, () => new Relic_29() },
        { 31, () => new Relic_31() },
        { 32, () => new Relic_32() },

        // ======================
        // Combo Relics
        // ======================
        { 8,  () => new Relic_8() },
        { 9,  () => new Relic_9() },
        { 14, () => new Relic_14() },
        { 18, () => new Relic_18() },
        { 23, () => new Relic_23() },
        { 24, () => new Relic_24() },
        { 25, () => new Relic_25() },
        { 27, () => new Relic_27() },

        // ======================
        // Saving Relics
        // ======================
        { 3,  () => new Relic_3() },
        { 19, () => new Relic_19() },
        { 30, () => new Relic_30() }
        };

    private void Awake()
    {
        inst = this;
        combatRelics = new CombatRelicContext();
        //ownedRelics = new List<RelicData>();

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

/// <summary>
/// 유물 효과의 공동적인 속성을 가지는 클래스
/// 유물 효과면 이 클래스를 상속받아야 하고 유물 효과에 필요한 변수나 함수를 가진다.
/// </summary>
public abstract class RelicEffect 
{
    public int RelicId { get; }

    protected RelicEffect(int relicId)
    {
        RelicId = relicId;
    }

    
}

/// <summary>
/// 유물의 공동적인 함수를 담고 있는 인터페이스. 
/// 유물이면 다 똑같이 가져야 하는 함수를 담고 있고 유물이라면 이 인터페이스를 상속받아야 한다.
/// </summary>
public interface IRelic
{
    bool CanAffect(DiceSlotRole slotRole);
}

public class CombatRelicContext
{
    public Dictionary<int, RelicEffect> activeEffects
        = new Dictionary<int, RelicEffect>(); // 가지고 있는 유물의 효과를 가지는 딕셔너리
}