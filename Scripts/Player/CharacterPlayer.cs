using BulletHellGD.Scripts;
using Godot;
using System;

public partial class CharacterPlayer : CharacterBody2D
{
	[Export]
	public float Gravity = -2f;

	[Export]
	public float Speed = 400f;

	[Export]
	public float MaxMovementSpeed = 300f;

	[Export]
	public float JumpPower = 300f;

	[Export]
	public float Friction = 0.99f;

	[Export]
	public float AirFriction = 0.99f;

    public override void _EnterTree()
    {
        base._EnterTree();
    }

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!IsMultiplayerAuthority()) return;

		if (Position.Y > 2000) Position = new Vector2(Position.X, -Position.Y);

		if (!IsOnFloor()) 
		{ 
			Velocity -= new Vector2(0, Gravity);

			Vector2 TempVel3 = Velocity;
			TempVel3.X *= AirFriction;
			Velocity = TempVel3;
		}
		else
		{
			Vector2 TempVel = Velocity;
			TempVel.Y = 0;
			Velocity = TempVel;

			Vector2 TempVel2 = Velocity;
			TempVel2.X *= Friction;
			Velocity = TempVel2;
		}

		if (Input.IsActionPressed("Up") && IsOnFloor()) Velocity -= new Vector2(0, JumpPower);

		if (Input.IsActionPressed("Right") && Velocity.X <  MaxMovementSpeed) Velocity += new Vector2( Speed * (float)delta, 0);
		if (Input.IsActionPressed("Left")  && Velocity.X > -MaxMovementSpeed) Velocity += new Vector2(-Speed * (float)delta, 0);

		MoveAndSlide();
    }


	public void BulletCall(int Pid, Vector2 GlobalPosition)
	{
		RpcId(Pid, nameof(BulletHit), GlobalPosition);
	}

	public void ResetPlayerCall(Vector2 MapSize)
	{
        Rpc(nameof(ResetPlayer), MapSize);
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void BulletHit(Vector2 GlobalBulletPosition)
	{
		if (!IsMultiplayerAuthority()) return;

		Vector2 Knockback = -GlobalPosition.DirectionTo(GlobalBulletPosition);
		Velocity += Knockback * 100f;
    }

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void ResetPlayer(Vector2 MapSize)
	{
        if (!IsMultiplayerAuthority()) return;

		Velocity = Vector2.Zero;

		Position = new Vector2(
			(float)GD.RandRange(-MapSize.X, MapSize.X),
			-MapSize.Y - 200f
		);
    }
}
