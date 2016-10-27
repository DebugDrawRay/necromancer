using UnityEngine;
using System.Collections.Generic;

public class MinionManager : Action 
{
    public InputActions.Actions raiseAction;
    public float raiseRange;
    public GameObject minion;
    public string vaildCorpse;

    public List<Minion> activeMinions = new List<Minion>();

    public List<Minion> followingMinions = new List<Minion>();

    protected override void Execute(InputActions actions)
    {
        if(GetActionInvoke(raiseAction, actions) && activeMinions.Count < actions.stats.maxSkeletons)
        {
            CreateNewMinion();
        }
    }

    void CreateNewMinion()
    {
        GameObject[] corpses = GameObject.FindGameObjectsWithTag(vaildCorpse);
        GameObject corpse = null;
        float closestRange = raiseRange + 5;

        for(int i = 0; i < corpses.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, corpses[i].transform.position);
            if(dist < raiseRange && dist < closestRange)
            {
                corpse = corpses[i];
                closestRange = dist;
            }
        }

        if(corpse != null)
        {
            Vector3 position = corpse.transform.position;
            Destroy(corpse);

            GameObject newSkel = (GameObject)Instantiate(minion, position, Quaternion.identity);
            Minion skel = newSkel.GetComponent<Minion>();

            activeMinions.Add(skel);
            followingMinions.Add(skel);

            skel.commander = this;
            skel.gameObject.SetActive(true);
        }
    }
}
