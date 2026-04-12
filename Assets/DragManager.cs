using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using Unity.Mathematics;
using UnityEngine;
using UnityEngineInternal;

public class DragManager : MonoBehaviour
{
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
        public LineAndSelected(GameObject state, GameObject line)
        {
            this.state = state;
            this.line = line;
        }
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
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Center"))
        {
            GameObject targetChild = hit.transform.GetChild(0).gameObject;
            if (highlightedState != null && highlightedState != targetChild)
                highlightedState.SetActive(false);
            highlightedState = targetChild;
            highlightedState.SetActive(true);
        }
        else
        {
            if (highlightedState != null)
            {
                highlightedState.SetActive(false);
                highlightedState = null;
            }
        }
    }


    void StartDrag()
    {
        RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider == null)
            return;
        StateScript hitState = hit.collider.GetComponentInParent<StateScript>()!;
        if (hitState == null)
            return;
        if (hit.collider != null && hitState.isPlayersState)
        {
            if (selected == null)
                selected = new List<LineAndSelected>();
            foreach (var s in selected)
            {
                if (s.state == hit.transform.parent.gameObject)
                    return;
            }
            GameObject newLine = ObjectPoolingLine();
            LineAndSelected newLandS = (new LineAndSelected(hit.transform.parent.gameObject, newLine));
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
        foreach (var s in selected)
        {
            LineRenderer l = s.line.GetComponent<LineRenderer>();
            l.SetPosition(1, mousePos);
        }
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
            Line.SetActive(false);
            lineRenderer.enabled = false;
        }
        selected.Clear();
        line.transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = (false);
    }
}
