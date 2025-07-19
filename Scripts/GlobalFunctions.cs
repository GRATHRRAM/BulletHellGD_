using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletHellGD.Scripts
{
    public static class GlobalFunctions
    {
        public static Color RandomColor()
        {
            Random Rand = new Random();
            return new Color(Rand.Next(0, 100) / 100f, Rand.Next(0, 100) / 100f, Rand.Next(0, 100) / 100f);
        }

        public static float GetbrightestColor(Color _Color)
        {
            if (_Color.R > _Color.G && _Color.R > _Color.B) return _Color.R;
            if (_Color.G > _Color.R && _Color.G > _Color.B) return _Color.G;
            if (_Color.B > _Color.R && _Color.B > _Color.G) return _Color.B;
            return 1f;
        }
    }
}
