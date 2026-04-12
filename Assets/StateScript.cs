using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using Mono.Cecil.Cil;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class StateScript : MonoBehaviour
{
    public float spaceBetweenTroops = 0.15f;
    public AttackBotType AttackBotPreset = AttackBotType.NotSet;
    [SerializeField] private CenterValue cv;
    Coroutine CurrentCR;
    [SerializeField] Collider2D StateArea;
    [SerializeField] Text troopCountText;

    [SerializeField] private int _troopCount;
    public int troopCount
    {
        get => _troopCount;
        set
        {
            _troopCount = value;
            UpdateTroopCountText();
        }        
    }
    [SerializeField]StateVisuals Visuals;
    [Serializable]
    struct StateVisuals
    {
        public Renderer[] renderers;
        public UnityEngine.UI.Image[] images;
    }
    [SerializeField] SpriteRenderer Transparent;
    #region team Property

    [SerializeField] private Team _team;
    public Team team
    {
        get => _team;
        set
        {
            _team = value;
            StopSendingTroops();
            UpdateVisuals();
            TransparencyFix();
        }
    }
    #endregion team Property
    Color[] teamColors =
    {
        new Color(1,1,1),
        new Color(1,0.5f,0.5f),
        new Color(0.5f,0.72f,0.93f),
        new Color(0.46f,1,0.62f),
        new Color(1,0.95f,0.56f)
    };
    #region isPlayersState Property
    [SerializeField] private bool _isPlayersState;
    public bool isPlayersState
    {
        get => _isPlayersState;
        set
        {
            _isPlayersState = value;
            if (!_isPlayersState && team != Team.None)
            {
                StartCoroutine(AIattackLoop());
            }
        }
    }
    #endregion isPlayersState Property
    [HideInInspector] public GameObject target;
    public GameObject troopPrefab;
    public SpriteRenderer centerSpriteRenderer;
    
    public enum Team
    {
        Blue,
        Red,
        Green,
        Yellow,
        None
    }
    void UpdateTroopCountText()
    {
        troopCountText.text = troopCount.ToString();
    }
    void UpdateVisuals()
    {
        foreach (SpriteRenderer s in Visuals.renderers)
        {
            Color selectedColor = default;
            switch (_team)
            {
                case Team.Red:
                    selectedColor = teamColors[1];
                    break;
                case Team.Blue:
                    selectedColor = teamColors[2];
                    break;
                case Team.Green:
                    selectedColor = teamColors[3];
                    break;
                case Team.Yellow:
                    selectedColor = teamColors[4];
                    break;
                case Team.None:
                default:
                    selectedColor = teamColors[0];
                    break;
            }
            if (s.CompareTag("Darken"))
                s.color = ColorUtils.Darken(selectedColor, cv.centerDarkness);
            else
                s.color = selectedColor;
        }
        foreach(UnityEngine.UI.Image i in Visuals.images)
        {
            Color selectedColor = default;
            switch (_team)
            {
                case Team.Red:
                    selectedColor = teamColors[1];
                    break;
                case Team.Blue:
                    selectedColor = teamColors[2];
                    break;
                case Team.Green:
                    selectedColor = teamColors[3];
                    break;
                case Team.Yellow:
                    selectedColor = teamColors[4];
                    break;
                case Team.None:
                default:
                    selectedColor = teamColors[0];
                    break;
            }
            if (i.CompareTag("Darken"))
                selectedColor = ColorUtils.Darken(selectedColor, cv.centerDarkness);
            bool hasBuilding = false;
            foreach(var bs in buildStructs)
            {
                if (bs.Bool)
                {
                    hasBuilding = true;
                    break;
                }
            }
            selectedColor = (hasBuilding)?new Color(selectedColor.r, selectedColor.g, selectedColor.b, 0.5f): new Color(selectedColor.r, selectedColor.g, selectedColor.b, 0.8f) ;
            i.color = selectedColor;
        }
    }
    void TransparencyFix()
    {
        Color c = ColorUtils.Darken(new Color(Transparent.color.r, Transparent.color.g, Transparent.color.b), 0.4f);
        Transparent.color = new Color(c.r, c.g, c.b, 0.3f);
    }
    IEnumerator TroopIncrementCR()
    {
        float waitTime;
        int increment;
        while (true)
        {
            increment = 1;
            waitTime = 0.5f;
            if (team == Team.None)
            {
                yield return new WaitForSeconds(waitTime);
                if (troopCount < 10)
                    troopCount += increment;
                continue;
            }
            else
            {
                switch (troopCount)
                {
                    case < 10:
                        break;
                    case >= 10 and < 30:
                        waitTime += 0.5f;
                        break;
                    case >= 30 and < 50:
                        waitTime += 1f;
                        break;
                    case >= 50 and < 100:
                        waitTime += 1.5f;
                        break;
                    case >= 100 and < 150:
                        waitTime += 2;
                        increment = 0;
                        break;
                    case >= 150 and < 200:
                        waitTime += 1;
                        increment = -1;
                        break;
                    case >= 200 and < 500:
                        increment = -1;
                        break;
                    case >= 500:
                        waitTime -= 0.25f;
                        increment = -1;
                        break;

                }
            }
            yield return new WaitForSeconds(waitTime);
            troopCount += increment;
        }
    }
    void Awake()
    {
    }
    void Start()
    {
        UpdateEverything();
    }
    private void UpdateEverything()
    {
        BuildUpdate();
        UpdateTroopCountText();
        TransparencyFix();
        StartCoroutine(TroopIncrementCR());
        UpdateVisuals();
    }


    public void SendTroops()
    {
        if (CurrentCR != null)
        {
            StopCoroutine(CurrentCR);
        }
        CurrentCR = StartCoroutine(sendingLoop());
    }
    public void StopSendingTroops()
    {
        if (CurrentCR != null)
        {
            StopCoroutine(CurrentCR);
        }
    }
    IEnumerator sendingLoop()
    {
        Vector3 CenterPos = StateArea.transform.position;

        switch (troopCount)
        {
            case < 20:
                {
                    while (troopCount > 0)
                    {
                        GameObject troop = Instantiate(troopPrefab, CenterPos, Quaternion.identity);
                        TroopScript troopScript = troop.GetComponent<TroopScript>();
                        #region Assigning Vars
                        troopScript.target = target;
                        troopScript.sender = gameObject;
                        troopScript.troopTeam = team;
                        troop.gameObject.layer = LayerAssignment(troopScript.troopTeam);
                        #endregion
                        TroopScript ts = troop.GetComponent<TroopScript>();
                        ts.StartCollisionCheck();
                        ts.March();
                        troopCount--;
                        yield return new WaitForSeconds(0.2f);
                    }
                }
                break;
            case < 30:
                {
                    while (troopCount > 1)
                    {
                        MultInstantiation(2, CenterPos);
                        yield return new WaitForSeconds(0.18f);
                    }
                }
                break;
            case < 40:
                {
                    while (troopCount > 2)
                    {
                        MultInstantiation(3, CenterPos);
                        yield return new WaitForSeconds(0.16f);
                    }
                }
                break;
            case < 50:
                {
                    while (troopCount > 3)
                    {
                        MultInstantiation(4, CenterPos);
                        yield return new WaitForSeconds(0.14f);
                    }
                }
                break;
            case < 60:
                {
                    while (troopCount > 4)
                    {
                        MultInstantiation(5, CenterPos);
                        yield return new WaitForSeconds(0.12f);
                    }
                }
                break;
            case >= 60:
                {
                    while (troopCount > 5)
                    {
                        MultInstantiation(6, CenterPos);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                break;
        }
        yield return new WaitForSeconds(0.2f);

        CurrentCR = null;
    }
    void MultInstantiation(int unitsToDeploy, Vector3 CenterPos)
    {
        for (int i = 0; i < unitsToDeploy; i++)
        {
            GameObject troop = Instantiate(troopPrefab, CenterPos, Quaternion.identity);
            TroopScript troopScript = troop.GetComponent<TroopScript>();
            #region Assigning Vars
            if (isBarracks)
                troopScript.troopDamage = 2;            
            troopScript.target = target;
            troopScript.sender = gameObject;
            troopScript.troopTeam = team;
            troop.gameObject.layer = LayerAssignment(troopScript.troopTeam);
            #endregion
            TroopScript ts = troop.GetComponent<TroopScript>();
            bool troopIsInBetween = i != 0 && i < (unitsToDeploy - 1);
            ts.ArmyOrder(i, unitsToDeploy, 0.15f, troopIsInBetween);
            //0.4f, i, allSoldiers.Length, 1.2f
            ts.StartCollisionCheck();
            ts.March();
            troopCount--;
        }
    }
    int LayerAssignment(Team t)
    {
        switch (t)
        {
            case StateScript.Team.Blue: return 6;
            case StateScript.Team.Red: return 7;
            case StateScript.Team.Green: return 8;
            case StateScript.Team.Yellow: return 9;
            default: return -1;
        }
    }
    public enum AttackBotType
    {
        Greedy,
        NeutralTargeter,
        Nemesis,
        NotSet
    }
    IEnumerator AIattackLoop()
    {
        AttackBotType AttackBot = default;
        if (AttackBotPreset == AttackBotType.NotSet)
        {
            AttackBot = (AttackBotType)UnityEngine.Random.Range(0, (int)AttackBotType.NotSet);
        }
        else
        {
            AttackBot = AttackBotPreset;
        }
        while (!isPlayersState)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(GameManager.Instance.AttackIntervalLower, GameManager.Instance.AttackIntervalUpper) / 100.0f);
            if (isPlayersState)
                break;
            switch (AttackBot)
            {
                case AttackBotType.Greedy: Greedy(); break;
                case AttackBotType.NeutralTargeter: NeutralTargeter(); break;
                case AttackBotType.Nemesis: Debug.Log("I am nemesis, but hungry cause no nemesis() done yet :("); Greedy(); break;
                default: Greedy(); break;
            }
        }
        yield return null;
    }
    struct StateAndHealth
    {
        public GameObject State;
        public int Count;
    }
    void Greedy()
    {
        StateAndHealth[] lowestHealth = new StateAndHealth[3];
        lowestHealth[0].Count = 9999; lowestHealth[1].Count = 9999; lowestHealth[2].Count = 9999;
        GameObject selected = null;
        foreach (GameObject s in GameManager.Instance.States)
        {
            StateScript st = s.GetComponent<StateScript>();
            if (st.team != team)
            {
                if (st.troopCount < lowestHealth[0].Count)
                {
                    lowestHealth[2].Count = lowestHealth[1].Count; lowestHealth[2].State = lowestHealth[1].State;
                    lowestHealth[1].Count = lowestHealth[0].Count; lowestHealth[1].State = lowestHealth[0].State;
                    lowestHealth[0].Count = st.troopCount; lowestHealth[0].State = s;
                }
                else if (st.troopCount < lowestHealth[1].Count)
                {
                    lowestHealth[2].Count = lowestHealth[1].Count; lowestHealth[2].State = lowestHealth[1].State;
                    lowestHealth[1].Count = st.troopCount; lowestHealth[1].State = s;
                }
                else if (st.troopCount < lowestHealth[2].Count)
                {
                    lowestHealth[2].Count = st.troopCount; lowestHealth[2].State = s;
                }
            }
        }
        if (lowestHealth[0].State == null)
            return;
        if (lowestHealth[1].State == null)
            selected = lowestHealth[0].State;
        else if (lowestHealth[2].State == null)
            selected = lowestHealth[UnityEngine.Random.Range(0, 2)].State;
        else
            selected = lowestHealth[UnityEngine.Random.Range(0, 3)].State;
        target = selected;
        SendTroops();
    }
    void NeutralTargeter()
    {
        List<GameObject> noneStates = new();

        foreach (GameObject s in GameManager.Instance.States)
        {
            StateScript st = s.GetComponent<StateScript>();
            if (st.team == Team.None)
            {
                noneStates.Add(s);
            }
        }
        if (noneStates.Count != 0)
        {
            target = noneStates[UnityEngine.Random.Range(0, noneStates.Count)];
        }
        else
        {
            RandomAttack();
        }
        SendTroops();
    }
    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);


            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    void RandomAttack()
    {
        List<int> randomizedStates = Enumerable.Range(0, GameManager.Instance.States.Length).ToList();
        Shuffle(randomizedStates);
        for (int i = 0; i < randomizedStates.Count; i++)
        {
            target = GameManager.Instance.States[i];
            if (target.GetComponent<StateScript>().team != this.team)
                break;
        }
    }
    void BuildUpdate()
    {
        for (int i = 0; i < buildStructs.Length; i++)
        {
            if (buildStructs[i].Bool)
            {
                centerSpriteRenderer.sprite = buildStructs[i].BuildSprite;
                break;
            }
        }
    }
    [System.Serializable]
    struct Build_Struct
    {
        public Build Build;
        public bool Bool;
        public Sprite BuildSprite;
    }
    [SerializeField] Build_Struct[] buildStructs;
    private bool isFort,isBarracks =false;

    enum Build
    {
        Fort,
        Barracks,
        build3
    }
    bool blockAttack =false;
    public void DealDamageToState(int damage, Team attackerTeam)
    {
        if (/*Fort*/buildStructs[0].Bool && blockAttack==true) { blockAttack = false; }
        else
        {
            blockAttack = true;
            if (troopCount-damage < 0)
            {
                StateScript self = this;
                GameManager.Instance.UpdateTeam(ref self, attackerTeam);
                troopCount = Math.Abs(troopCount-damage);
            }
            else
                troopCount -= damage;
        }
    }
}
