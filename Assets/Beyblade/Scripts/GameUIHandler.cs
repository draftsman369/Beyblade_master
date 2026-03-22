using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private UIDocument GameUI;
    private Label scoreLabel;

    private void Start()
    {
        scoreLabel = GameUI.rootVisualElement.Q<Label>("scoreLabel");    
        SpinPowerUp.OnUpdateScore += UpdateScore;


    }

    private void OnEnable()
    {
        SpinPowerUp.OnUpdateScore += UpdateScore;
    }

    private void OnDisable()
    {
        SpinPowerUp.OnUpdateScore -= UpdateScore;
    }

    private void UpdateScore(object sender, System.EventArgs e)
    {
        scoreLabel.text = playerController.Score.ToString();
        Debug.Log("Score Updated");
    }

}
