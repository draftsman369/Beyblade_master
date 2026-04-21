using UnityEngine;
using System;

public class SpinPowerUp : MonoBehaviour
{
    [SerializeField] private int scorePoint = 1;
    [SerializeField] private float speedBoost = 0.15f;
    [SerializeField] private int ultimateChargeAmount = 1;

    public static event EventHandler OnUpdateScore;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out PlayerController playerController))
        {
            playerController.UpgradeSpeed(speedBoost);
            playerController.AddToScore(scorePoint);
            playerController.AddUltimateCharge(ultimateChargeAmount);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySpeedBoostAudio();
            }

            Debug.Log("ConsumedSpeedBoost");

            OnUpdateScore?.Invoke(this, EventArgs.Empty);

            Destroy(gameObject);
        }
    }
}