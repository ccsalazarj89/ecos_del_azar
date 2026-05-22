using UnityEngine;
using UnityEngine.UI;

public class Dice_manager : MonoBehaviour
{
    public Text player_Dice;
    public Text NPC_Dice;
    public GameObject player_text;
    public GameObject npc_text;
    public GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // player.text = GetComponent<player_text>();
        player_Dice.text = this.player_text.GetComponent<Text>().text;
        NPC_Dice.text = this.npc_text.GetComponent<Text>().text;
        int.Parse(player_Dice.text);
        int.Parse(NPC_Dice.text);
    }

    // Update is called once per frame
    void Update()
    {
        
        Debug.Log("el valor de " + player_Dice.text);

    }
}
