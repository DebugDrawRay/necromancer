using UnityEngine;
using System.Collections;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(EightWayMovement))]
public class SeekTarget : Action
{
    [System.Serializable]
    public class TargetData
    {
        public string tag;
        public float withinRangeRadius;
    }
    [Header("Seeking")]
    [SerializeField]
    public TargetData[] targetsToSeek;
    public InputActions.Actions seekAction;

    [Header("Ranges")]
    public float defaultRadius;
    public float breakOffRadius;

    [Header("Pathing")]
    public LayerMask pathAvoidance;
    public float atNodeRadius;

    private Seeker seeker;
    private Path currentPath;
    private int currentNodeIndex;

    private InputActions subActions;

    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
        seeker = GetComponent<Seeker>();
    }

    protected override void Execute(InputActions actions)
    {
        subActions = actions;
        if(GetActionInvoke(seekAction, actions))
        {
            FollowTarget(actions.target);
        }
        else
        {
            subActions.primaryDirection = Vector3.zero;
        }
        Debug.DrawLine(transform.position, actions.target.position, Color.yellow);
    }

    void FollowTarget(Transform target)
    {
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist > GetRadius(target) && dist < breakOffRadius)
            {
                Vector3 position = target.position;
                //point to our target
                Vector3 dir = position - transform.position;

                //Check if target is in sight
                Ray sight = new Ray(transform.position, dir);
                RaycastHit debug;
                //check if we see our leader
                if (!Physics.Raycast(sight, out debug, Mathf.Infinity, pathAvoidance))
                {
                    Debug.Log("Target: " + position + " Debug hit: " + debug.point + " Info: " + debug.collider.name);
                    Debug.DrawRay(transform.position, dir, Color.yellow);
                    Debug.DrawLine(transform.position, debug.point, Color.cyan);
                    UnityEditor.EditorApplication.isPaused = true;
                    if (currentPath == null)
                    {
                        seeker.StartPath(transform.position, position, RequestPath);
                    }
                    FollowPath(position);
                }
                else
                {
                    currentPath = null;
                    MoveInDirection(dir);
                }
            }
            else
            {
                subActions.primaryDirection = Vector3.zero;
            }
        }
        else
        {
            subActions.primaryDirection = Vector3.zero;
        }

    }

    void MoveInDirection(Vector3 dir)
    {
        dir = dir.normalized;
        dir.y = dir.z;
        subActions.primaryDirection = dir;
    }

    void FollowPath(Vector3 target)
    {
        if (currentPath != null)
        {
            if (currentNodeIndex < currentPath.path.Count)
            {
                GraphNode node = currentPath.path[currentNodeIndex];

                Vector3 pos = (Vector3)node.position;
                Vector3 direction = pos - transform.position;
                MoveInDirection(direction);

                if (Vector3.Distance(transform.position, pos) < atNodeRadius)
                {
                    currentNodeIndex++;
                }
            }
        }
    }

    void RequestPath(Path p)
    {
        currentPath = p;
        currentNodeIndex = 0;
    }

    float GetRadius(Transform target)
    {
        for(int i = 0; i < targetsToSeek.Length; i++)
        {
            TargetData data = targetsToSeek[i];

            if(data.tag == target.tag)
            {
                return data.withinRangeRadius;
            }
        }
        return defaultRadius;
    }
}

