using UnityEngine;
using Pathfinding;
using System.Collections.Generic;

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
    public float attackRadius;

    [Header("Targeting")]
    public LayerMask targetingMask;
    public string primaryTarget;
    public string secondaryTarget;

    public bool withinAttackRange
    {
        get
        {
            return Vector3.Distance(transform.position, actions.target.position) <= attackRadius;
        }
    }

    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
    }

    Transform CheckForClosestEnemy(Transform origin, float radius, LayerMask layers, string primaryTag, string secondaryTag)
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

        Transform closest = null;
        float closestRange = radius + 5;
        for (int i = 0; i < enemies.Count; i++)
        {
            Actor select = enemies[i];
            float dist = Vector3.Distance(origin.position, select.transform.position);
            if (dist <= closestRange)
            {
                closest = select.transform;
                closestRange = dist;
            }
        }

        return closest;
    }

    void Update()
    {
        actions.target = CheckForClosestEnemy(transform, seekRadius, targetingMask, primaryTarget, secondaryTarget);
        RunStates();
        bus.Action(actions);
    }

    void RunStates()
    {
        switch(currentState)
        {
            case States.Idle:
                actions.primaryAction = false;
                actions.secondaryAction = false;
                if(actions.target != null)
                {
                    currentState = States.Seek;
                }
                break;
            case States.Seek:
                actions.primaryAction = true;
                actions.secondaryAction = false;
                if(actions.target == null)
                {
                    currentState = States.Idle;
                }
                else
                {
                    if(withinAttackRange)
                    {
                        currentState = States.Attack;
                    }
                }
                break;
            case States.Attack:
                actions.primaryAction = false;
                actions.secondaryAction = true;
                if (actions.target == null)
                {
                    currentState = States.Idle;
                }
                else
                {
                    if (!withinAttackRange)
                    {
                        currentState = States.Seek;
                    }
                }
                break;
        }
    }
}