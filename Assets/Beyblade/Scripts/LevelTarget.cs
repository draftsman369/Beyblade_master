using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LevelTarget : MonoBehaviour
{
    [SerializeField] UIDocument gameUI;
    [SerializeField] private PlayerController player;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Game Ended");
            EndGame();
        }
    }

    private void EndGame()
    {
        Time.timeScale = .2f;
        Debug.LogWarning("Game Ended");
        StartCoroutine(EndCoroutine());
    }

    IEnumerator EndCoroutine()
    {
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene(0);
    }
}
