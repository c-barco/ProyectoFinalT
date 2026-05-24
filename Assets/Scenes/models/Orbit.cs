using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target;
    public float speed = 30f;
    public bool invertirDireccion = true;

    void Update()
    {
        if (target != null)
        {
            Vector3 posicionAnterior = transform.position;
            
            transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
            
            Vector3 direccionMovimiento = transform.position - posicionAnterior;

            if (direccionMovimiento != Vector3.zero)
            {
                if (invertirDireccion)
                {
                    transform.forward = -direccionMovimiento.normalized;
                }
                else
                {
                    transform.forward = direccionMovimiento.normalized;
                }
            }
        }
    }
}