using System.Drawing;
using System.Runtime.InteropServices;

namespace Helion.Render.Legacy.Renderers.Legacy.World;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LegacyVertex
{
    public float X;
    public float Y;
    public float Z;
    public float U;
    public float V;
    public float LightLevelUnit;
    public float Alpha;
    public float R;
    public float G;
    public float B;
    public float Fuzz;

    public LegacyVertex(float x, float y, float z, float u, float v, short lightLevel = 256,
        float alpha = 1.0f, float fuzz = 0.0f)
    {
        X = x;
        Y = y;
        Z = z;
        U = u;
        V = v;
        LightLevelUnit = lightLevel;
        Alpha = alpha;
        R = 1f;
        G = 1f;
        B = 1f;
        Fuzz = fuzz;
    }
}
