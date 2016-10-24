using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EightWayMovement : Action 
{
    public InputActions.Actions movementAction;
    [Range(0,1)]
    public float acceleration;
    private Rigidbody rigid;

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

        Vector3 forward = transform.forward * direction.y * actions.stats.speed;
        Vector3 right = transform.right * direction.x * actions.stats.speed;
        Vector3 move = forward + right;
        move.y = rigid.velocity.y;

        rigid.velocity = Vector3.Lerp(rigid.velocity, move, acceleration);
    }
}
