using System.Collections;
using UnityEngine;
using static StateScript;

public class GameManager : MonoBehaviour
{
    public StateScript.Team PlayerTeam;

    public int AttackIntervalLower;
    public int AttackIntervalUpper;
    public static GameManager Instance { get; private set; }
    public GameObject[] States;

    [SerializeField] private TeamStats[] teamStats;
    [System.Serializable]
    public struct TeamStats
    {
        public StateScript.Team team;
        public int count;
        public void ChangeCount(int i)
        {
            count += i;
        }
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        foreach (GameObject s in States)
        {
            StateScript st = s.GetComponent<StateScript>();
            UpdateTeam(ref st);
        }

    }
    public void UpdateTeam(ref StateScript toBeUpdated)
    {
        toBeUpdated.isPlayersState = toBeUpdated.team == PlayerTeam;
        TeamIncrement(toBeUpdated.team, 1);
    }
    void TeamIncrement(StateScript.Team t, int x)
    {
        switch (t)
        {
            case Team.Blue:
                teamStats[1].ChangeCount(x);
                break;
            case Team.Red:
                teamStats[2].ChangeCount(x);
                break;
            case Team.Green:
                teamStats[3].ChangeCount(x);
                break;
            case Team.Yellow:
                teamStats[4].ChangeCount(x);
                break;
            case Team.None:
            default:
                teamStats[0].ChangeCount(x);
                break;
        }
    }
    TeamStats GetTeamStats(Team team)
    {
        switch (team)
        {
            case Team.Blue:
                return teamStats[1];
            case Team.Red:
                return teamStats[2];
            case Team.Green:
                return teamStats[3];
            case Team.Yellow:
                return teamStats[4];
            case Team.None:
            default:
                return teamStats[0];
        }
    }
    public Color GetTeamColor(Team team)
    {
        switch (team)
        {
            case Team.Blue:
                return Color.blue;
            case Team.Red:
                return Color.red;
            case Team.Green:
                return Color.green;
            case Team.Yellow:
                return Color.yellow;
            case Team.None:
            default:
                return Color.white;
        }
    }

    public void UpdateTeam(ref StateScript toBeUpdated, StateScript.Team UpdatingToThisTeam)
    {
        TeamIncrement(toBeUpdated.team, -1);
        toBeUpdated.team = UpdatingToThisTeam;
        TeamIncrement(UpdatingToThisTeam, 1);
        toBeUpdated.isPlayersState = UpdatingToThisTeam == PlayerTeam;
        if ((GetTeamStats(PlayerTeam).count + GetTeamStats(StateScript.Team.None).count) == States.Length)
            Debug.Log("You Won");
        if (GetTeamStats(PlayerTeam).count==0)
            Debug.Log("You Lost");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
