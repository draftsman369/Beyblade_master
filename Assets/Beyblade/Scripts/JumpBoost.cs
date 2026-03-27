using UnityEngine;

public class JumpBoost : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 currentVelocity = playerRb.linearVelocity;
                currentVelocity.y =  10f;
                playerRb.linearVelocity = new Vector3(currentVelocity.x, 10f, currentVelocity.z); 
                //playerRb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
            }
        }
    }
}
