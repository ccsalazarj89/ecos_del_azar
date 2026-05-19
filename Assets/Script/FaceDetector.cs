using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class FaceDetector : MonoBehaviour
{
    DiceRoll dice;
    private void Awake()
    {
        dice = FindObjectOfType<DiceRoll>();
    }

    private void OnTriggerStay(Collider other)
    {
        if(dice != null)
        {
            if(dice.GetComponent<Rigidbody>().angularVelocity == Vector3.zero)
            {
                dice.diceFaceNum = int.Parse(other.name);
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
