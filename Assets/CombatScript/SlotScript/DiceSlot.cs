using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class DiceSlot : MonoBehaviour
{
    private List<GameObject> enteredDiceList;
    /*private bool isDiceIn;
    private float diceInTime; // 주사위가 들어왔을 떄 들어와서 있는 시간을 측정 (주사위를 빠르게 넣다뺐다 해서 계속 계산 함수 호출하는 거 막기 위함)
    private float diceOutTime;*/

    private void Awake()
    {
        enteredDiceList = new List<GameObject>();
        
        
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Dice"))
        {
            enteredDiceList.Add(collision.gameObject);
            
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Dice"))
        {
            enteredDiceList.Remove(collision.gameObject);
          
        }
    }
   
    public List<GameObject> GetSlotDiceList()
    {
        return enteredDiceList; 
    }
    
}
