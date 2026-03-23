using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LevelTarget : MonoBehaviour
{
    [SerializeField] UIDocument gameUI;
    VisualElement visual;
    [SerializeField] private PlayerController player;

    [SerializeField] private float fadeDuration;
    [SerializeField] private float timer;
    private bool isLevelComplete;

    private void Start()
    {
        visual = gameUI.rootVisualElement.Q<VisualElement>("GameWon");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Game Ended");
            isLevelComplete = true;
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
        yield return new WaitForSeconds(fadeDuration + 0.5f);
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        if(isLevelComplete)
        {
            timer += Time.deltaTime;
            visual.style.opacity = timer/fadeDuration;            
        }

    }
}
