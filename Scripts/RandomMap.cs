using Godot;
using System;

public partial class RandomMap : Node2D
{
	public uint Seed = 0;

	public uint PlatformCount = 70;

	public float MaxPlatformScale = 10;

	public Vector2 MapSize = new Vector2(5000, 1000);

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SetSeed(uint _Seed)
	{
		Seed = _Seed;
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void GenerateMap()
	{
		PackedScene PlatformScene = GD.Load<PackedScene>("res://Scenes/Platforms/NormalPlatform.tscn");

		GD.Seed(Seed);

		for (uint i = 0; i < PlatformCount; i++)
		{
			Node2D Platform = PlatformScene.Instantiate<Node2D>();

			Platform.Position = new Vector2(
				(float)GD.RandRange(-MapSize.X, MapSize.X),
				(float)GD.RandRange(-MapSize.Y, MapSize.Y)
			);

			Platform.Scale = new Vector2(
				(float)GD.RandRange(0.1, MaxPlatformScale),
				(float)GD.RandRange(0.1, MaxPlatformScale)
			);

			Platform.RotationDegrees = GD.RandRange(0, 360);

			AddChild(Platform, true);
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();

		if (Multiplayer.IsServer())
		{
			GD.Randomize();
			Seed = GD.Randi();
			Rpc(nameof(SetSeed), Seed);
			Rpc(nameof(GenerateMap));
		}
	}
}
