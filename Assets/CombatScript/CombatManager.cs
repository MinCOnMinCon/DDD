using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    private CombatContext CombatContext;
    public static CombatManager inst { get; private set; }

    private void Awake()
    {
        inst = new CombatManager();
        CombatContext = new CombatContext();
    }

}
public class CombatContext 
{
    public DiceState diceState { get; private set; }
    public PlayerState playerState { get; private set; }

    public List<GameObject> attackSlotDiceList { get; private set; }
    public List<GameObject> defenseSlotDiceList { get; private set; }
    public List<GameObject> savingSlotDiceList { get; private set; }

}
interface ICombatHook
{
    void OnCombatPhase(CombatPhase phase, CombatContext ctx);
}

public enum CombatPhase 
{ 

}
