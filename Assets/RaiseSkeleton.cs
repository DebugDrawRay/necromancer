using UnityEngine;
using System.Collections.Generic;

public class RaiseSkeleton : Action 
{
    public GameObject skeleton;

    public List<Skeleton> activeSkeletons = new List<Skeleton>();

    public List<Skeleton> followingSkeletons = new List<Skeleton>();

    protected override void Execute(InputActions actions)
    {
        bool invokePrimary = false;
        switch(primaryActionToRegister)
        {
            case InputActions.Actions.PrimaryAction:
                invokePrimary = actions.primaryAction;
                break;
            case InputActions.Actions.SecondaryAction:
                invokePrimary = actions.secondaryAction;
                break;
            case InputActions.Actions.TertiaryAction:
                invokePrimary = actions.tertiaryAction;
                break;
        }

        if(invokePrimary && activeSkeletons.Count < actions.stats.maxSkeletons)
        {
            //Temporary Positioning
            Vector3 direction = Random.insideUnitCircle * 2;
            direction.z = direction.y;
            direction.y = 0;
            Vector3 position = transform.position + direction;

            GameObject newSkel = (GameObject)Instantiate(skeleton, position, Quaternion.identity);
            Skeleton skel = newSkel.GetComponent<Skeleton>();

            activeSkeletons.Add(skel);
            followingSkeletons.Add(skel);

            skel.necro = this;
            skel.gameObject.SetActive(true);
        }
    }
}
