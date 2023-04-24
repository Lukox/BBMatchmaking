using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.MultiplayerModels;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

public class Matchmaking : MonoBehaviour
{


    public Button joinQueueButton;
    public Button leaveQueueButton;
    public TMP_Text queueStatus;

    private string matchId;
    private string ticketId;
    private string[] Players;
    private Coroutine pollTicketCoroutine;
    private const string QueueName = "DefaultQueue";

    public static Matchmaking Instance;

    public static int playerNumber;
    public static string matchID;

    // Start is called before the first frame update
    void Start()
    {
        joinQueueButton.onClick.AddListener(StartMatchmaking);
    }

    // Update is called once per frame
    private void Awake()
    {
        /*if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }*/
    }

    public void StartMatchmaking() {
        //playButton.SetActive(false);
        Debug.Log(LoginSystem.EntityId);
        PlayFabMultiplayerAPI.CreateMatchmakingTicket(
            new CreateMatchmakingTicketRequest 
            {
                Creator = new MatchmakingPlayer 
                {
                    Entity = new EntityKey {
                        Id = LoginSystem.EntityId,
                        Type = "title_player_account"
                    },
                    Attributes = new MatchmakingPlayerAttributes 
                    {
                       DataObject = new { 
                        }
                    }
                },

                GiveUpAfterSeconds = 120,
                QueueName = QueueName
            },
            OnMatchmakingTicketCreated, 
            OnMatchmakingError
        );
    
    }

    private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult result) {
        ticketId = result.TicketId;
        pollTicketCoroutine = StartCoroutine(PollTicket());
        queueStatus.SetText("Ticket created");

    }

    private void OnMatchmakingError(PlayFabError error) {
        Debug.Log(error.GenerateErrorReport());
    }

    private IEnumerator PollTicket() {
        while (true) {
            PlayFabMultiplayerAPI.GetMatchmakingTicket (
                new GetMatchmakingTicketRequest
                {
                    TicketId = ticketId,
                    QueueName = QueueName
                },
                OnGetMatchmakingTicket,
                OnMatchmakingError
            );
            yield return new WaitForSeconds(6);
        }
    }

    private void OnGetMatchmakingTicket(GetMatchmakingTicketResult result) {
        queueStatus.text = $"Status: {result.Status}";

        switch(result.Status) {
            case "Matched":
                StopCoroutine(pollTicketCoroutine);
                StartMatch(result.MatchId);
                break;
            
            case "Canceled":
                break;
        }
    }

    private void StartMatch(string matchId) {
        queueStatus.text = $"Starting Match";
        
        PlayFabMultiplayerAPI.GetMatch(
            new GetMatchRequest
            {
                MatchId = matchId,
                QueueName = QueueName,
            },
            OnGetMatch,
            OnMatchmakingError
        );
    }

    private void CancelMatchmaking()
    {
        PlayFabMultiplayerAPI.CancelMatchmakingTicket(
            new CancelMatchmakingTicketRequest 
            {
                TicketId = ticketId,
                QueueName = QueueName
            },
            OnMatchmakingCanceled,
            OnMatchmakingError
        );
    }

    private void OnMatchmakingCanceled(CancelMatchmakingTicketResult result)
    {
        StopCoroutine(pollTicketCoroutine);
        queueStatus.text = $"Matchmaking canceled";
    }

    private void OnGetMatch(GetMatchResult result) {
        matchID = result.MatchId;
        queueStatus.text = $"{result.Members[0].Entity.Id} vs {result.Members[1].Entity.Id}";
        
        Players = new string[2] {result.Members[0].Entity.Id, result.Members[1].Entity.Id}; 
        AuthorizeRelay(Players);
    }

    private async void AuthorizeRelay(string [] Players) {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        AssignRoles(Players);
   }

   public void AssignRoles(string[] Players){
    if (LoginSystem.EntityId == Players[0]) {
            Debug.Log("HOST");
            playerNumber = 0;
        } else {
            Debug.Log("CLIENT");
            playerNumber = 1;
            //SEND PLAYER 1 to game scene
            //ConnectionNetwork.Instance.joinRelay();
        }
    SceneManager.LoadScene("SampleScene");
   }
   
}
