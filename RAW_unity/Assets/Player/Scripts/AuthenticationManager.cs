using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;


public class AuthenticationManager : MonoBehaviour
{

    [Header("Sign Up UI")]
    public TMP_InputField signUpIdInput;
    public TMP_InputField signUpPasswordInput;
    public Button signUpButton;

    [Header("Sign In UI")]
    public TMP_InputField signInIdInput;
    public TMP_InputField signInPasswordInput;
    public Button SignInButton;

    [Header("Status")]
    public Button signOutButton;
    public TMP_Text statusText;

    async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initailized");

            SetupEvents();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error on initialize of sign in: {e}");
        }
    }

    private void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            SetStatus($"Signed In! Player ID: {playerId}");
            Debug.Log($"Player ID: {playerId}");
        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            Debug.LogError($"Sign in failed with error: {err.Message}");
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Player signed out.");
        };
    }

    public async void SignUpWithIdPassword()
    {
        string id = signUpIdInput.text;
        string password = signUpPasswordInput.text;
        Debug.Log(id);
        Debug.Log(password);

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password)) {
            SetStatus("Id and password cannot be empty.");
            return;
        }

        try
        {
            SetStatus("Signing up...");
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(id, password);
            SetStatus("Sign up successful! Please sign in.");
            Debug.Log("Sign up is successful.");
        }
        catch (AuthenticationException ex)
        {
            SetStatus($"Sign up failed: {ex.Message}");
            Debug.LogError(ex);
        }
        catch (RequestFailedException ex)
        {
            SetStatus($"Sign up failed: {ex.Message}");
            Debug.LogError(ex);
        }
    }

    public async void SignInWithIdPassword()
    {
        string id = signInIdInput.text;
        string password = signInPasswordInput.text;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
        {
            SetStatus("Id and password cannot be empty.");
            return;
        }

        try
        {
            SetStatus("Signing in...");
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(id, password);
        }
        catch (AuthenticationException ex)
        {
            SetStatus($"Sign up failed: {ex.Message}");
            Debug.LogError(ex);
        }
        catch (RequestFailedException ex)
        {
            SetStatus($"Sign up failed: {ex.Message}");
            Debug.LogError(ex);
        }
    }

    public void SignOut()
    {
        try
        {
            AuthenticationService.Instance.SignOut();
        }
        catch (Exception ex)
        {
            SetStatus($"Sign out failed: {ex.Message}");
        }
    }

    private void SetStatus(string message)
    {
        statusText.text = message;
        Debug.Log(message);
    }
}
