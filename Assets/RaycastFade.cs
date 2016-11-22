using UnityEngine;
using System.Collections;
using DG.Tweening;
public class RaycastFade : MonoBehaviour
{
    public LayerMask castAgainst;
    public Color fadeTo;
    public float fadeSpeed;
    public Shader transparencyShader;
    private Transform target;

    private MeshRenderer lastHit;
    private Shader lastShader;

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
            MeshRenderer render = hit.collider.GetComponentInChildren<MeshRenderer>();
            if(lastHit != render)
            {
                if (lastHit)
                {
                    StartCoroutine(FadeTo(lastHit, Color.white, fadeSpeed, lastShader));
                }

                lastHit = render;
                lastShader = render.material.shader;
                lastHit.material.shader = transparencyShader;
                lastHit.material.DOColor(fadeTo, fadeSpeed);
            }
        }
        else
        {
            if (lastHit)
            {
                StartCoroutine(FadeTo(lastHit, Color.white, fadeSpeed, lastShader));
                lastHit = null;
            }
        }
    }

    IEnumerator FadeTo(MeshRenderer render, Color to, float speed, Shader result)
    {
        render.material.DOColor(to, speed);
        yield return new WaitUntil(() => { return render.material.color == to; });
        render.material.shader = result;
    }
}
