using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;

public class ConsejosSaludManager : MonoBehaviour
{
    public Image[] selectores;
    private int indice = 0;

    public GameObject marcoConsejosSalud;

    public GameObject pantallaAlimentacion;
    public GameObject pantallaEjercicio;
    public GameObject pantallaDescanso;
    public GameObject pantallaSaludMental;

    public AlimentacionManager alimentacionManager;
    public EjercicioManager ejercicioManager;
    public DescansoManager descansoManager;
    public SaludMentalManager saludMentalManager;

    private bool enMenu = true;

    UdpClient client;
    IPEndPoint remoteEndPoint;

    void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient(25000);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        enMenu = true;

        marcoConsejosSalud.SetActive(true);
        pantallaAlimentacion.SetActive(false);
        pantallaEjercicio.SetActive(false);
        pantallaDescanso.SetActive(false);
        pantallaSaludMental.SetActive(false);

        Actualizar();
    }

    void Update()
    {
        if (client.Available > 0)
        {
            byte[] data = client.Receive(ref remoteEndPoint);
            int senal = data[0];

            ProcesarEMG(senal);
        }
    }

    public void ProcesarEMG(int senal)
    {
        if (enMenu)
        {
            if (senal == 1)
            {
                Derecha();
            }
            else if (senal == 2)
            {
                Izquierda();
            }
            else if (senal == 3)
            {
                Seleccionar();
            }
        }
        else
        {
            if (pantallaAlimentacion.activeSelf && alimentacionManager != null)
            {
                alimentacionManager.ProcesarEMG(senal);
            }
            else if (pantallaEjercicio.activeSelf && ejercicioManager != null)
            {
                ejercicioManager.ProcesarEMG(senal);
            }
            else if (pantallaDescanso.activeSelf && descansoManager != null)
            {
                descansoManager.ProcesarEMG(senal);
            }
            else if (pantallaSaludMental.activeSelf && saludMentalManager != null)
            {
                saludMentalManager.ProcesarEMG(senal);
            }
        }
    }

    public void Derecha()
    {
        indice++;
        if (indice >= selectores.Length)
            indice = 0;

        Actualizar();
    }

    public void Izquierda()
    {
        indice--;
        if (indice < 0)
            indice = selectores.Length - 1;

        Actualizar();
    }

    public void Seleccionar()
    {
        string nombre = selectores[indice].name;
        Debug.Log("Seleccionado: " + nombre);

        if (nombre == "SelectorAlimentacion")
        {
            MostrarSolo(pantallaAlimentacion);
        }
        else if (nombre == "SelectorEjercicio")
        {
            MostrarSolo(pantallaEjercicio);
        }
        else if (nombre == "SelectorDescanso")
        {
            MostrarSolo(pantallaDescanso);
        }
        else if (nombre == "SelectorSaludMental")
        {
            MostrarSolo(pantallaSaludMental);
        }
        else if (nombre == "BotonAtras")
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
        else if (nombre == "BotonInicio")
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
    }

    void MostrarSolo(GameObject pantalla)
    {
        enMenu = false;

        marcoConsejosSalud.SetActive(false);

        pantallaAlimentacion.SetActive(false);
        pantallaEjercicio.SetActive(false);
        pantallaDescanso.SetActive(false);
        pantallaSaludMental.SetActive(false);

        pantalla.SetActive(true);
    }

    public void VolverAlMenu()
    {
        enMenu = true;

        pantallaAlimentacion.SetActive(false);
        pantallaEjercicio.SetActive(false);
        pantallaDescanso.SetActive(false);
        pantallaSaludMental.SetActive(false);

        marcoConsejosSalud.SetActive(true);
        Actualizar();
    }

    void Actualizar()
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