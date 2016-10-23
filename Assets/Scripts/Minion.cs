using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

[RequireComponent(typeof(PathManager))]
public class Minion : Actor 
{
    [HideInInspector]
    public MinionManager commander;
    public float speedVariation;

    [Header("Positioning")]
    public float checkTargetRadius;
    public float atPositionRadius;
    public float followRadius;
    public float attackRadius;

    //New Stuff AGAIN LOLOLOLOLOLOLOL
    private PathManager pathing;
    private int currentPathIndex;
    public float nodeRange;

    public List<GraphNode> nodes;
    private GraphNode currentNode;

    public bool withinNecroRange
    {
        get
        {
            return Vector3.Distance(transform.position, Necromancer.instance.transform.position) <= followRadius;
        }
    }

    private bool withinGroupRange
    {
        get
        {
            for (int i = 0; i < commander.activeMinions.Count; i++)
            {
                Minion skel = commander.activeMinions[i];
                if (Vector3.Distance(transform.position, skel.transform.position) <= followRadius && skel.withinNecroRange)
                {
                    return true;
                }
            }
            return false;
        }
    }


    [Header("Combat")]
    public string validTargets;
    public LayerMask targetLayers;

    public float attackInterval;
    private float currentInterval;
    public bool withinAttackRange
    {
        get
        {
            return Vector3.Distance(transform.position, currentTarget.transform.position) <= attackRadius;
        }
    }

    private Transform currentTarget;
    private Actor enemyTarget
    {
        get
        {
            if(currentTarget.GetComponent<Actor>())
            {
                return currentTarget.GetComponent<Actor>();
            }
            return null;
        }
    }
    private Transform previousTarget;

    public enum States
    {
        Idle,
        MoveToLocation,
        Attack
    }
    public States currentState;

    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
        pathing = GetComponent<PathManager>();
        currentInterval = attackInterval;
        SkeletonEvent.BroadcastEvent += ChangeLocation;
    }

    void Start()
    {
        currentStats.speed += Random.Range(-speedVariation, speedVariation);
        UpdateBaseStats();
    }

    void OnEnable()
    {
        currentTarget = commander.transform;
        previousTarget = commander.transform;
        pathing.SetTarget(currentTarget);
    }

    void Update()
    {
        RunStates();
        bus.Action(actions);
    }

    void Follow(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = direction.z;
        actions.primaryDirection = direction.normalized;
    }

    void FollowPath()
    {
        if(pathing.currentPath.newPath)
        {
            nodes = new List<GraphNode>();
            foreach(GraphNode node in pathing.currentPath.path.path)
            {
                nodes.Add(node);
            }
            if (currentNode == null)
            {
                currentNode = nodes[0];
            }

            //currentPathIndex = 0;
            pathing.currentPath.newPath = false;
        }
        else
        {
            //Path path = pathing.currentPath.path;
            //Vector3 pos = path.vectorPath[currentPathIndex];

            Vector3 pos = (Vector3)currentNode.position;
            Vector3 direction = pos - transform.position;
            direction.y = direction.z;
            actions.primaryDirection = direction.normalized;
            
            if(Vector3.Distance(transform.position, pos) < nodeRange)
            {
                nodes.Remove(currentNode);
                if (nodes.Count > 0)
                {
                    currentNode = nodes[0];
                }
                /*currentPathIndex++;
                if(currentPathIndex >= path.vectorPath.Count)
                {
                    currentPathIndex--;
                }*/
            }
        }
    }

    void MoveTo(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = direction.z;
        actions.primaryDirection = direction.normalized;
    }

    void Attack(Actor target)
    {
        if (currentInterval > 0)
        {
            currentInterval -= Time.deltaTime;
        }
        else
        {
            target.DealDamage(currentStats.strength);
            currentInterval = attackInterval;
        }
    }

    public void ChangeLocation(Transform trans)
    {
        List<Actor> enemies = CheckForEnemies(trans, checkTargetRadius, targetLayers, validTargets);
        if (enemies.Count > 0)
        {
            currentTarget = FindClosestTarget(enemies, trans, checkTargetRadius).transform;
            if (previousTarget != currentTarget)
            {
                previousTarget = currentTarget;
                pathing.SetTarget(currentTarget);
                currentState = States.MoveToLocation;
            }
        }
        else
        {
            currentTarget = trans;
            previousTarget = trans;
            pathing.SetTarget(currentTarget);
            currentState = States.MoveToLocation;
        }
    }

    public void ReturnHome(Transform home)
    {
        currentTarget = home;
        previousTarget = home;
        pathing.SetTarget(currentTarget);
        currentState = States.MoveToLocation;
    }

    List<Actor> CheckForEnemies(Transform origin, float radius, LayerMask layers, string tagToWatch)
    {
        List<Actor> enemies = new List<Actor>();

        Collider[] hits = Physics.OverlapSphere(origin.position, radius, layers);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == tagToWatch)
            {
                enemies.Add(hits[i].GetComponent<Actor>());
            }
        }
        return enemies;
    }

    Actor FindClosestTarget(List<Actor> targets, Transform origin, float radius)
    {
        Actor closest = null;
        float closestRange = radius + 5;
        for (int i = 0; i < targets.Count; i++)
        {
            Actor select = targets[i];
            float dist = Vector3.Distance(origin.position, select.transform.position);
            if (dist <= closestRange)
            {
                closest = select;
                closestRange = dist;
            }
        }
        return closest;
    }

    void RunStates()
    {
        switch(currentState)
        {
            case States.Idle:
                currentNode = null;
                actions.primaryDirection = Vector3.zero;
                if (currentTarget.GetComponent<MinionManager>())
                {
                    if (!withinNecroRange || !withinGroupRange)
                    {
                        currentState = States.MoveToLocation;
                    }
                }
                else
                {
                    List<Actor> enemiesInRange = CheckForEnemies(transform, checkTargetRadius, targetLayers, validTargets);
                    if (enemiesInRange.Count > 0)
                    {
                        currentTarget = FindClosestTarget(enemiesInRange, transform, checkTargetRadius).transform;
                        previousTarget = currentTarget;
                        currentState = States.MoveToLocation;
                    }
                }
                break;
            case States.Attack:
                actions.primaryDirection = Vector3.zero;
                Attack(currentTarget.GetComponent<Actor>());
                if(currentTarget == null)
                {
                    currentState = States.Idle;
                }
                if(!withinAttackRange)
                {
                    currentState = States.MoveToLocation;
                }
                break;
            case States.MoveToLocation:
                if(currentTarget.tag == validTargets)
                {
                    //Follow(currentTarget);
                    FollowPath();
                    if(withinAttackRange)
                    {
                        currentState = States.Attack;
                    }
                }
                else if(currentTarget.GetComponent<MinionManager>())
                {
                    //Follow(currentTarget);
                    FollowPath();
                    if (withinNecroRange)
                    {
                        currentState = States.Idle;
                    }
                }
                else
                {
                    //Follow(currentTarget);
                    FollowPath();
                    if(Vector3.Distance(transform.position, currentTarget.position) <= atPositionRadius)
                    {
                        currentState = States.Idle;
                    }
                }
                break;
        }
    }
}

