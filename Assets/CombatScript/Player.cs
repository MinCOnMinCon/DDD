using System;
using System.Data.SqlTypes;
using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField]
    private PlayerInitValue piv;
    private DiceManager diceM;
    private RelicManager relicM;
    public class PlayerState
    {
        private Player player;
        public int maxHp { set; private get; }
        public int hp
        {
            get => hp;
            set
            {
                if (value > 0)
                {
                    hp = Mathf.Max(0, hp - value);
                    player.TakeDamage();
                    if (hp <= 0)
                    {
                        player.Died();
                    }
                }
                else { hp = Mathf.Min(maxHp, hp - value); }
            }
        }
        public int money
        {
            get => money;
            set
            {
                money = Mathf.Max(0, money - value);
            }
        }

        public int savingMaximumValue { get; set; }
        public int savingMaximumDice { get; set; }

        public PlayerState(PlayerInitValue piv, Player player)
        {
            maxHp = piv.maxHp;
            hp = maxHp;
            money = piv.money;
            savingMaximumDice = piv.savingMaximumDice;
            savingMaximumValue = piv.savingMaximumValue;
            this.player = player;
        }
    }
    public PlayerState playerState { get; private set; }

    private void Awake()
    {
        playerState = new PlayerState(piv, this);
    }
    public void Died() 
    {
        OnPlayerDied?.Invoke();
    }
    public void TakeDamage()
    {
        OnPlayerDamaged?.Invoke();
    }

    public event Action OnPlayerDied;
    public event Action OnPlayerDamaged;

}

