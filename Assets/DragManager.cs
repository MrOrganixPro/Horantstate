using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngineInternal;

public class DragManager : MonoBehaviour
{
    public AudioSource audioSource; 
    public AudioClip highlightClip;
    private List<GameObject> lineBucket;
    private Camera cam;
    [SerializeField] private GameObject line;
    [SerializeField] private List<LineAndSelected> selected;
    [SerializeField] private GameObject target;
    GameObject highlightedState;
    public class LineAndSelected
    {
        public GameObject state;
        public GameObject line;
        public LineAndSelected(GameObject state, GameObject line,StateScript.Team team)
        {
            this.state = state;
            this.line = line;
            this.team = team;
        }
        public StateScript.Team team;
    }

    void Awake()
    {
        cam = Camera.main;
    }
    private void Start()
    {
        lineBucket = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            GameObject l = Instantiate(line);
            l.SetActive(false);
            lineBucket.Add(l);
        }
    }
    void Update()
    {
        StateHighLighter();
        if (Input.GetMouseButton(0))
            StartDrag();
        if (selected != null)
        {
            if (selected.Count != 0)
                ContinueDrag();
            if (Input.GetMouseButtonUp(0) && selected.Count != 0) EndDrag();
        }
    }
    void StateHighLighter()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        RaycastHit2D hit = default;

        if (hits.Length == 0)
        {
            if (highlightedState != null)
            {
                transparencyChange = -0.04f; // Fade down if we hit nothing
            }
            return;
        }

        foreach (RaycastHit2D h in hits)
        {
            if (h.collider.gameObject.layer == 12) // 12 is center
            {
                hit = h;
                break;
            }
        }

        Collider2D hitCollider = hit.collider;
        if (hitCollider == null)
        {
            if (highlightedState != null)
            {
                transparencyChange = -0.04f;
            }
        }
        else
        {
            GameObject targetChild = hit.transform.GetChild(0).gameObject;

            if (highlightedState != null && highlightedState != targetChild)
            {
                transparencyChange = -0.04f; // Fade out current before switching
            }
            else
            {
                if (highlightedState != null)
                {
                    // We are already highlighting this exact state, keep pushing it to fade up
                    transparencyChange = 0.04f;
                }
                else
                {
                    // Brand new state selection! Initialize everything
                    highlightedState = targetChild;
                    transparencyChange = 0.04f;
                    highlightedState.SetActive(true);

                    if (HighlightingCR == null)
                    {
                        HighlightingCR = StartCoroutine(GraduallyHighlightState());
                    }

                    audioSource = hit.collider.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        highlightClip = audioSource.clip;
                        HighlightSoundEffect();
                    }
                }
            }
        }
    }

    Coroutine HighlightingCR;
    float transparencyChange = 0.04f;
    WaitForSeconds one_hundredth = new WaitForSeconds(0.01f);

    IEnumerator GraduallyHighlightState()
    {
        Debug.Log("Coroutine Started");

        while (highlightedState != null)
        {
            SpriteRenderer renderer = highlightedState.GetComponent<SpriteRenderer>();
            if (renderer == null) break;

            // Grab all child sprite renderers (like your shield) so they fade together!
            SpriteRenderer[] childRenderers = highlightedState.GetComponentsInChildren<SpriteRenderer>();

            // 1. Calculate what the target alpha would be
            float nextAlpha = renderer.color.a + transparencyChange;

            // 2. Handle the FADING UP boundary
            if (transparencyChange > 0f && nextAlpha >= 0.6f)
            {
                nextAlpha = 0.6f;
                ApplyAlphaToGroup(renderer, childRenderers, nextAlpha);

                // Stay alive, but just wait here as long as we are supposed to hold the highlight
                while (transparencyChange > 0f)
                {
                    yield return one_hundredth;
                }
                continue; 
            }

            // 3. Handle the FADING DOWN boundary
            if (transparencyChange < 0f && nextAlpha <= 0f)
            {
                ApplyAlphaToGroup(renderer, childRenderers, 0f);
                highlightedState.SetActive(false);

                highlightedState = null;
                Debug.Log("Fully faded out. Clearing state target.");
                break;
            }

            // 4. Apply alpha safely if no boundaries were crossed
            ApplyAlphaToGroup(renderer, childRenderers, nextAlpha);

            yield return one_hundredth;
        }

        HighlightingCR = null;
        Debug.Log("Coroutine cleanly ended and tracker set to null.");
    }

    // Helper helper method to cleanly apply alpha down to the nested shields
    private void ApplyAlphaToGroup(SpriteRenderer mainRenderer, SpriteRenderer[] children, float mainAlpha)
    {
        mainRenderer.color = new Color(mainRenderer.color.r, mainRenderer.color.g, mainRenderer.color.b, mainAlpha);
        float fadeProgress = Mathf.InverseLerp(0f, 0.3f, mainAlpha);

        foreach (SpriteRenderer child in children)
        {
            if (child != mainRenderer && child != null)
            {
                // Child elements (shields) map directly to the progress, ending at full 1.0 alpha
                child.color = new Color(child.color.r, child.color.g, child.color.b, fadeProgress);
            }
        }
    }
    /*
    void StateHighLighter()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        RaycastHit2D hit=default;
        if (hits.Length==0)
        {
            if (highlightedState != null)
                {
                    transparencyChange = -0.02f;
                }
            return;
        }
        foreach(RaycastHit2D h in hits)
        {
            if(h.collider.gameObject.layer==12)//12 is center
            {
                hit = h;
                break;
            }
        }
        Collider2D hitCollider = hit.collider;
        if (hitCollider==null)
            {
                if (highlightedState != null)
                {
                    transparencyChange = -0.02f;
                }
            }
        else 
        {
            GameObject targetChild = hit.transform.GetChild(0).gameObject;
            if (highlightedState != null && highlightedState != targetChild)
            {                
                transparencyChange = -0.02f;
            }
            else
            {
                if(highlightedState != null)
                {
                    Debug.Log("Already highlighted");
                }
                else
                {
                    highlightedState = targetChild;
                    transparencyChange = 0.02f;
                    highlightedState.SetActive(true);

                    if(HighlightingCR == null)
                    {
                        HighlightingCR = StartCoroutine(GraduallyHighlightState());
                    }
                    /////////////////////////////////
                    audioSource = hit.collider.GetComponent<AudioSource>();
                    highlightClip = audioSource.clip;
                    HighlightSoundEffect();
                }
            }
        }
    }
    Coroutine HighlightingCR;
    float transparencyChange = 0.02f; //positive value to increase transparency, negative value to decrease transparency
    WaitForSeconds two_hundredth = new WaitForSeconds(0.02f);
    IEnumerator GraduallyHighlightState()
    {
        Debug.Log("in the coroutine");

        if (highlightedState != null)
        {
            SpriteRenderer renderer = highlightedState.GetComponent<SpriteRenderer>();
            while (true)
            {
                Debug.Log("Got in the loop");

                while (true)
                {
                    Debug.Log(renderer.color.a);
                    Color color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, renderer.color.a+transparencyChange);
                    if(color.a <= 0)
                    {
                        Debug.Log("Quits");

                        highlightedState.SetActive(false);
                        break;
                    }
                    if(color.a > 0.3f)
                    {
                        Debug.Log("reached max transparency");
                        break;
                    }   
                    renderer.color = color;
                    yield return two_hundredth;
                }
                if (!(renderer.color.a < 0.3f))
                {
                    yield return two_hundredth;
                }
                else
                    break;
            }
            HighlightingCR = null;
        }
        Debug.Log("Coroutine ended");
        Debug.Log(HighlightingCR);
    }
    */

    void HighlightSoundEffect()
    {
        if (audioSource != null && highlightClip != null)
        {
            audioSource.volume = 0.2f; 
            audioSource.PlayOneShot(highlightClip);
        }
    }


    void StartDrag()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        RaycastHit2D hit=default;
        if (hits.Length==0)
            return;
        foreach(RaycastHit2D h in hits)
        {
            if(h.collider.gameObject.layer==12)//12 is center
            {
                hit = h;
                break;
            }
        }
        Collider2D hitCollider = hit.collider;
        if (hitCollider==null)
            return;
        
        StateScript hitState = hitCollider.GetComponentInParent<StateScript>()!;
        
        if (hitCollider != null && hitState.isPlayersState)
        {
            if (selected == null)
                selected = new List<LineAndSelected>();
            foreach (var s in selected)
            {
                if (s.state == hit.transform.parent.gameObject)
                    return;
            }
            GameObject newLine = ObjectPoolingLine();
            GameObject state = hit.transform.parent.gameObject;
            LineAndSelected newLandS = (new LineAndSelected(state, newLine,state.GetComponent<StateScript>().team));
            selected.Add(newLandS);
            LineRenderer l = newLandS.line.GetComponent<LineRenderer>();
            l.SetPosition(0, newLandS.state.transform.GetChild(0).position);
            l.SetPosition(1, hit.transform.position);
            l.enabled = true;
        }
    }
    GameObject ObjectPoolingLine()
    {
        foreach (GameObject l in lineBucket)
        {
            if (!l.activeSelf)
            {
                l.SetActive(true);
                //l.transform.GetChild(0).gameObject.SetActive(true); bu işlem line tip'te
                return l;
            }
        }
        GameObject li = Instantiate(line);
        lineBucket.Add(li);
        return li;
    }
    void ContinueDrag()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        LineAndSelected toBeRemoved = null;
        foreach (var s in selected)
        {
            StateScript stateScript = s.state.GetComponent<StateScript>();
            if(stateScript.team !=s.team)
            {
                toBeRemoved = s;
                GameObject Line = s.line;
                Renderer lineRenderer = Line.GetComponent<Renderer>();
                Line.transform.GetChild(0).gameObject.SetActive(false);
                Line.SetActive(false);
                lineRenderer.enabled = false;
            }
            LineRenderer l = s.line.GetComponent<LineRenderer>();
            l.SetPosition(1, mousePos);
        }
        selected.Remove(toBeRemoved);
        //dragLine.SetPosition(1, mousePos);
    }

    void EndDrag()
    {
        RaycastHit2D[] hit = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        target = null;
        foreach (RaycastHit2D h in hit)
        {
            GameObject HitObject = h.collider.transform.parent?.gameObject;
            if (HitObject&&HitObject.GetComponent<StateScript>() != null)
            {
                target = HitObject;
                break;
            }
        }
        if (target != null)
        {
            if (selected.Count == 1 && target == selected[0].state)
            {
                goto skip;
            }
            for (int i = selected.Count - 1; i >= 0; i--)
            {
                if (selected[i].state == target)
                {
                    selected[i].line.SetActive(false);

                    selected.RemoveAt(i);
                }
            }
            StateScript targetScript = target.GetComponent<StateScript>();
            if (targetScript != null)
            {
                foreach (var s in selected)
                {
                    StateScript selectedScript = s.state.GetComponent<StateScript>();
                    selectedScript.target = target;
                    selectedScript.SendTroops();
                }
            }
        }
        else
        {
            foreach (var s in selected)
            {
                StateScript selectedScript = s.state.gameObject.GetComponent<StateScript>();
                selectedScript.StopSendingTroops();
            }
        }
    skip:
        foreach (var s in selected)
        {
            GameObject Line = s.line;
            Renderer lineRenderer = Line.GetComponent<Renderer>();
            Line.transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = false;
            Line.SetActive(false);
            lineRenderer.enabled = false;
        }
        selected.Clear();
    }
}
