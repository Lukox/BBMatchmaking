using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class LoginSystem : MonoBehaviour
{

    EventSystem system;

    // Buttons
    public Button loginButton;
    public Button registerButton;
    public Button passwordResetButton;

    // InputFields
    public TMP_InputField mailInputField;
    public TMP_InputField passwordInput;

    public TMP_Text message;

    public Selectable startSelectable;
 

    public static string EntityId;
    public static string SessionTicket;

    // Start is called before the first frame update
    void Start()
    {
        system = EventSystem.current;
        startSelectable.Select();
        loginButton.onClick.AddListener(LoginButton);
        registerButton.onClick.AddListener(RegisterButton);
        passwordResetButton.onClick.AddListener(PasswordResetButton);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null) {
                next.Select();
            }

        } else if (Input.GetKeyDown(KeyCode.Return)) {
            loginButton.onClick.Invoke();
            Debug.Log("Pressed");
        }
    }

    // Button Handling Methods
    public void LoginButton() {
        SceneManager.LoadScene (sceneName:"LobbyPage");
        var request = new LoginWithEmailAddressRequest {
            Email = mailInputField.text,
            Password = passwordInput.text,
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }
    public void RegisterButton() {
        var request = new RegisterPlayFabUserRequest {
            Email = mailInputField.text,
            Password = passwordInput.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);



    }
    public void PasswordResetButton() {
        var request = new SendAccountRecoveryEmailRequest {
            Email = mailInputField.text,
            TitleId = "8DC0F"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);

    }

    // Success and Error Messages
    void OnLoginSuccess (LoginResult result) {
        Debug.Log("Logged In");
        SceneManager.LoadScene (sceneName:"LobbyPage");
        SessionTicket = result.SessionTicket;
        EntityId = result.EntityToken.Entity.Id;
        Debug.Log(EntityId);
    }
    void OnRegisterSuccess (RegisterPlayFabUserResult result) {
        SessionTicket = result.SessionTicket;
        EntityId = result.EntityToken.Entity.Id;
        message.SetText("Account Created");
    }
    void OnPasswordReset (SendAccountRecoveryEmailResult result) {
        message.SetText("Recovery Email Sent");
    }
    void OnError(PlayFabError error) {
        message.SetText("Error");
    }


  


}
