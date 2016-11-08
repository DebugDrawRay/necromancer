using UnityEngine;

namespace Utilities
{
    public static class Roll
    {
        public static int RangeInt(Vector2 range)
        {
            return Random.Range((int)range.x, (int)range.y);
        }
        public static float RangeFloat(Vector2 range)
        {
            return Random.Range(range.x, range.y);
        }
    }
}
