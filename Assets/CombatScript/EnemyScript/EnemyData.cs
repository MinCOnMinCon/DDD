using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public int id;
    public string enemyName;

    public int maxHp;
    public int baseAttack;
    public int baseDefense;

    public int difficulty;
    public int rewardValue;

    public string description;
    public Sprite image;
}