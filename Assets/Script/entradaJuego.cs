using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class entradaJuego : MonoBehaviour
{
    public GameObject canvasDados;
    private bool panelActivado = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current[Key.E].wasPressedThisFrame && panelActivado==true)
        {
            SceneManager.LoadScene(6);
        }


    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            canvasDados.SetActive(true);
            panelActivado = true;
        }
            
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            canvasDados.SetActive(false);
            panelActivado = false;
        }

    }
}
