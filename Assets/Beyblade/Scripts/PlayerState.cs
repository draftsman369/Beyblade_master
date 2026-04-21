public abstract class PlayerState
{
	protected PlayerController controller;

	protected PlayerState(PlayerController controller)
	{
		this.controller = controller;
	}

	public virtual void Enter(){}
	public virtual void Update(){}
	public virtual void Exit(){}
	public virtual void FixedUpdate(){}
}
