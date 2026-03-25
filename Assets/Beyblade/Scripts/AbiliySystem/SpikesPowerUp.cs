using UnityEngine;

public class SpikesPowerUp : PowerUp
{
    public override void Apply(PlayerController player)
    {
        player.spikes.SetActive(true);
    }

    public override void Remove(PlayerController player)
    {
        player.spikes.SetActive(false);
    }

}
