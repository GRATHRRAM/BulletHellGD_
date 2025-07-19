using BulletHellGD.Scripts;
using Godot;
using System;

public partial class NormalPlatform : Node2D
{
	public override void _EnterTree()
	{
		base._EnterTree();

		GetNode<Sprite2D>("StaticBody2D/Sprite").Modulate = GlobalFunctions.RandomColor();

        ShaderMaterial mat = GetNode<Sprite2D>("StaticBody2D/Sprite").Material as ShaderMaterial;
        if (mat != null)
        {
            if (GlobalFunctions.GetbrightestColor(GetNode<Sprite2D>("StaticBody2D/Sprite").Modulate) > 0.6f)
            {
                float glowstr = (float)mat.GetShaderParameter("glow_strength");
                glowstr *= 0.8f;
                mat.SetShaderParameter("glow_strength", glowstr);
            }
        }
    }
}
