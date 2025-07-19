using Godot;
using System;
using System.Security.Cryptography;
using static Godot.TextServer;

public partial class Main : Node2D
{
	public override void _Ready()
	{
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;
	}
	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (!IsMultiplayerAuthority()) return;

		if (Input.IsActionJustPressed("ToggleGui"))
			GetNode<Control>("CanvasLayer/ServerGui").Visible = !GetNode<Control>("CanvasLayer/ServerGui").Visible;
	}

	public void _on_host_pressed()
	{
		string Port = GetNode<TextEdit>("CanvasLayer/StartGui/Port").Text;

		if (!Port.IsValidInt())
		{
			GD.Print("Main.cs Invalid Port!!!");
			return;
		}

		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		peer.CreateServer(Port.ToInt());
		Multiplayer.MultiplayerPeer = peer;

		GetNode<Node2D>("PlayerHolder").Call("SpawnPlayer", Multiplayer.GetUniqueId());

		GetNode<Control>("CanvasLayer/StartGui").Visible = false;
		GetNode<Control>("CanvasLayer/ServerGui").Visible = true;
	}

	public void _on_join_pressed()
	{
		string Port = GetNode<TextEdit>("CanvasLayer/StartGui/Port").Text;
		string ip = GetNode<TextEdit>("CanvasLayer/StartGui/Ip").Text;

		if (!Port.IsValidInt())
		{
			GD.Print("Main.cs Invalid Port!!!");
			return;
		}

		if (!ip.IsValidIPAddress())
		{
			GD.Print("Main.cs Invalid ip Address!!!");
			return;
		}

		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		var err = peer.CreateClient(ip, Port.ToInt());
		if(err != Error.Ok) { GD.Print(err); return; }

		Multiplayer.MultiplayerPeer = peer;

		GetNode<Control>("CanvasLayer/StartGui").Visible = false;
	}

	public void _on_switch_lobby_pressed()
	{
		Rpc(nameof(RequestSwitchMap), "res://Scenes/Maps/lobby.tscn", new Vector2(500, 1000));
	}

	public void _on_switch_random_pressed()
	{
		Rpc(nameof(RequestSwitchMap), "res://Scenes/Maps/RandomMap.tscn", new Vector2(5000, 1000));
	}

	private void OnPeerConnected(long id)
	{
		GD.Print($"Peer connected: {id}");

		if (Multiplayer.IsServer())
		{
			GetNode<Node2D>("PlayerHolder").Call("SpawnPlayer", id);
		}
	}

	private void OnPeerDisconnected(long id)
	{
		GD.Print($"Peer disconnected: {id}");
		GetNode<Node2D>("PlayerHolder").Call("FreePlayer", id);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void RequestSwitchMap(string Path, Vector2 MapSize) // Name Is optional
	{
		PackedScene MapScene = GD.Load<PackedScene>(Path);
		if (MapScene == null) { GD.PrintErr("Invalid Map Path Abording!!!"); return; }

		Node Map = MapScene.Instantiate();
		Map.Name = Name;

		foreach (Node child in GetNode("PlayerHolder").GetChildren())
		{
			if (child is Node2D Player)
			{
				if (Player.HasMeta("NotPlayer")) continue;
				Player.GetNode<Node>("CharacterBody2D").Call("ResetPlayerCall", MapSize);
				break;
			}
		}

		foreach (Node child in GetNode("Map").GetChildren()) child.QueueFree();
		GetNode("Map").AddChild(Map);
	}
}
