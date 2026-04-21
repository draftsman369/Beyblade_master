public class PlayerUltimateState : PlayerState
{
    public PlayerUltimateState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        controller.ConsumeUltimateCharge();
        controller.StartUltimateStateRoutine();
    }
}