using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DiceSlot : MonoBehaviour
{
    private List<GameObject> enteredDiceList;
    private bool isDiceIn; 

    private void Awake()
    {
        enteredDiceList = new List<GameObject>();
        isDiceIn = false;
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
