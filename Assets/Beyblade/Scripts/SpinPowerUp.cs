using System.Runtime.InteropServices;
using UnityEngine;
using System;

public class SpinPowerUp : MonoBehaviour
{

    [SerializeField] private int scorePoint;
    [SerializeField] private float speedBoost = .15f;


    public static event EventHandler OnUpdateScore;

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.TryGetComponent(out PlayerController playerController))
        {
            
            playerController.UpdgradeSpeed(speedBoost);
            AudioManager.Instance.PlaySpeedBoostAudio();
            Debug.Log("ConsumedSpeedBoost");
            playerController.AddToScore(scorePoint); 

            OnUpdateScore?.Invoke(this, EventArgs.Empty);
            
            Destroy(this.gameObject);       
        }
    }
}
