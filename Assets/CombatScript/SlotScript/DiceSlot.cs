using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

public class DiceSlot : MonoBehaviour, ICombatContextProvider
{
    [SerializeField] private DiceSlotRole role;

    protected List<GameObject> enteredDiceList;
    protected bool isIn;
    protected bool isOut;
 

    private void Awake()
    {
        enteredDiceList = new List<GameObject>();
        isIn = false;
        isOut = false;
    }

    private void Start()
    {
        CombatManager.inst.SlotRegister(this);
        ApplyTo(CombatManager.inst.combatContext);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Dice"))
        {
            enteredDiceList.Add(collision.gameObject);
            isIn = true;
            if(role == DiceSlotRole.Saving)
            {
               ApplyTo(CombatManager.inst.combatContext);
            }
            CombatManager.inst.CommitSlotChanges();



        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Dice"))
        {
            enteredDiceList.Remove(collision.gameObject);
            isOut = true;
            if (role == DiceSlotRole.Saving)
            {
                ApplyTo(CombatManager.inst.combatContext);
            }
            CombatManager.inst.CommitSlotChanges();



        }
    }
    public void ApplyTo(CombatContext ctx)
    {
        switch (role)
        {
            case DiceSlotRole.Attack:
                ctx.attackSlotDiceList = new List<GameObject>(enteredDiceList);
                break;

            case DiceSlotRole.Defense:
                ctx.defenseSlotDiceList = new List<GameObject>(enteredDiceList);
                break;

            case DiceSlotRole.Saving:
                ctx.savingSlotDiceList = new List<GameObject>(enteredDiceList);
                break;
        }
    } // ctx에 존재하는 diceSlotlist는 최초 호출 시 적어도 하나는 null 상태다. 왜냐하면 
    public bool ConsumeFlag()
    {
        bool flag;
        if(isIn ^ isOut)
        {
            flag = true;
        }
        else
        {
            flag = false;
        }
        isIn = false;
        isOut = false;
        return flag;

    }
}
public enum DiceSlotRole
{
    Attack,
    Defense,
    Saving
}
