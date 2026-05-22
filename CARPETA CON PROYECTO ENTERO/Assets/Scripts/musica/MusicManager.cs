using SimpleFileBrowser;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public string rutaCarpeta;

    void Awake()
    {
        rutaCarpeta = System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.MyMusic
        );
    }


    public AudioSource audioSource;
    public AudioClip[] canciones;

    AudioClip[] cancionesExplorador;
    
    AudioClip[] cancionesDisponibles;
    int indiceExplorador = 0;
    int selectorExplorador = 0;

    // MI MÚSICA
    public Image[] selectoresMiMusica;
    public Image[] selectoresCancionesMiMusica;

    // SUBIR CANCIONES
    public Image[] selectoresSubir;
    public Image[] selectoresCancionesSubir;

    public GameObject botonPrefab;
    public RectTransform content;

    public RectTransform contentExplorador;

    public Image botonPlayImagen;
    public Sprite iconoPlay;
    public Sprite iconoPausa;

    int botonActual = 0;
    int indiceCancion = 0;
    int selectorActual = 0;

    public int maxVisible = 4;

    bool estaReproduciendo = false;

    public float alturaBoton = 40f;

    public ScrollRect scrollRect;

    public TextMeshProUGUI textoCancionActual;
    public TextMeshProUGUI textoEstado;

    bool estabaSonando = false;

    public GameObject panelSubirCanciones;
    public GameObject panelMiMusica;

    bool enSubirCanciones = false;

    UdpClient client;
    IPEndPoint remoteEndPoint;

    void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient(25000);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        if (canciones.Length > 0)
        {
            audioSource.clip = canciones[indiceCancion];
        }

        ActualizarSelectores();
        GenerarListaCanciones();
        ActualizarSelectorCancion();
        ActualizarUI();
        content.anchoredPosition = Vector2.zero;
    }

    void Update()
    {
        if (client.Available > 0)
        {
            byte[] data = client.Receive(ref remoteEndPoint);
            int señal = data[0];
            ProcesarEMG(señal);
        }

        if (audioSource.clip != null)
        {
            if (estaReproduciendo && !audioSource.isPlaying && estabaSonando)
            {
                // terminó la canción → siguiente
                Next();
            }

            estabaSonando = audioSource.isPlaying;
        }
    }

    void ProcesarEMG(int señal)
    {
        if (señal == 1)
            SiguienteBoton();
        else if (señal == 2)
            BotonAnterior();
        else if (señal == 3)
            ClickBoton();
    }

    void GenerarListaCanciones()
    {
        for (int i = 0; i < canciones.Length; i++)
        {
            GameObject boton = Instantiate(botonPrefab, content);

            TextMeshProUGUI texto = boton.GetComponentInChildren<TextMeshProUGUI>();
            texto.text = canciones[i].name;

            int index = i;

            boton.GetComponent<Button>().onClick.AddListener(() =>
            {
                indiceCancion = index;
                PlayPause();
            });
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    public void SiguienteBoton()
    {
        Image[] arrayActual;

        if (enSubirCanciones)
            arrayActual = selectoresSubir;
        else
            arrayActual = selectoresMiMusica;

        botonActual++;

        if (botonActual >= arrayActual.Length)
            botonActual = 0;

        ActualizarSelectores();
    }

    public void BotonAnterior()
    {
        Image[] arrayActual;

        if (enSubirCanciones)
            arrayActual = selectoresSubir;
        else
            arrayActual = selectoresMiMusica;

        botonActual--;

        if (botonActual < 0)
            botonActual = arrayActual.Length - 1;

        ActualizarSelectores();
    }

    void ActualizarSelectores()
    {
        Image[] arrayActual;

        if (enSubirCanciones)
            arrayActual = selectoresSubir;
        else
            arrayActual = selectoresMiMusica;

        for (int i = 0; i < arrayActual.Length; i++)
        {
            Color c = arrayActual[i].color;
            c.a = (i == botonActual) ? 1f : 0f;
            arrayActual[i].color = c;
        }
    }

    void ActualizarSelectorCancion()
    {
        Image[] arrayActual;

        if (enSubirCanciones)
            arrayActual = selectoresCancionesSubir;
        else
            arrayActual = selectoresCancionesMiMusica;

        for (int i = 0; i < maxVisible; i++)
        {
            Color c = arrayActual[i].color;
            c.a = (i == selectorActual) ? 1f : 0f;
            arrayActual[i].color = c;
        }
    }

    void ScrollAbajo()
    {
        if (enSubirCanciones)
            contentExplorador.anchoredPosition += new Vector2(0, alturaBoton);
        else
            content.anchoredPosition += new Vector2(0, alturaBoton);
    }

    void ScrollArriba()
    {
        if (enSubirCanciones)
            contentExplorador.anchoredPosition -= new Vector2(0, alturaBoton);
        else
            content.anchoredPosition -= new Vector2(0, alturaBoton);
    }

    void Bajar()
    {
        if (indiceCancion < canciones.Length - 1)
        {
            indiceCancion++;

            if (selectorActual < maxVisible - 1)
                selectorActual++;
            else
                ScrollAbajo();

            ActualizarSelectorCancion();
        }
    }

    void Subir()
    {
        if (indiceCancion > 0)
        {
            indiceCancion--;

            if (selectorActual > 0)
                selectorActual--;
            else
                ScrollArriba();

            ActualizarSelectorCancion();
        }
    }

    void PlayPause()
    {
        if (canciones.Length == 0) return;

        if (!estaReproduciendo)
        {
            audioSource.clip = canciones[indiceCancion];
            audioSource.Play();

            botonPlayImagen.sprite = iconoPausa;
            estaReproduciendo = true;
            estabaSonando = true;
        }
        else
        {
            audioSource.Pause();

            botonPlayImagen.sprite = iconoPlay;
            estaReproduciendo = false;
            estabaSonando = false;
        }

        ActualizarUI();
    }

    void Next()
    {
        indiceCancion++;

        if (indiceCancion >= canciones.Length)
        {
            ResetScrollYSelector();
        }
        else
        {
            if (selectorActual < maxVisible - 1)
            {
                selectorActual++;
            }
            else
            {
                ScrollAbajo();
            }

            ActualizarSelectorCancion();
        }

        audioSource.clip = canciones[indiceCancion];
        audioSource.Play();

        botonPlayImagen.sprite = iconoPausa;
        estaReproduciendo = true;
        estabaSonando = true;
        ActualizarUI();
    }

    void ResetScrollYSelector()
    {
        indiceCancion = 0;
        selectorActual = 0;

        Vector2 pos = content.anchoredPosition;
        pos.y = 0;
        content.anchoredPosition = pos;

        ActualizarSelectorCancion();
    }
    void Prev()
    {
        indiceCancion--;

        if (indiceCancion < 0)
        {
            ResetScrollYSelectorToLast();
        }
        else
        {
            if (selectorActual > 0)
            {
                selectorActual--;
            }
            else
            {
                ScrollArriba();
            }

            ActualizarSelectorCancion();
        }

        audioSource.clip = canciones[indiceCancion];
        audioSource.Play();

        botonPlayImagen.sprite = iconoPausa;
        estaReproduciendo = true;
        estabaSonando = true;
        ActualizarUI();
    }

    void ResetScrollYSelectorToLast()
    {
        indiceCancion = canciones.Length - 1;
        selectorActual = maxVisible - 1;

        // Ir al final del scroll de forma dinámica
        scrollRect.verticalNormalizedPosition = 0f;

        ActualizarSelectorCancion();
    }

    void ClickBoton()
    {
        if (!enSubirCanciones)
        {
            // PANTALLA MI MÚSICA
            if (botonActual == 1)
                IrASubirCanciones();
            else if (botonActual == 2)
                Subir();
            else if (botonActual == 3)
                Bajar();
            else if (botonActual == 4)
                Prev();
            else if (botonActual == 5)
                PlayPause();
            else if (botonActual == 6)
                Next();
            else if (botonActual == 7)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Pantalla inicio");
            else if (botonActual == 8)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Pantalla inicio");
        }
        else
        {
            // PANTALLA SUBIR CANCIONES
            if (botonActual == 0)
                IrAMiMusica();
            else if (botonActual == 1)
                CargarCancionesDesdeCarpeta();
            else if (botonActual == 2)
                SubirExplorador();
            else if (botonActual == 3)
                BajarExplorador();
            else if (botonActual == 4)
                AñadirDesdeExplorador(); // botón subir canción
            else if (botonActual == 5)
                IrAMiMusica(); // botón atrás
            else if (botonActual == 6)
                UnityEngine.SceneManagement.SceneManager.LoadScene("Pantalla inicio");
        }
    }

    void ActualizarUI()
    {
        if (canciones.Length == 0) return;

        textoCancionActual.text = canciones[indiceCancion].name;

        if (estaReproduciendo)
            textoEstado.text = "Reproduciendo";
        else
            textoEstado.text = "Pausa";
    }


    void IrASubirCanciones()
    {
        panelMiMusica.SetActive(false);
        panelSubirCanciones.SetActive(true);

        enSubirCanciones = true;

        botonActual = 0;
        ActualizarSelectores();
        selectorActual = 0;
        ActualizarSelectorCancion();

        CargarCancionesDesdeCarpeta();

        indiceExplorador = 0;
        selectorExplorador = 0;
        ActualizarSelectorExplorador();

        contentExplorador.anchoredPosition = Vector2.zero;
    }

    void IrAMiMusica()
    {
        panelMiMusica.SetActive(true);
        panelSubirCanciones.SetActive(false);

        enSubirCanciones = false;

        botonActual = 0;
        ActualizarSelectores();
        selectorActual = 0;
        ActualizarSelectorCancion();
    }

    void CargarCancionesDesdeCarpeta()
    {
        string[] archivos = Directory.GetFiles(rutaCarpeta, "*.mp3");

        cancionesDisponibles = new AudioClip[archivos.Length];

        for (int i = 0; i < archivos.Length; i++)
        {
            StartCoroutine(CargarAudioExplorador(archivos[i], i));
        }

        Debug.Log("Ruta usada: " + rutaCarpeta);
        Debug.Log("Existe carpeta: " + Directory.Exists(rutaCarpeta));
    }

    IEnumerator CargarAudioExplorador(string ruta, int index)
    {
        using (UnityEngine.Networking.UnityWebRequest www =
            UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + ruta, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                clip.name = Path.GetFileNameWithoutExtension(ruta);

                cancionesDisponibles[index] = clip;

                StartCoroutine(EsperarYGenerar());

            }
        }
    }

    IEnumerator EsperarYGenerar()
    {
        yield return new WaitForSeconds(0.2f);
        GenerarListaExplorador();
    }

    void GenerarListaExplorador()
    {
        foreach (Transform hijo in contentExplorador)
            Destroy(hijo.gameObject);

        for (int i = 0; i < cancionesDisponibles.Length; i++)
        {
            if (cancionesDisponibles[i] == null) continue;

            GameObject boton = Instantiate(botonPrefab, contentExplorador);

            TextMeshProUGUI texto = boton.GetComponentInChildren<TextMeshProUGUI>();
            texto.text = cancionesDisponibles[i].name;
        }

        AjustarAlturaExplorador(); 
    }

    void AjustarAlturaExplorador()
    {
        if (cancionesDisponibles == null) return;

        RectTransform rt = contentExplorador.GetComponent<RectTransform>();

        float altura = cancionesDisponibles.Length * 43f;

        rt.sizeDelta = new Vector2(rt.sizeDelta.x, altura);
    }

    void SeleccionarCancionExplorador()
    {
        if (cancionesExplorador == null || cancionesExplorador.Length == 0)
            return;

        AñadirCancion(cancionesExplorador[indiceCancion]);
    }

    void AñadirDesdeExplorador()
    {
        if (cancionesDisponibles == null || cancionesDisponibles.Length == 0)
        {
            Debug.Log("No hay canciones cargadas");
            return;
        }

        AudioClip seleccionada = cancionesDisponibles[indiceExplorador];

        AñadirCancion(seleccionada);

        List<AudioClip> lista = new List<AudioClip>(cancionesDisponibles);
        lista.RemoveAt(indiceExplorador);
        cancionesDisponibles = lista.ToArray();

        GenerarListaExplorador();
        AjustarAlturaExplorador();
        contentExplorador.anchoredPosition = Vector2.zero;
        IrAMiMusica();
    }

    void AñadirCancion(AudioClip nueva)
    {
        // Crear nueva lista
        AudioClip[] nuevasCanciones = new AudioClip[canciones.Length + 1];

        // meter nueva en la primera posición
        nuevasCanciones[0] = nueva;

        // mover las anteriores
        for (int i = 0; i < canciones.Length; i++)
        {
            nuevasCanciones[i + 1] = canciones[i];
        }

        canciones = nuevasCanciones;

        // Aumentar tamaño del Content
        RectTransform rt = content.GetComponent<RectTransform>();

        float incremento = 40f; // altura botón
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + incremento);

        // reiniciar lista visual
        RefrescarLista();
    }

    void RefrescarLista()
    {
        // borrar botones antiguos
        foreach (Transform hijo in content)
        {
            Destroy(hijo.gameObject);
        }

        // volver a generar lista
        GenerarListaCanciones();

        // reset visual
        ResetScrollYSelector();
    }
    void ActualizarSelectorExplorador()
    {
        for (int i = 0; i < maxVisible; i++)
        {
            Color c = selectoresCancionesSubir[i].color;
            c.a = (i == selectorExplorador) ? 1f : 0f;
            selectoresCancionesSubir[i].color = c;
        }
    }
    void BajarExplorador()
    {
        if (indiceExplorador < cancionesDisponibles.Length - 1)
        {
            indiceExplorador++;

            if (selectorExplorador < maxVisible - 1)
                selectorExplorador++;
            else
                contentExplorador.anchoredPosition += new Vector2(0, alturaBoton);

            ActualizarSelectorExplorador();
        }
    }

    void SubirExplorador()
    {
        if (indiceExplorador > 0)
        {
            indiceExplorador--;

            if (selectorExplorador > 0)
                selectorExplorador--;
            else
                contentExplorador.anchoredPosition -= new Vector2(0, alturaBoton);

            ActualizarSelectorExplorador();
        }
    }

    
    void OnDestroy()
    {
        if (client != null)
            client.Close();
    }
}