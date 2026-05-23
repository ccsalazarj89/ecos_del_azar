using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dice_manager : MonoBehaviour
{
   // public TextMeshProUGUI player_Dice;
    public TextMeshProUGUI NPC_Dice;
    public TextMeshProUGUI text;
    public GameObject npc_text;
    public string newvalor = "";
    public int aux;
    public GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // player.text = GetComponent<player_text>();
        //player_Dice = GetComponent<TextMeshProUGUI>().text;
        //NPC_Dice.text = this.npc_text.GetComponent<Text>().text;
        // int.Parse(player_Dice.text);
        // int.Parse(NPC_Dice.text);
        //player_Dice.text = "200";
        newvalor = "200";
        text.text = "hola que tal";
    }

    // Update is called once per frame
    void Update()
    {
        string prueba = "200";
        aux = int.Parse(prueba); 
        Debug.Log("el valor de " + aux);
        //text.text= player_Dice.text;
        text.text = prueba;

    }
}
