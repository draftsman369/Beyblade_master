using System.Collections;
using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    public float duration = 5f;

    private Collider powerUpCollider;
    [SerializeField] private GameObject visual;
    private bool collected;

    private void Awake()
    {
        powerUpCollider = this.GetComponent<Collider>();
    }

    public void Activate(PlayerController player)
    {
        StartCoroutine(ApplyRoutine(player));
    }

    private IEnumerator ApplyRoutine(PlayerController player)
    {
        Apply(player);

        yield return new WaitForSeconds(duration);

        Remove(player);
        Destroy(gameObject);   
    }

    public abstract void Apply(PlayerController player);
    public abstract void Remove(PlayerController player);

    private void OnTriggerEnter(Collider other)
    {
        if(collected) return;

        if(other.TryGetComponent(out PlayerController player))
        {
            collected = true;
            GetComponent<Collider>().enabled = false;
            visual.SetActive(false);
            Activate(player);

        }
    }

}
