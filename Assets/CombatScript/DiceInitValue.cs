using UnityEngine;

[CreateAssetMenu(fileName = "DiceInitValue", menuName = "Scriptable Objects/DiceInitValue")]
public class DiceInitValue : ScriptableObject
{
    public int normalDiceNum = 3;
    public int penaltyDiceNum = 0;
    public int loanDiceNum = 0;
    public int basicPenaltyDicePerLoan = 1;
    public int curPenaltyDicePerLoan = 0;
    public int normalDicePerLoan = 2;
    public int loanNum = 0;
    public int diceEyeNum = 6; // n 면 주사위 -> 현재는 6면
}
