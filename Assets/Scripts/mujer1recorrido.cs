using UnityEngine;

public class SeguirCanoa : MonoBehaviour
{
    [Tooltip("Arrastra aquí el GameObject de la canoa.")]
    public Transform canoa;

    [Tooltip("Posición local dentro de la canoa (ajusta a ojo en el Inspector).")]
    public Vector3 offsetPosicion = new Vector3(0f, 0.5f, 0f);

    [Tooltip("Rotación local respecto a la canoa, en grados.")]
    public Vector3 offsetRotacion = new Vector3(0f, 0f, 0f);

    void Start()
    {
        // Emparenta al personaje con la canoa en tiempo de ejecución.
        transform.SetParent(canoa);
        transform.localPosition = offsetPosicion;
        transform.localRotation = Quaternion.Euler(offsetRotacion);
    }
}
