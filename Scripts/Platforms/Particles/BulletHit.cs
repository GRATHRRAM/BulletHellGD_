using Godot;
using System;

public partial class BulletHit : GpuParticles2D
{
	public void _on_timer_timeout()
	{
		QueueFree();
	}
}
