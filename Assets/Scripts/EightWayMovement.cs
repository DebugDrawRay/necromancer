using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EightWayMovement : Action 
{
    public InputActions.Actions movementAction;
    [Range(0,1)]
    public float acceleration;
    private Rigidbody rigid;

    private float currentSpeed;
    private Vector3 currentDirection;

    protected override void InitializeOnAwake()
    {
        base.InitializeOnAwake();
        rigid = GetComponent<Rigidbody>();
    }
    protected override void Execute(InputActions actions)
    {
        Vector3 direction = Vector3.zero;
        switch (movementAction)
        {
            case InputActions.Actions.PrimaryDirection:
                direction = actions.primaryDirection;
                break;
            case InputActions.Actions.SecondaryDirection:
                direction = actions.secondaryDirection;
                break;
        }

        //if (direction == Vector3.zero)
        //{
        //    if (currentSpeed > 0)
        //    {
        //        currentSpeed -= acceleration * Time.deltaTime;
        //    }
        //}
        //else
        //{
        //    currentDirection = direction;
        //    if (currentSpeed <= actions.stats.speed)
        //    {
        //        currentSpeed += acceleration * Time.deltaTime;
        //    }
        //}

        //Vector3 forward = transform.forward * currentDirection.y;
        //Vector3 right = transform.right * currentDirection.x;
        //Vector3 move = (forward + right) * currentSpeed;
        //move.y = rigid.velocity.y;
        //rigid.velocity = move;

        Vector3 forward = transform.forward * direction.y * actions.stats.speed;
        Vector3 right = transform.right * direction.x * actions.stats.speed;
        Vector3 move = forward + right;
        move.y = rigid.velocity.y;

        
        rigid.velocity = Vector3.Lerp(rigid.velocity, move, acceleration);
    }
}
