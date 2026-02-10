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