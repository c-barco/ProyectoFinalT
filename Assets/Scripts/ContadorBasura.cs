using UnityEngine;
using TMPro; // Requerido para controlar textos de TextMeshPro

public class ContadorBasura : MonoBehaviour
{
    [Header("Configuración de Interfaz")]
    public TextMeshProUGUI textoContador; // El texto de la pantalla

    [Header("Configuración de la Misión")]
    public int totalBasuraEnMapa = 10; // Cambia este número desde el Inspector si pones más

    private int basuraRecolectada = 0;

    void Start()
    {
        // Inicializamos el texto al arrancar el juego
        ActualizarInterfaz();
    }

    // Se activa cuando entramos en el colisionador tipo "Trigger" de la basura
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Basura"))
        {
            // 1. Sumamos uno al contador numérico
            basuraRecolectada++;

            // 2. Actualizamos el texto en la pantalla o mostramos el mensaje de éxito
            ActualizarInterfaz();

            // 3. Eliminamos la pila de basura de la escena
            Destroy(other.gameObject);
        }
    }

    // Función que maneja lo que se despliega en pantalla
    void ActualizarInterfaz()
    {
        if (textoContador != null)
        {
            // Condición: Si ya recogimos todo lo que configuramos como total
            if (basuraRecolectada >= totalBasuraEnMapa)
            {
                textoContador.text = "¡Misión cumplida!";
                textoContador.color = Color.green; // Opcional: cambia el color a verde para resaltar el éxito
            }
            else
            {
                // Si aún faltan, muestra el progreso normal
                textoContador.text = "Basura recogida: " + basuraRecolectada + " / " + totalBasuraEnMapa;
            }
        }
    }
}
