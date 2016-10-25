using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

[RequireComponent(typeof(PathManager))]
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

    private PathManager pathing;

    private List<GraphNode> pathNodes;
    private GraphNode currentNode;

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
        pathing = GetComponent<PathManager>();
    }

    void FollowPath()
    {
        if (pathing.currentPath.newPath)
        {
            pathNodes = new List<GraphNode>();
            foreach (GraphNode node in pathing.currentPath.path.path)
            {
                pathNodes.Add(node);
            }
            pathing.currentPath.newPath = false;
        }
        if (pathNodes.Count > 0)
        {
            currentNode = pathNodes[0];

            Vector3 pos = (Vector3)currentNode.position;
            Vector3 direction = pos - transform.position;
            direction.y = direction.z;

            direction = direction.normalized;
            direction.x = Mathf.Round(direction.x);
            direction.y = Mathf.Round(direction.y);
            direction.z = Mathf.Round(direction.z);

            actions.primaryDirection = direction;

            if (Vector3.Distance(transform.position, pos) < atNodeRadius)
            {
                pathNodes.Remove(currentNode);
                if (pathNodes.Count > 0)
                {
                    currentNode = pathNodes[0];
                }
            }
        }
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
            target.RecieveDamage(this);
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
                    pathing.SetTarget(currentTarget.transform);
                    currentState = States.Seek;
                }
                break;
            case States.Seek:
                FollowPath();
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
        if (newTarget.tag == primaryTarget && currentTarget.tag == secondaryTarget)
        {
            currentTarget = newTarget;
            currentState = States.Seek;
        }
    }
}