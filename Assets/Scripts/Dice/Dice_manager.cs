using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Dice_manager : MonoBehaviour
{
   // public TextMeshProUGUI player_Dice;
    public TextMeshProUGUI NPC_Dice;
    public TextMeshProUGUI text; 
    public TextMeshProUGUI Player_Dice;
    public GameObject npc_text;
    public int nuevoDinero = 0;
    public string newvalor = "";
    public int jugadorValor;
    public int NPCValor;
    public bool nuevaTirada=true;
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
        text.text = "1000";
    }

    // Update is called once per frame
    void Update()
    {
        //string prueba = "200";
       // aux = int.Parse(prueba); 
       // Debug.Log("el valor de " + aux);
        //text.text= player_Dice.text;
        //text.text = prueba;
       if (nuevaTirada==true)
        {
            /*jugadorValor = int.Parse(Player_Dice.text);
            NPCValor = int.Parse(NPC_Dice.text);
            if(jugadorValor <  NPCValor)
            {
                Debug.Log("jugador perdio");
            }
            else if(jugadorValor > NPCValor) {

                Debug.Log("jugador gano");
            
            }
            else if(jugadorValor == NPCValor)
            {
                Debug.Log("empate");
            }
            Debug.Log("la puntuacion de jugardor es " + jugadorValor + " y  el del npc es " + NPCValor);
            newvalor = "vamos que nos vamos";
            //Debug.Log(newvalor);
            nuevaTirada = false;
            //newvalor = "se fini";
           
            //if (valor)*/
           // StartCoroutine("calculoJugada");
            
        }

    }
    public void Abandonar()
    {
        SceneManager.LoadScene(0);
    }

    public void nuevaJugada()
    {
        //nuevaTirada = true;
        StartCoroutine("calculoJugada");

    }
    private IEnumerator calculoJugada()
    {
        Debug.Log("entro");
      //  if (nuevaTirada == true)
       // {
            jugadorValor = int.Parse(Player_Dice.text);
            NPCValor = int.Parse(NPC_Dice.text);
            if (jugadorValor < NPCValor)
            {
                Debug.Log("jugador perdio");
                nuevoDinero = int.Parse(text.text)-100;
            }
            else if (jugadorValor > NPCValor)
            {

                Debug.Log("jugador gano");

            }
            else if (jugadorValor == NPCValor)
            {
                Debug.Log("empate");
            }
            Debug.Log("la puntuacion de jugardor es " + jugadorValor + " y  el del npc es " + NPCValor);
            newvalor = "vamos que nos vamos";
            //Debug.Log(newvalor);
            //nuevaTirada = false;
      //  }
        nuevaTirada = false;
        yield return new WaitForSeconds(0.05f);

    }
}
