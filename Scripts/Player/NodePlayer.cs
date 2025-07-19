using BulletHellGD.Scripts;
using Godot;
using System;
using System.Linq;

public partial class NodePlayer : Node2D
{
	[Export]
	public float BulletSpeed = 500f;

	[Export]
	private PackedScene BulletScene = null;

	private Node2D BulletHolder = null;

	public override void _EnterTree()
	{
		base._EnterTree();

		MeshInstance2D Mesh = GetNode<MeshInstance2D>("CharacterBody2D/MeshInstance2D");
		Mesh.Modulate = GlobalFunctions.RandomColor();

		SetMultiplayerAuthority(Int32.Parse(Name));

		BulletHolder = GetParent().GetParent().GetNode<Node2D>("BulletHolder");
		BulletHolder.GetNode<MultiplayerSpawner>("MultiplayerSpawner").SpawnFunction = Callable.From((Godot.Collections.Dictionary data) => BulletSpawn(data));
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (Input.IsActionJustPressed("Shoot"))
		{
			RpcId(1, nameof(RequestShoot), GetGlobalMousePosition());
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RequestShoot(Vector2 mousePosition)
	{
		if (!Multiplayer.IsServer()) return;

		int SenderId = Multiplayer.GetRemoteSenderId();
		var charBody = GetParent().GetNode(SenderId.ToString()).GetNode<CharacterBody2D>("CharacterBody2D");

		var SpawnData = new Godot.Collections.Dictionary
		{
			{ "Pid", 1 },
			{ "CreatorID", SenderId},
			{ "Name", $"bullet_{Multiplayer.GetUniqueId()}_{GD.Randi()}"},
			{ "SpawnPosition", charBody.GlobalPosition },
			{ "Velocity", charBody.GlobalPosition.DirectionTo(mousePosition) * BulletSpeed },
		};

		BulletHolder.GetNode<MultiplayerSpawner>("MultiplayerSpawner").Spawn(SpawnData);
	}

	private Node BulletSpawn(Godot.Collections.Dictionary Data)
	{
		Node _Bullet = (Node) BulletScene.Instantiate();
		_Bullet.SetMultiplayerAuthority((int) Data["Pid"]);
		_Bullet.Set("SpawnGlobalPosition", (Vector2)Data["SpawnPosition"]);
		_Bullet.Set("Velocity", (Vector2)Data["Velocity"]);
		_Bullet.Set("CreatorID", (int)Data["CreatorID"]);
		_Bullet.Name = (string) Data["Name"];

		return _Bullet;
	}
}
