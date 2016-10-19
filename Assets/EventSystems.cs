using UnityEngine;

public static class SkeletonEvent 
{
    public delegate void BroadcastPosition(Transform trans);
    public static BroadcastPosition BroadcastEvent;
    public static void Broadcast(Transform trans)
    {
        if(BroadcastEvent != null)
        {
            BroadcastEvent(trans);
        }
    }
}
