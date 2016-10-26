using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

[RequireComponent(typeof(Seeker))]
public class SeekAndDestroy : Actor
{
    public enum States
    {
        Idle,
        Seek,
        Attack
    }
    public States currentState;

    [Header("Ranges")]
    public float seekRadius;
    public float breakOffRadius;
    public float attackRadius;
    public float atNodeRadius;

    [Header("Attacking")]
    public LayerMask targetingMask;
    public string primaryTarget;
    public string secondaryTarget;

    public float attackInterval;
    private float currentInterval;

    private Actor currentTarget;

    [Header("Pathing")]
    public LayerMask checkAvoidanceMask;

    private Path currentPath;

    private Seeker seeker;
    private int currentNodeIndex;

    public bool withinAttackRange
    {
        get
        {
            return Vector3.Distance(transform.position, currentTarget.transform.position) <= attackRadius;
        }
    }

    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
        seeker = GetComponent<Seeker>();
    }

    void Follow(Vector3 position)
    {
        //point to our target
        Vector3 dir = position - transform.position;

        //Check if target is in sight
        Ray sight = new Ray(transform.position, dir);

        //check if we see our leader
        if (Physics.Raycast(sight, Mathf.Infinity, checkAvoidanceMask))
        {
           Debug.Log("Not in sight");
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

    void MoveInDirection(Vector3 dir)
    {
        dir = dir.normalized;
        dir.y = dir.z;
        /*dir.x = Mathf.Round(dir.x);
        dir.y = Mathf.Round(dir.y);
        dir.z = Mathf.Round(dir.z);*/

        Debug.DrawRay(transform.position, dir);
        actions.primaryDirection = dir;
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

    Actor CheckForClosestEnemy(Transform origin, float radius, LayerMask layers, string primaryTarget, string secondaryTarget)
    {
        List<Actor> primaryTargets = new List<Actor>();
        List<Actor> secondaryTargets = new List<Actor>();

        Collider[] hits = Physics.OverlapSphere(origin.position, radius, layers);
        for (int i = 0; i < hits.Length; i++)
        {
            Actor hit = hits[i].GetComponent<Actor>();
            if(hit.tag == primaryTarget)
            {
                primaryTargets.Add(hit);
            }
            if(hit.tag == secondaryTarget)
            {
                secondaryTargets.Add(hit);
            }
        }

        List<Actor> enemies = new List<Actor>();

        if(primaryTargets.Count > 0)
        {
            enemies = primaryTargets;
        }
        else
        {
            enemies = secondaryTargets;
        }

        Actor closest = null;
        float closestRange = radius + 5;
        for (int i = 0; i < enemies.Count; i++)
        {
            Actor select = enemies[i];
            float dist = Vector3.Distance(origin.position, select.transform.position);
            if (dist <= closestRange)
            {
                closest = select;
                closestRange = dist;
            }
        }

        return closest;
    }

    void Attack(Actor target)
    {
        if (currentInterval > 0)
        {
            currentInterval -= Time.deltaTime;
        }
        else
        {
            /*target.RecieveDamage(this);*/
            currentInterval = attackInterval;
        }
    }

    void Update()
    {
        RunStates();
        bus.Action(actions);
    }

    void RunStates()
    {
        switch(currentState)
        {
            case States.Idle:
                actions.primaryDirection = Vector3.zero;
                currentTarget = CheckForClosestEnemy(transform, seekRadius, targetingMask, primaryTarget, secondaryTarget);
                if(currentTarget != null)
                {
                    currentState = States.Seek;
                }
                break;
            case States.Seek:
                Follow(currentTarget.transform.position);
                float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
                if(dist > breakOffRadius)
                {
                    currentState = States.Idle;
                }
                if(withinAttackRange)
                {
                    currentState = States.Attack;
                }
                CheckForNewPrimary();
                break;
            case States.Attack:
                actions.primaryDirection = Vector3.zero;
                Attack(currentTarget);
                if(!withinAttackRange)
                {
                    currentState = States.Seek;
                }
                CheckForNewPrimary();
                break;
        }
    }

    void CheckForNewPrimary()
    {
        Actor newTarget = CheckForClosestEnemy(transform, seekRadius, targetingMask, primaryTarget, secondaryTarget);
        if (newTarget != null && newTarget.tag == primaryTarget && currentTarget.tag == secondaryTarget)
        {
            currentTarget = newTarget;
            currentState = States.Seek;
        }
    }
}