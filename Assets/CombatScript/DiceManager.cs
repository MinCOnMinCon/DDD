using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    public class DiceState
    {
        public int normalDiceNum { get => normalDiceNum;
            set 
            {
                normalDiceNum = Mathf.Max(0, normalDiceNum - value);
            }
        }
        public int penaltyDiceNum
        {
            get => penaltyDiceNum;
            set
            {
                penaltyDiceNum = Mathf.Max(0, penaltyDiceNum - value);
            }
        }
        public int loanDiceNum
        {
            get => loanDiceNum;
            set
            {
                loanDiceNum = Mathf.Max(0,loanDiceNum - value);
            }
        }
        public int basicPenaltyDicePerLoan { get; private set; } // 기본 대출 당 패널티 주사위 획득 개수
        public int curPenaltyDicePerLoan { get; set; } // 현재 대출 당 패널티 주사위 획득 개수
        public int normalDicePerLoan { get; set; }
        public int loanNum { get; set; } // 이번 턴 대출 횟수

        public int[] basicDiceWeight { get; set; }
        public int[] penaltyDiceWeight { get; set; }
        public int[] loanDiceWeight { get; set; }
        public List<Dice> existingDiceList { get; set; }

        public DiceState(DiceInitValue div)
        {
            normalDiceNum = div.normalDiceNum;
            penaltyDiceNum = div.penaltyDiceNum;
            loanDiceNum = div.loanDiceNum;
            basicPenaltyDicePerLoan = div.basicPenaltyDicePerLoan;
            curPenaltyDicePerLoan = div.curPenaltyDicePerLoan;
            normalDicePerLoan = div.normalDicePerLoan;
            loanNum = div.loanNum;
            basicDiceWeight = new int[div.diceEyeNum+1];
            penaltyDiceWeight = new int[div.diceEyeNum+1];
            loanDiceWeight = new int[div.diceEyeNum+1];
            existingDiceList = new List<Dice>();
        }

    }
    [SerializeField]
    private DiceInitValue div;
    private DiceState diceState; 
    private void Awake()
    {
        diceState = new DiceState(div); 
    }

    public void RollDice()
    {

    }
}

public class Dice
{
    public enum DiceType 
    {
        normal,
        penalty,
        loan
    }

    public int diceEye { get; private set; }
    public int diceValue { get; set; }
    public DiceType diceType { get; private set; }
    public int duration { get; set; } // 주사위가 몇턴동안 지속했는지 보여주는 값
}
