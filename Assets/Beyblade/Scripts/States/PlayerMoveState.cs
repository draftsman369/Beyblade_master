public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController controller) : base(controller) { }

    public override void Update()
    {
        controller.ReadInput();

        if (controller.TryConsumeUltimateInput())
        {
            if (controller.CanStartUltimate())
            {
                controller.BeginUltimateEffects();
                controller.SwitchState(new PlayerUltimateState(controller));
            }
            else
            {
                UnityEngine.Debug.Log("Ultimate not ready yet.");
            }
        }
    }

    public override void FixedUpdate()
    {
        controller.HandleJump();
        controller.Move();
        controller.CheckGameOver();
    }
}