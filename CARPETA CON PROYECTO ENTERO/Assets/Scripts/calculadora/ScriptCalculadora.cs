using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using TMPro;

public class CalculadoraManager : MonoBehaviour
{
    public Image[] selectores; // Los cuadros negros
    public TMP_Text pantallaTexto; // Texto de arriba

    private int indice = 0;

    private string entrada = "";

    UdpClient client;
    IPEndPoint remoteEndPoint;

    void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient(25000); 
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        ActualizarSelector();
    }

    void Update()
    {
        
        if (client.Available > 0)
        {
            byte[] data = client.Receive(ref remoteEndPoint);
            int señal = data[0];

            ProcesarEMG(señal);
        }
    }

    void ProcesarEMG(int señal)
    {
        if (señal == 1)
            Derecha();
        else if (señal == 2)
            Izquierda();
        else if (señal == 3)
            Seleccionar();
    }

    void Derecha()
    {
        indice++;
        if (indice >= selectores.Length)
            indice = 0;

        ActualizarSelector();
    }

    void Izquierda()
    {
        indice--;
        if (indice < 0)
            indice = selectores.Length - 1;

        ActualizarSelector();
    }

    void Seleccionar()
    {
        string nombre = selectores[indice].name;

        Debug.Log("Boton" + nombre);

        switch (nombre)
        {
            case "AC":
                entrada = "";
                break;

            case "Borrar":
                if (entrada.Length > 0)
                    entrada = entrada.Substring(0, entrada.Length - 1);
                break;

            case "=":
                Calcular();
                return;

            case "+":
            case "-":
            case "x":
            case "�":
                entrada += " " + nombre + " ";
                break;
            case "Home":
            case "Atras":
                UnityEngine.SceneManagement.SceneManager.LoadScene("Pantalla inicio");
                break;

            default:
                // n�meros o punto
                entrada += nombre;
                break;
        }

        pantallaTexto.text = entrada;
    }

    void Calcular()
    {
        try
        {
            string operacion = entrada.Replace("x", "*").Replace("�", "/");

            var resultado = new System.Data.DataTable().Compute(operacion, null);

            entrada = resultado.ToString();
            pantallaTexto.text = entrada;
        }
        catch
        {
            pantallaTexto.text = "Error";
            entrada = "";
        }
    }

    void ActualizarSelector()
    {
        for (int i = 0; i < selectores.Length; i++)
        {
            Color c = selectores[i].color;
            c.a = (i == indice) ? 1f : 0f;
            selectores[i].color = c;
        }
    }

    void OnDestroy()
    {
        if (client != null)
            client.Close();
    }
}