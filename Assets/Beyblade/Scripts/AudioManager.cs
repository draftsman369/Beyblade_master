using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource audioSource;

    [SerializeField] private AudioClip speedBoost;
    [SerializeField] private AudioClip explosion;

    private void Awake()
    {

        if(Instance == null)
        {
          Instance = this;   
        }
        audioSource = this.GetComponent<AudioSource>();
    }

    public void PlaySpeedBoostAudio()
    {
        audioSource.PlayOneShot(speedBoost, 1f);
    }

    public void PlayExplosion()
    {
        audioSource.PlayOneShot(explosion, 2f);
    }
}
