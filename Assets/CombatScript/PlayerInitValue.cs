using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInitValue", menuName = "Scriptable Objects/PlayerInitValue")]
public class PlayerInitValue : ScriptableObject
{
    public int maxHp = 100;
    public int savingMaximumValue = 20;
    public int savingMaximumDice = 3;
    public int money = 50;
}
