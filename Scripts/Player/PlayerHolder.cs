using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHolder : Node2D
{
	[Export]
	private PackedScene PlayerScene = null;

	[Export]
	private Node2D Camera = null;

	[Export]
	public float MinZoom = 0.15f;

	[Export]
	public float MaxZoom = 1f;

	[Export]
	public float ZoomSpeed = 5f;

	public override void _Ready()
	{
		base._Ready();

		if (PlayerScene == null) GD.Print("PlayerHolder PlayerScene Null!!!!");
		if (Camera == null)      GD.Print("PlayerHolder Camera Null!!!!");
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		Vector2 CamDest = Vector2.Zero;

		Int16 PlayerCount = 0;

		foreach (Node child in GetChildren())
		{
			if (child is Node2D Player)
			{
				if (Player.HasMeta("NotPlayer")) continue;
				CamDest += Player.GetNode<CharacterBody2D>("CharacterBody2D").Position;
				PlayerCount++;
			}
		}

		if (PlayerCount == 0) PlayerCount++;
		CamDest /= PlayerCount;

		Vector2 CamPos = Camera.Position;
		CamPos.X = Mathf.Lerp(CamPos.X, CamDest.X, Mathf.Ease(0.5f, 2.45f));
		CamPos.Y = Mathf.Lerp(CamPos.Y, CamDest.Y, Mathf.Ease(0.5f, 2.45f));
		Camera.Position = CamPos;

		if (PlayerCount > 1)
		{
			List<Vector2> playerPositions = new();

			foreach (Node child in GetChildren())
			{
				if (child is Node2D player && !player.HasMeta("CamHolder"))
				{
					if (player.HasMeta("NotPlayer")) continue;
					var body = player.GetNode<CharacterBody2D>("CharacterBody2D");
					playerPositions.Add(body.Position);
				}
			}

			float maxDistance = 0f;

			for (int i = 0; i < playerPositions.Count - 1; i++)
			{
				for (int j = i + 1; j < playerPositions.Count; j++)
				{
					float dist = playerPositions[i].DistanceTo(playerPositions[j]);
					if (dist > maxDistance)
						maxDistance = dist;
				}
			}

			maxDistance *= 1.5f;
			float zoomFactor = Mathf.Clamp(MaxZoom - (maxDistance / 3000f), MinZoom, MaxZoom);

			Vector2 currentZoom = Camera.GetNode<Camera2D>("Camera2D").Zoom; ;
			Vector2 targetZoom = new Vector2(zoomFactor, zoomFactor);
			Camera.GetNode<Camera2D>("Camera2D").Zoom = currentZoom.Lerp(targetZoom, (float)delta * ZoomSpeed);
		}
		else { Camera.GetNode<Camera2D>("Camera2D").Zoom = Camera.GetNode<Camera2D>("Camera2D").Zoom.Lerp(new Vector2(0.3f,0.3f), (float)delta * ZoomSpeed); }
	}


	public void SpawnPlayer(long pid)
	{
		Node2D Player = PlayerScene.Instantiate<Node2D>();
		Player.Name = pid.ToString();
		AddChild(Player, true);
	}

	public void FreePlayer(long pid)
	{
		foreach (Node child in GetChildren())
		{
			if (child is Node2D Player)
			{
                if (Player.HasMeta("NotPlayer")) continue;
                if (Player.Name == pid.ToString()) Player.QueueFree();
			}
		}
	}
}
