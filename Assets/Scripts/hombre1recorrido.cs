using UnityEngine;

public class RecorridoPersonaje : MonoBehaviour
{
    [Header("Configuración del Recorrido")]
    public Transform[] puntos; // Arreglo para guardar los puntos (Waypoints)
    public float velocidad = 2.0f; // Velocidad de movimiento
    public float velocidadRotacion = 5.0f; // Qué tan rápido gira

    private int indiceActual = 0;

    void Update()
    {
        // Si no hay puntos asignados, no hacemos nada
        if (puntos.Length == 0) return;

        Transform destino = puntos[indiceActual];

        // 1. Mover al personaje hacia el punto de destino
        transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidad * Time.deltaTime);

        // 2. Rotar al personaje para que mire hacia donde camina
        Vector3 direccion = destino.position - transform.position;
        direccion.y = 0; // Ignoramos la altura para que no se incline hacia arriba o abajo

        if (direccion != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
        }

        // 3. Comprobar si ya llegamos al punto para pasar al siguiente
        if (Vector3.Distance(transform.position, destino.position) < 0.1f)
        {
            // Pasa al siguiente punto. El módulo (%) hace que regrese al inicio al terminar.
            indiceActual = (indiceActual + 1) % puntos.Length; 
        }
    }
}