using UnityEngine;

/// <summary>
/// Anima un objeto (la canoa) para que parezca flotar en el agua y seguir
/// una trayectoria sobre el lago. Combina:
///   1. Flotación vertical (bobbing) — sube y baja con un seno.
///   2. Balanceo (rocking) — pequeñas inclinaciones de pitch y roll.
///   3. Movimiento horizontal — elegible entre estático, circular, o Lissajous.
///
/// El modo Lissajous permite trayectorias mucho más interesantes que un círculo:
///   - 1:2 → figura de 8 (lemniscata clásica)
///   - 2:3 → patrón de 5 lóbulos
///   - 3:4 → patrón complejo, varios cruces
///   - 1:3 → tres pétalos
/// Cambiando el desfase también se rotan/deforman los patrones.
/// </summary>
public class CanoaFlotante : MonoBehaviour
{
    public enum TipoTrayectoria { Estatica, Circulo, Lissajous }

    [Header("Tipo de trayectoria")]
    public TipoTrayectoria trayectoria = TipoTrayectoria.Lissajous;

    [Header("Flotación vertical (bobbing)")]
    [Tooltip("Qué tanto sube y baja, en metros.")]
    public float amplitudY = 0.15f;

    [Tooltip("Velocidad de la oscilación vertical.")]
    public float velocidadY = 1.2f;

    [Header("Balanceo (rocking)")]
    [Tooltip("Inclinación máxima al frente/atrás (pitch), en grados.")]
    public float amplitudPitch = 2.5f;

    [Tooltip("Inclinación máxima a los lados (roll), en grados.")]
    public float amplitudRoll = 1.5f;

    [Tooltip("Velocidad del balanceo.")]
    public float velocidadBalanceo = 0.8f;

    [Header("Trayectoria — Círculo (solo si trayectoria = Circulo)")]
    [Tooltip("Radio del círculo en metros.")]
    public float radioCirculo = 8f;

    [Tooltip("Velocidad angular en grados por segundo.")]
    public float velocidadAngular = 10f;

    [Header("Trayectoria — Lissajous (solo si trayectoria = Lissajous)")]
    [Tooltip("Tamaño del patrón en X (metros).")]
    public float amplitudX = 12f;

    [Tooltip("Tamaño del patrón en Z (metros).")]
    public float amplitudZ = 8f;

    [Tooltip("Frecuencia de oscilación en X. Junto con frecuenciaZ define la forma. " +
             "Prueba 1, 2, 3, 4. Razones recomendadas: 1:2 = figura de 8, 2:3 = 5 lóbulos.")]
    [Range(1, 6)]
    public int frecuenciaX = 1;

    [Tooltip("Frecuencia de oscilación en Z. Mira el tooltip de frecuenciaX.")]
    [Range(1, 6)]
    public int frecuenciaZ = 2;

    [Tooltip("Desfase entre las dos ondas, en grados. Cambia la 'inclinación' del patrón. " +
             "Para la figura de 8 clásica con 1:2, usa 90.")]
    [Range(0f, 360f)]
    public float desfaseGrados = 90f;

    [Tooltip("Velocidad global del recorrido. Más bajo = más calmado.")]
    public float velocidadLissajous = 0.3f;

    [Header("Orientación")]
    [Tooltip("Si la canoa debe rotar para apuntar hacia donde se mueve.")]
    public bool mirarHaciaMovimiento = true;

    [Tooltip("Ajuste manual de yaw. Si la canoa apunta perpendicular al movimiento, prueba 90 o -90.")]
    public float offsetYawGrados = 90f;

    // Estado inicial
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;

    // Para el suavizado de la rotación cuando hay cambios rápidos
    private float yawActualSuavizado = 0f;
    private bool yawInicializado = false;

    void Start()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
    }

    void Update()
    {
        float t = Time.time;

        // ----- 1. Flotación vertical -----
        float dy = Mathf.Sin(t * velocidadY) * amplitudY;

        // ----- 2. Balanceo (dos senos desfasados para que no se sienta mecánico) -----
        float pitch = Mathf.Sin(t * velocidadBalanceo) * amplitudPitch;
        float roll  = Mathf.Sin(t * velocidadBalanceo * 1.3f + 1f) * amplitudRoll;

        // ----- 3. Movimiento horizontal según el tipo elegido -----
        Vector3 dxz = Vector3.zero;
        Vector3 velocidad = Vector3.zero; // tangente al recorrido, para calcular yaw

        switch (trayectoria)
        {
            case TipoTrayectoria.Estatica:
                // No hay movimiento horizontal, solo flota en su lugar.
                break;

            case TipoTrayectoria.Circulo:
                {
                    float ang = t * velocidadAngular * Mathf.Deg2Rad;
                    dxz = new Vector3(Mathf.Cos(ang) * radioCirculo, 0f,
                                      Mathf.Sin(ang) * radioCirculo);
                    // Tangente al círculo: derivada de (R·cos, R·sin) es (-R·sin, R·cos)
                    velocidad = new Vector3(-Mathf.Sin(ang), 0f, Mathf.Cos(ang));
                    break;
                }

            case TipoTrayectoria.Lissajous:
                {
                    float ts = t * velocidadLissajous;
                    float desfase = desfaseGrados * Mathf.Deg2Rad;

                    // Posición: sumas de senos con frecuencias distintas en cada eje
                    dxz = new Vector3(
                        Mathf.Sin(ts * frecuenciaX + desfase) * amplitudX,
                        0f,
                        Mathf.Sin(ts * frecuenciaZ) * amplitudZ
                    );

                    // Tangente: derivada da cosenos con factor de la frecuencia
                    velocidad = new Vector3(
                        Mathf.Cos(ts * frecuenciaX + desfase) * frecuenciaX * amplitudX,
                        0f,
                        Mathf.Cos(ts * frecuenciaZ) * frecuenciaZ * amplitudZ
                    );
                    break;
                }
        }

        // ----- Yaw (si la canoa debe orientarse hacia donde va) -----
        float yawObjetivo = 0f;
        if (mirarHaciaMovimiento && velocidad.sqrMagnitude > 0.0001f)
        {
            yawObjetivo = Mathf.Atan2(velocidad.x, velocidad.z) * Mathf.Rad2Deg + offsetYawGrados;
        }

        // Suavizado: evita giros bruscos cuando la velocidad cambia rápido de dirección
        // (importante en Lissajous, donde la canoa puede invertir sentido en los extremos).
        if (!yawInicializado)
        {
            yawActualSuavizado = yawObjetivo;
            yawInicializado = true;
        }
        else
        {
            // LerpAngle maneja correctamente el cruce 0/360.
            yawActualSuavizado = Mathf.LerpAngle(yawActualSuavizado, yawObjetivo,
                                                  Time.deltaTime * 3f);
        }

        // ----- Aplicar todo -----
        transform.position = posicionInicial + dxz + Vector3.up * dy;
        transform.rotation = Quaternion.Euler(0f, yawActualSuavizado, 0f)
                           * Quaternion.Euler(pitch, 0f, roll)
                           * rotacionInicial;
    }
}
