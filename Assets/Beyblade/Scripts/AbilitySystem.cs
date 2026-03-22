using System;
using System.Collections;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{

    [SerializeField] float spikeAbilityDuration = 3f;
    [SerializeField] public GameObject Spike;
    
    private bool spikeAbilityInUse;

    private void Start()
    {
        InputManager.Instance.OnSpikeAbility += UseSpikeAbility;
    }

    private IEnumerator SpikeAbilityCoroutine()
    {
        spikeAbilityInUse = true;
        Spike.SetActive(true);
        yield return new WaitForSeconds(spikeAbilityDuration);
        InputManager.Instance.ConsumeSpikeAbility();
        Spike.SetActive(false);
        Debug.LogWarning("Spike Ability Reset");
        spikeAbilityInUse = false;
    }

    private void UseSpikeAbility(object sender, EventArgs e)
    {
        if(spikeAbilityInUse)
            return;
        StartCoroutine(SpikeAbilityCoroutine());
    }
}
