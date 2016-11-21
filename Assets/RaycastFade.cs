using UnityEngine;
using System.Collections;
using DG.Tweening;
public class RaycastFade : MonoBehaviour
{
    public LayerMask castAgainst;
    public Color fadeTo;
    public float fadeSpeed;
    private Transform target;

    private MeshRenderer lastHit;

    void Start()
    {
        if(Necromancer.instance)
        {
            target = Necromancer.instance.transform;
        }
    }

    void Update()
    {
        Ray cast = new Ray(transform.position, target.position - transform.position);
        RaycastHit hit;
        if (Physics.Raycast(cast, out hit, Mathf.Infinity, castAgainst) && hit.collider.transform != target)
        {
            Debug.Log("Hit");
            MeshRenderer render = hit.collider.GetComponentInChildren<MeshRenderer>();
            if (render)
            {
                if (render != lastHit)
                {
                    if (lastHit)
                    {
                        lastHit.material.DOColor(Color.white, fadeSpeed);
                    }
                    lastHit = render;
                    lastHit.material.DOColor(fadeTo, fadeSpeed);
                }
            }
        }
        else
        {
            if (lastHit && lastHit.material.color != Color.white)
            {
                lastHit.material.DOColor(Color.white, fadeSpeed);
            }
        }
    }
}
