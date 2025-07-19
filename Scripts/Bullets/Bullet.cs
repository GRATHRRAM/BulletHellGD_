using BulletHellGD.Scripts;
using Godot;
using System;
using System.Reflection.Metadata;

public partial class Bullet : Node2D
{
	public int CreatorID = 0;

	public Vector2 Velocity = Vector2.Zero;
	public Vector2 SpawnGlobalPosition = Vector2.Zero;

	public override void _Ready()
	{
		base._Ready();
		GlobalPosition = SpawnGlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!IsMultiplayerAuthority()) return;
		Position += Velocity * (float) delta;
	}

	public void _on_timer_timeout()
	{
		if (IsMultiplayerAuthority()) QueueFree();
	}

	public void _on_area_2d_body_entered(Node2D Colider)
	{
		if (!IsMultiplayerAuthority()) return;

		if (Colider.HasMeta("Player") && Int32.Parse(Colider.GetParent().Name) != CreatorID)
		{
			Colider.Call("BulletCall", Int32.Parse(Colider.GetParent().Name), GlobalPosition);
			Rpc(nameof(SpawnParticle));
			QueueFree();
		}

		if (Colider.HasMeta("Platform")) { QueueFree(); Rpc(nameof(SpawnParticle)); }
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SpawnParticle()
	{
		PackedScene HitParticleScene = GD.Load<PackedScene>("res://Scenes/Particles/BulletHit.tscn");
		GpuParticles2D HitParticle = HitParticleScene.Instantiate<GpuParticles2D>();
		HitParticle.GlobalPosition = GlobalPosition;
		HitParticle.Restart();
		HitParticle.Emitting = true;
		GetParent().AddChild(HitParticle);
	}
}
