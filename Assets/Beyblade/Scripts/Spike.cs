using UnityEngine;

public class Spike : MonoBehaviour
{

    public GameObject explosionVFX;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            GameObject explosion = Instantiate(explosionVFX, other.transform.position, explosionVFX.transform.rotation);
            AudioManager.Instance.PlayExplosion();
            Destroy(explosion, 1.5f);
            Destroy(other.gameObject);
        }
    }
}
