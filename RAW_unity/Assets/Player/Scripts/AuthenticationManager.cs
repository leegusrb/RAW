using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;


public class AuthenticationManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject signInPage;
    public GameObject signUpPage;

    [Header("Sign In UI")]
    public TMP_InputField signInIdInput;
    public TMP_InputField signInPasswordInput;
    public Button signInButton;
    public Button goToSignUpButton;

    [Header("Sign Up UI")]
    public TMP_InputField signUpIdInput;
    public TMP_InputField signUpPasswordInput;
    public TMP_InputField signUpPasswordCheckInput;
    public Button signUpButton;
    public Button goToSignInButton;

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

    void Start()
    {
        // --- 버튼 이벤트 리스너 할당 ---
        // 페이지 전환 버튼
        goToSignUpButton.onClick.AddListener(ShowSignUpPage);
        goToSignInButton.onClick.AddListener(ShowSignInPage);

        // 기능 버튼
        signInButton.onClick.AddListener(SignInWithIdPassword);
        signUpButton.onClick.AddListener(SignUpWithIdPassword);
        signOutButton.onClick.AddListener(SignOut);

        // 초기 화면 설정
        ShowSignInPage();
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

    private void SetStatus(string message)
    {
        statusText.text = message;
        Debug.Log(message);
    }

    #region Page Navigation
    public void ShowSignInPage()
    {
        SetStatus("로그인");
        signInPage.SetActive(true);
        signUpPage.SetActive(false);
    }

    public void ShowSignUpPage()
    {
        SetStatus("회원가입");
        signInPage.SetActive(false);
        signUpPage.SetActive(true);
    }
    #endregion

    #region Authenticaiton Logic
    public async void SignInWithIdPassword()
    {
        string id = signInIdInput.text;
        string password = signInPasswordInput.text;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
        {
            SetStatus("아이디와 비밀번호를 입력해주십시오.");
            return;
        }

        try
        {
            SetStatus("로그인 중...");
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(id, password);
            SetStatus("로그인 성공!");
            Debug.Log("로그인 성공!");
            // 메인메뉴 씬으로 전환
        }
        catch (AuthenticationException ex)
        {
            SetStatus($"로그인 실패: {ex.Message}");
            Debug.LogError(ex);
        }
        catch (RequestFailedException ex)
        {
            SetStatus($"로그인 실패: {ex.Message}");
            Debug.LogError(ex);
        }
    }

    public async void SignUpWithIdPassword()
    {
        string id = signUpIdInput.text;
        string password = signUpPasswordInput.text;
        string passwordCheck = signUpPasswordCheckInput.text;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
        {
            SetStatus("아이디와 비밀번호를 입력해주십시오.");
            return;
        }

        if (!password.Equals(passwordCheck))
        {
            SetStatus("비밀번호가 일치하지 않습니다.");
            return;
        }

        try
        {
            SetStatus("회원가입 진행 중...");
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(id, password);
            SetStatus("회원가입 성공! 로그인 페이지로 이동합니다.");
            Debug.Log("Sign up is successful.");
            Invoke("ShowSignInPage", 2f);
        }
        catch (AuthenticationException ex)
        {
            SetStatus($"회원가입 실패: {ex.Message}");
            Debug.LogError(ex);
        }
        catch (RequestFailedException ex)
        {
            SetStatus($"회원가입 실패: {ex.Message}");
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
    #endregion
}
