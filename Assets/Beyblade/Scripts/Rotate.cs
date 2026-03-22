using Unity.VisualScripting;
using UnityEngine;

public class Rotate : MonoBehaviour
{

    [SerializeField] private float rotateSpeed = 10f;
    private void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}
