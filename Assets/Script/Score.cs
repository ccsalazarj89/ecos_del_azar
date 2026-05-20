using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    DiceRoll dice;
    [SerializeField] Text scoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        dice = FindFirstObjectByType<DiceRoll>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(dice != null)
        {
            if (dice.diceFaceNum !=0)
            {
                scoreText.text = dice.diceFaceNum.ToString();
            }
        }   
    }
}
