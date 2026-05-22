using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using SimpleFileBrowser;
using TMPro;
using Unity.VisualScripting;

public class fotosManager : MonoBehaviour
{
    [Header("Pantallas")]
    public GameObject menu_1;
    public GameObject menu_2;
    public GameObject[] fotos;
    public GameObject ExploradorPanel;
    public GameObject FotoAnyadir;
    public GameObject MenuExternasPanel;
    public GameObject fotoExterna;

    [Header("Selectores UI")]
    public Image[] selectores;
    private int indice = 0;

    [Header("Imágenes externas (IMPORTADAS)")]
    private List<Texture2D> imagenesExternas = new List<Texture2D>();
    public Image imagenUI;

    private List<Sprite> galeriaFotos = new List<Sprite>(); //para que añada las fotos normales y las externas

    Texture2D imagenSeleccionada;

    [Header("Explorador EMG")]
    public Transform contenedorUI;
    public GameObject botonPrefab;

    UdpClient client;
    IPEndPoint remoteEndPoint;

    // --- PAGINACIÓN MENÚ EXTERNAS ---
    private int paginaExternas = 0;
    private const int FOTOS_POR_PAGINA = 6;
    private int TotalPaginasExternas => Mathf.CeilToInt((float)imagenesExternas.Count / FOTOS_POR_PAGINA);

    public enum EstadoPantalla
    {
        Menu1,
        Menu2,
        Fotos,
        Explorador,
        AddFoto,
        MenuExternas
    }

    private EstadoPantalla estadoActual;
    private int fotoActual = 0;

    private List<string> rutas = new List<string>();
    private int indiceExplorador = 0;
    private string rutaActual = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);

    private bool viendoImagenExterna = false;

    private bool all_photos_added = false;


    void Start()
    {
        Application.runInBackground = true;

        client = new UdpClient(25000);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        // cargar fotos iniciales
        foreach (GameObject go in fotos)
        {
            if (go.TryGetComponent<Image>(out Image img))
            {
                galeriaFotos.Add(img.sprite);
            }
        }

        all_photos_added = false;

        DesactivarTodo();
        menu_1.SetActive(true);
        estadoActual = EstadoPantalla.Menu1;

        Actualizar();
    }

    void Update()
    {
        if (client != null && client.Available > 0)
        {
            byte[] data = client.Receive(ref remoteEndPoint);
            ProcesarEMG(data[0]);
        }

        if (estadoActual == EstadoPantalla.Fotos)
        {
            for (int i = 0; i <= 1; i++)
                selectores[i].gameObject.SetActive(true);
            for (int i = 2; i < 9; i++)
                selectores[i].gameObject.SetActive(false);
            for (int i = 11; i <= 16; i++)
                selectores[i].gameObject.SetActive(false);
        }
        else if (estadoActual == EstadoPantalla.Explorador)
        {
            for (int i = 0; i < 9; i++)
                selectores[i].gameObject.SetActive(false);

            if (rutas.Count < 5)
            {
                for (int i = 11; i < 11 + rutas.Count; i++)
                    selectores[i].gameObject.SetActive(true);
            }
            else
            {
                for (int i = 11; i <= 15; i++)
                    selectores[i].gameObject.SetActive(true);
            }
            selectores[16].gameObject.SetActive(false);
        }
        else if (estadoActual == EstadoPantalla.AddFoto)
        {
            for (int i = 0; i < 9; i++)
                selectores[i].gameObject.SetActive(false);
            for (int i = 11; i < 16; i++)
                selectores[i].gameObject.SetActive(false);
            selectores[16].gameObject.SetActive(true);
        }
        else if (estadoActual == EstadoPantalla.MenuExternas)
        {
            // Selectores 0 y 1: flechas de paginación (solo si hay más de una página)
            selectores[0].gameObject.SetActive(TotalPaginasExternas > 1);
            selectores[1].gameObject.SetActive(TotalPaginasExternas > 1);

            // Ocultar todos los selectores de foto primero
            for (int i = 2; i < 9; i++)
                selectores[i].gameObject.SetActive(false);

            // Activar solo los que corresponden a fotos en la página actual
            int inicio = paginaExternas * FOTOS_POR_PAGINA;
            int fotosEnPagina = Mathf.Min(FOTOS_POR_PAGINA, imagenesExternas.Count - inicio);
            for (int i = 0; i < 2 + fotosEnPagina; i++)
                selectores[i].gameObject.SetActive(true);

            for (int i = 11; i <= 16; i++)
                selectores[i].gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i <= 1; i++)
                selectores[i].gameObject.SetActive(true);
            for (int i = 2; i < 9; i++)
                selectores[i].gameObject.SetActive(true);
            for (int i = 11; i <= 16; i++)
                selectores[i].gameObject.SetActive(false);
        }
    }

    void ProcesarEMG(int señal)
    {
        if (señal == 1) Derecha();
        else if (señal == 2) Izquierda();
        else if (señal == 3) Seleccionar();
    }

    //1
    void Derecha()
    {
        if (estadoActual == EstadoPantalla.Fotos)
        {
            if (indice == 0) indice = 1;
            else if (indice == 1) indice = 10;
            else if (indice == 10) indice = 9;
            else if (indice == 9) indice = 0;
            Actualizar();
        }
        else if (estadoActual == EstadoPantalla.Explorador)
        {
            if (rutas.Count < 5 && rutas.Count != 0)
            {
                if (indice == 10 + rutas.Count) indice = 10;
                else if (indice == 10) indice = 9;
                else if (indice == 9) indice = 11;
                else indice = indice + 1;
            }
            else if (rutas.Count == 0)
            {
                if (indice == 10) indice = 9;
                else indice = indice + 1;
            }
            else
            {
                if (indice == 15) indice = 10;
                else if (indice == 10) indice = 9;
                else if (indice == 9) indice = 11;
                else indice = indice + 1;
            }
            Actualizar();
        }
        else if (estadoActual == EstadoPantalla.AddFoto)
        {
            if (indice == 10) indice = 9;
            else if (indice == 9) indice = 16;
            else if (indice == 16) indice = 10;
            Actualizar();
        }
        else if (estadoActual == EstadoPantalla.MenuExternas)
        {
            int inicio = paginaExternas * FOTOS_POR_PAGINA;
            int fotosEnPagina = Mathf.Min(FOTOS_POR_PAGINA, imagenesExternas.Count - inicio);
            int maxSelectorFoto = 1 + fotosEnPagina; // último selector de foto en esta página

            if (TotalPaginasExternas > 1)
            {
                // Con paginación: 0 y 1 son flechas, 2..maxSelectorFoto son fotos, 10 es volver
                if (indice == 10)
                    indice = 9;
                else if (indice == 9)
                    indice = 0;
                else if (indice == 1 && TotalPaginasExternas > 1)
                    indice = 2;
                else if (indice == maxSelectorFoto)
                    indice = 10;
                else if (indice == 0)
                    indice = 1;
                else
                    indice = indice + 1;
            }
            else
            {
                // Sin paginación (comportamiento original)
                if (indice == 10)
                    indice = 9;
                else if (indice == 9)
                    indice = 0;
                else if (indice == 1 + imagenesExternas.Count)
                    indice = 10;
                else
                    indice = (indice + 1) % selectores.Length;
            }
            Actualizar();
        }
        else
        {
            if (indice == 10) indice = 9;
            else if (indice == 9) indice = 0;
            else if (indice == 8) indice = 10;
            else indice = (indice + 1) % selectores.Length;
            Actualizar();
        }
    }

    //2
    void Izquierda()
    {
        if (estadoActual == EstadoPantalla.Fotos)
        {
            if (indice == 1) indice = 0;
            else if (indice == 0) indice = 9;
            else if (indice == 9) indice = 10;
            else if (indice == 10) indice = 1;
            Actualizar();
        }
        else if (estadoActual == EstadoPantalla.Explorador)
        {
            if (rutas.Count < 5 && rutas.Count != 0)
            {
                if (indice == 9) indice = 10;
                else if (indice == 10) indice = 10 + rutas.Count;
                else if (indice == 11) indice = 9;
                else indice = indice - 1;
            }
            else if (rutas.Count == 0)
            {
                if (indice == 9) indice = 10;
                else indice = indice - 1;
            }
            else
            {
                if (indice == 9) indice = 10;
                else if (indice == 10) indice = 15;
                else if (indice == 11) indice = 9;
                else indice = indice - 1;
            }
            Actualizar();
        }
        else if (estadoActual == EstadoPantalla.AddFoto)
        {
            if (indice == 9) indice = 10;
            else if (indice == 10) indice = 16;
            else if (indice == 16) indice = 9;
            Actualizar();
        }
        else if (estadoActual == EstadoPantalla.MenuExternas)
        {
            int inicio = paginaExternas * FOTOS_POR_PAGINA;
            int fotosEnPagina = Mathf.Min(FOTOS_POR_PAGINA, imagenesExternas.Count - inicio);
            int maxSelectorFoto = 1 + fotosEnPagina;

            if (TotalPaginasExternas > 1)
            {
                if (indice == 0)
                    indice = 9;
                else if (indice == 9)
                    indice = 10;
                else if (indice == 10)
                    indice = maxSelectorFoto;
                else if (indice == 2)
                    indice = 1;
                else if (indice == 1)
                    indice = 0;
                else
                {
                    indice--;
                    if (indice < 0) indice = selectores.Length - 1;
                }
            }
            else
            {
                if (indice == 0)
                    indice = 9;
                else if (indice == 9)
                    indice = 10;
                else if (indice == 10)
                    indice = 1 + imagenesExternas.Count;
                else
                {
                    indice--;
                    if (indice < 0) indice = selectores.Length - 1;
                }
            }
            Actualizar();
        }
        else
        {
            if (indice == 0)
                indice = 9;
            else if (indice == 9)
                indice = 10;
            else if (indice == 10)
                indice = 8;
            else
            {
                indice--;
                if (indice < 0) indice = selectores.Length - 1;
            }
            Actualizar();
        }
    }

    //3
    void Seleccionar()
    {
        DesactivarTodo();

        if (indice == 9)
        {
            SceneManager.LoadScene("Pantalla inicio");
        }
        else if (indice == 10)
        {
            if (estadoActual == EstadoPantalla.Menu1)
            {
                SceneManager.LoadScene("Pantalla inicio");
            }
            else if (estadoActual == EstadoPantalla.Menu2)
            {
                menu_1.SetActive(true);
                estadoActual = EstadoPantalla.Menu1;
            }
            else if (estadoActual == EstadoPantalla.MenuExternas || estadoActual == EstadoPantalla.Fotos)
            {
                bool esExterna = fotoActual >= fotos.Length;

                if (esExterna)
                {
                    if (viendoImagenExterna)
                    {
                        fotoExterna.SetActive(false);
                        MenuExternasPanel.SetActive(true);
                        estadoActual = EstadoPantalla.MenuExternas;
                        viendoImagenExterna = false;

                        int indexExterna = fotoActual - fotos.Length;
                        // Calcular en qué página cae esa foto y seleccionar su selector
                        paginaExternas = indexExterna / FOTOS_POR_PAGINA;
                        indice = (indexExterna % FOTOS_POR_PAGINA) + 2;

                        MostrarMenuExternas();
                    }
                    else
                    {
                        MenuExternasPanel.SetActive(false);
                        menu_1.SetActive(true);
                        estadoActual = EstadoPantalla.Menu1;
                        indice = 0;
                    }
                    Actualizar();
                }
                else
                {
                    // FOTOS NORMALES
                    if (fotoActual <= 5)
                    {
                        menu_1.SetActive(true);
                        estadoActual = EstadoPantalla.Menu1;
                        indice = fotoActual + 2;
                    }
                    else
                    {
                        menu_2.SetActive(true);
                        estadoActual = EstadoPantalla.Menu2;
                        indice = fotoActual - 4;
                    }
                }
                Actualizar();
            }
            else if (estadoActual == EstadoPantalla.Explorador)
            {
                ExploradorPanel.SetActive(false);
                menu_1.SetActive(true);
                estadoActual = EstadoPantalla.Menu1;
            }
            else if (estadoActual == EstadoPantalla.AddFoto)
            {
                FotoAnyadir.SetActive(false);
                ExploradorPanel.SetActive(true);
                estadoActual = EstadoPantalla.Explorador;
            }
        }
        // --- SELECTOR 0: página anterior en MenuExternas / navegación normal en otros estados ---
        else if (indice == 0)
        {
            if (estadoActual == EstadoPantalla.MenuExternas && TotalPaginasExternas > 1)
            {
                // Página anterior (con wrap)
                paginaExternas = (paginaExternas - 1 + TotalPaginasExternas) % TotalPaginasExternas;
                MenuExternasPanel.SetActive(true);
                MostrarMenuExternas();
                indice = 0;
                Actualizar();
                return;
            }

            if (imagenesExternas.Count > 0)
            {
                if (estadoActual == EstadoPantalla.Menu1)
                {
                    paginaExternas = 0;
                    MenuExternasPanel.SetActive(true);
                    estadoActual = EstadoPantalla.MenuExternas;
                    MostrarMenuExternas();
                }
                else if (estadoActual == EstadoPantalla.Menu2)
                {
                    menu_1.SetActive(true);
                    estadoActual = EstadoPantalla.Menu1;
                }
                else if (estadoActual == EstadoPantalla.MenuExternas)
                {
                    menu_2.SetActive(true);
                    estadoActual = EstadoPantalla.Menu2;
                }
            }
            else
            {
                if (estadoActual == EstadoPantalla.Menu1)
                {
                    menu_2.SetActive(true);
                    estadoActual = EstadoPantalla.Menu2;
                }
                else if (estadoActual == EstadoPantalla.Menu2)
                {
                    menu_1.SetActive(true);
                    estadoActual = EstadoPantalla.Menu1;
                }
            }

            if (estadoActual == EstadoPantalla.Fotos)
            {
                SiguientePantalla();
            }
        }
        // --- SELECTOR 1: página siguiente en MenuExternas / navegación normal en otros estados ---
        else if (indice == 1)
        {
            if (estadoActual == EstadoPantalla.MenuExternas && TotalPaginasExternas > 1)
            {
                // Página siguiente (con wrap)
                paginaExternas = (paginaExternas + 1) % TotalPaginasExternas;
                MenuExternasPanel.SetActive(true);
                MostrarMenuExternas();
                indice = 1;
                Actualizar();
                return;
            }

            if (imagenesExternas.Count > 0)
            {
                if (estadoActual == EstadoPantalla.Menu1)
                {
                    menu_2.SetActive(true);
                    estadoActual = EstadoPantalla.Menu2;
                }
                else if (estadoActual == EstadoPantalla.Menu2)
                {
                    paginaExternas = 0;
                    MenuExternasPanel.SetActive(true);
                    estadoActual = EstadoPantalla.MenuExternas;
                    MostrarMenuExternas();
                }
                else if (estadoActual == EstadoPantalla.MenuExternas)
                {
                    menu_1.SetActive(true);
                    estadoActual = EstadoPantalla.Menu1;
                }
            }
            else
            {
                if (estadoActual == EstadoPantalla.Menu1)
                {
                    menu_2.SetActive(true);
                    estadoActual = EstadoPantalla.Menu2;
                }
                else if (estadoActual == EstadoPantalla.Menu2)
                {
                    menu_1.SetActive(true);
                    estadoActual = EstadoPantalla.Menu1;
                }
            }

            if (estadoActual == EstadoPantalla.Fotos)
            {
                SiguientePantalla();
            }
        }
        else if (indice == 8)
        {
            ExploradorPanel.SetActive(true);
            estadoActual = EstadoPantalla.Explorador;

            if (rutas.Count == 0 && all_photos_added == false)
            {
                CargarCarpeta(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures));
            }
            else if (rutas.Count == 0 && all_photos_added == true)
            {
                indice = 10;
                Actualizar();
            }

            if (rutas.Count > 0)
            {
                indice = 11;
                Actualizar();
            }
        }
        else if (indice == 16)
        {
            if (imagenSeleccionada != null)
            {
                Sprite sprite = Sprite.Create(imagenSeleccionada, new Rect(0, 0, imagenSeleccionada.width, imagenSeleccionada.height), new Vector2(0.5f, 0.5f));
                imagenesExternas.Add(imagenSeleccionada);
                galeriaFotos.Add(sprite);
                imagenSeleccionada = null;
            }

            indice = 11;
            Actualizar();

            DesactivarTodo();

            ExploradorPanel.SetActive(true);
            estadoActual = EstadoPantalla.Explorador;

            ActualizarExplorador();

            if (rutas.Count == 0)
            {
                ExploradorPanel.SetActive(false);
                FotoAnyadir.SetActive(false);

                menu_1.SetActive(true);
                estadoActual = EstadoPantalla.Menu1;
                fotoActual = 0;
                indice = 8;
                Actualizar();
            }
        }
        else
        {
            SiguientePantalla();
        }
    }

    void SiguientePantalla()
    {
        DesactivarTodo();

        if (estadoActual == EstadoPantalla.Menu1)
        {
            int index = indice - 2;
            fotos[index].SetActive(true);
            fotoActual = index;
            estadoActual = EstadoPantalla.Fotos;
            indice = 0;
            Actualizar();
        }
        else if (estadoActual == EstadoPantalla.Menu2)
        {
            int index = indice + 4;
            fotos[index].SetActive(true);
            fotoActual = index;
            estadoActual = EstadoPantalla.Fotos;
            indice = 0;
            Actualizar();
        }
        else if (estadoActual == EstadoPantalla.Fotos)
        {
            int n;
            if (indice == 0)
            {
                if (fotoActual > 12 || fotoActual == 0)
                {
                    if (galeriaFotos.Count == 12)
                    {
                        n = (fotoActual - 1 + galeriaFotos.Count) % galeriaFotos.Count;
                        fotos[n].SetActive(true);
                        fotoActual = n;
                    }
                    else
                    {
                        n = (fotoActual - 1 + galeriaFotos.Count) % galeriaFotos.Count;
                        MostrarSpriteEnFotos(galeriaFotos[n]);
                        fotoActual = n;
                    }
                }
                else if (fotoActual <= 12)
                {
                    n = (fotoActual - 1 + galeriaFotos.Count) % galeriaFotos.Count;
                    fotos[n].SetActive(true);
                    fotoActual = n;
                }
            }
            else if (indice == 1)
            {
                if (fotoActual < 11 || fotoActual == galeriaFotos.Count - 1)
                {
                    n = (fotoActual + 1) % galeriaFotos.Count;
                    fotos[n].SetActive(true);
                    fotoActual = n;
                }
                else
                {
                    n = (fotoActual + 1) % galeriaFotos.Count;
                    MostrarSpriteEnFotos(galeriaFotos[n]);
                    fotoActual = n;
                }
            }
        }
        else if (estadoActual == EstadoPantalla.Explorador)
        {
            indiceExplorador = Mathf.Clamp(indice - 11, 0, rutas.Count - 1);
            string seleccion = rutas[indiceExplorador];

            if (Directory.Exists(seleccion))
            {
                CargarCarpeta(seleccion);
            }
            else
            {
                byte[] data = File.ReadAllBytes(seleccion);
                Texture2D tex = new Texture2D(70, 70);
                tex.LoadImage(data);

                imagenSeleccionada = tex;
                MostrarImagen(tex);

                ExploradorPanel.SetActive(false);
                imagenUI.gameObject.SetActive(true);
                FotoAnyadir.SetActive(true);
                estadoActual = EstadoPantalla.AddFoto;

                indice = 16;
                Actualizar();
            }
        }
        else if (estadoActual == EstadoPantalla.MenuExternas)
        {
            // El selector 2 corresponde a la foto 0 de la página actual, etc.
            int localIndex = indice - 2;
            int globalIndex = paginaExternas * FOTOS_POR_PAGINA + localIndex;

            if (globalIndex >= 0 && globalIndex < imagenesExternas.Count)
            {
                MostrarFotoExterna(globalIndex);
            }
        }
    }

    void MostrarSpriteEnFotos(Sprite sprite)
    {
        DesactivarTodo();
        fotoExterna.SetActive(true);
        estadoActual = EstadoPantalla.Fotos;
        imagenUI.gameObject.SetActive(true);
        imagenUI.sprite = sprite;
        imagenUI.rectTransform.sizeDelta = new Vector2(150, 150);
        estadoActual = EstadoPantalla.Fotos;
    }

    void MostrarFotoExterna(int index)
    {
        viendoImagenExterna = true;
        DesactivarTodo();

        Texture2D tex = imagenesExternas[index];
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        fotoExterna.SetActive(true);
        imagenUI.gameObject.SetActive(true);
        imagenUI.sprite = sprite;

        estadoActual = EstadoPantalla.Fotos;
        fotoActual = fotos.Length + index;

        indice = 0;
        Actualizar();
    }

    void MostrarImagen(Texture2D tex)
    {
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        imagenUI.sprite = sprite;
    }

    void DesactivarTodo()
    {
        menu_1.SetActive(false);
        menu_2.SetActive(false);

        for (int i = 0; i < fotos.Length; i++)
            fotos[i].SetActive(false);

        if (estadoActual != EstadoPantalla.Explorador)
            ExploradorPanel.SetActive(false);

        FotoAnyadir.SetActive(false);
        imagenUI.gameObject.SetActive(false);
        MenuExternasPanel.SetActive(false);
        fotoExterna.SetActive(false);
    }

    void Actualizar()
    {
        for (int i = 0; i < selectores.Length; i++)
        {
            selectores[i].color = (i == indice) ? UnityEngine.Color.white : new UnityEngine.Color(1, 1, 1, 0);
        }
    }

    void CargarCarpeta(string ruta)
    {
        rutas.Clear();

        string[] files = Directory.GetFiles(ruta, "*.png");
        rutas.AddRange(files);

        files = Directory.GetFiles(ruta, "*.jpg");
        rutas.AddRange(files);

        files = Directory.GetFiles(ruta, "*.jpeg");
        rutas.AddRange(files);

        rutaActual = ruta;
        GenerarUI();
    }

    void GenerarUI()
    {
        int cantidad = Mathf.Min(rutas.Count, 5);
        for (int i = 0; i < cantidad; i++)
        {
            GameObject btn = Instantiate(botonPrefab, contenedorUI);
            TMP_Text txt = btn.transform.GetComponentInChildren<TMP_Text>(true);
            if (txt != null)
                txt.text = Path.GetFileName(rutas[i]);
        }
        contenedorUI.GetChild(0).gameObject.SetActive(false);
    }

    void ActualizarExplorador()
    {
        rutas.RemoveAt(indiceExplorador);

        for (int i = 0; i < contenedorUI.childCount; i++)
        {
            if (i == 0)
                contenedorUI.GetChild(0).gameObject.SetActive(true);
            else
                Destroy(contenedorUI.GetChild(i).gameObject);
        }

        GenerarUI();

        indiceExplorador = Mathf.Clamp(indiceExplorador, 0, rutas.Count - 1);

        if (rutas.Count == 0)
            all_photos_added = true;
    }

    void MostrarMenuExternas()
    {
        viendoImagenExterna = false;

        // Limpiar panel
        foreach (Transform child in MenuExternasPanel.transform)
            Destroy(child.gameObject);

        // Calcular rango de fotos para esta página
        int inicio = paginaExternas * FOTOS_POR_PAGINA;
        int fin = Mathf.Min(inicio + FOTOS_POR_PAGINA, imagenesExternas.Count);

        for (int i = inicio; i < fin; i++)
        {
            int localIndex = i - inicio;

            GameObject obj = new GameObject("Externa_" + i, typeof(RectTransform));
            obj.transform.SetParent(MenuExternasPanel.transform, false);

            Image img = obj.AddComponent<Image>();
            Texture2D tex = imagenesExternas[i];
            img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(70, 70);

            // Posición en 2 columnas
            float x = (localIndex % 2 == 0) ? -40f : 40f;
            float y = -60f - (localIndex / 2) * 80f;
            rt.anchoredPosition = new Vector2(x, y);
        }

        // Si hay más de una página, añadir indicador de página (texto)
        if (TotalPaginasExternas > 1)
        {
            GameObject indicador = new GameObject("IndicadorPagina", typeof(RectTransform));
            indicador.transform.SetParent(MenuExternasPanel.transform, false);

            TMP_Text txt = indicador.AddComponent<TextMeshProUGUI>();
            txt.text = $"{paginaExternas + 1} / {TotalPaginasExternas}";
            txt.fontSize = 22;
            txt.alignment = TextAlignmentOptions.Center;

            //indicador de paginación
            RectTransform rt = indicador.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(150, 50);
            rt.anchoredPosition = new Vector2(0, 60f);
        }
    }

    void OnDestroy()
    {
        client?.Close();
    }
}
