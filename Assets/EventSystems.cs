using UnityEngine;
using System.Collections;

public class SkeletonEvent 
{
    public delegate void BroadcastPosition(Vector3 position);
    public static BroadcastPosition BroadcastEvent;
    public static void Broadcast(Vector3 position)
    {
        if(BroadcastEvent != null)
        {
            BroadcastEvent(position);
        }
    }
}
