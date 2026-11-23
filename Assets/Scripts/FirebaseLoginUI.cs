
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class FirebaseLoginUI : MonoBehaviour
{
    // ==========================================
    // 1. KHAI BAO GIAO DIEN
    // ==========================================

    [Header("LOGIN UI")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    [Header("WELCOME PANEL")]
    public GameObject welcomePanel;

    [Header("LOGIN / REGISTER FORM")]
    public GameObject authFormPanel;
    public GameObject loginButtonObject;
    public GameObject registerButtonObject;
    public TMP_Text formTitleText;

    [Header("SETTINGS")]
    public string startMenuSceneName = "StartMenu";
    public bool requireLoginEveryLaunch = true;

    // ==========================================
    // 2. FIREBASE
    // ==========================================

    private FirebaseAuth auth;

    private bool firebaseReady = false;
    private bool isBusy = false;

    private enum AuthScreen
    {
        Welcome,
        Login,
        Register
    }

    private AuthScreen currentScreen = AuthScreen.Welcome;

    // ==========================================
    // 3. AWAKE
    // ==========================================

    void Awake()
    {
        // An mat khau
        if (passwordInput != null)
        {
            passwordInput.contentType =
                TMP_InputField.ContentType.Password;

            passwordInput.ForceLabelUpdate();
        }
    }

    // ==========================================
    // 4. START - KHOI TAO FIREBASE
    // ==========================================

    void Start()
    {
        ShowWelcome();

        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (this == null)
                    return;

                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError(
                        "Firebase Init Error: " + task.Exception
                    );

                    ShowMessage(
                        "Khong the ket noi Firebase."
                    );

                    return;
                }

                if (task.Result != DependencyStatus.Available)
                {
                    firebaseReady = false;

                    Debug.LogError(
                        "Firebase Dependency Error: " +
                        task.Result
                    );

                    ShowMessage(
                        "Firebase chua san sang."
                    );

                    return;
                }

                auth = FirebaseAuth.DefaultInstance;

                // Bat buoc dang nhap khi mo game
                if (requireLoginEveryLaunch &&
                    auth.CurrentUser != null)
                {
                    auth.SignOut();
                }

                firebaseReady = true;

                Debug.Log(
                    "Firebase Authentication READY!"
                );

                ShowMessage("");
            });
    }

    // ==========================================
    // 5. MAN HINH CHAO
    // ==========================================

    public void ShowWelcome()
    {
        if (isBusy)
            return;

        currentScreen = AuthScreen.Welcome;

        ClearInputFields();
        ShowMessage("");

        if (authFormPanel != null)
        {
            authFormPanel.SetActive(false);
        }

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);

            // Dua WelcomePanel ra phia truoc
            welcomePanel.transform.SetAsLastSibling();

            // An nut dang nhap/dang ky cu
            SetFormButtons(false, false);
        }
        else
        {
            // Du phong neu chua gan WelcomePanel
            currentScreen = AuthScreen.Login;

            if (authFormPanel != null)
                authFormPanel.SetActive(true);

            SetFormButtons(true, true);
        }
    }

    // ==========================================
    // 6. NUT DA CO TAI KHOAN
    // ==========================================

    public void ShowLoginForm()
    {
        if (isBusy)
            return;

        currentScreen = AuthScreen.Login;

        // An man hinh hoi tai khoan
        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        // Hien giao dien nhap tai khoan
        if (authFormPanel != null)
            authFormPanel.SetActive(true);

        // Chi hien nut LOGIN
        SetFormButtons(true, false);

        if (formTitleText != null)
            formTitleText.text = "LOGIN";

        ClearPassword();
        ShowMessage("");

        Debug.Log("Open Login Form");
    }

    // ==========================================
    // 7. NUT CHUA CO TAI KHOAN
    // ==========================================

    public void ShowRegisterForm()
    {
        if (isBusy)
            return;

        currentScreen = AuthScreen.Register;

        // An man hinh hoi tai khoan
        if (welcomePanel != null)
            welcomePanel.SetActive(false);

        // Hien giao dien dang ky
        if (authFormPanel != null)
            authFormPanel.SetActive(true);

        // Chi hien nut REGISTER
        SetFormButtons(false, true);

        if (formTitleText != null)
            formTitleText.text = "REGISTER";

        ClearPassword();
        ShowMessage("");

        Debug.Log("Open Register Form");
    }

    // ==========================================
    // 8. KIEM TRA DU LIEU
    // ==========================================

    bool CheckInput(
        bool isRegister,
        out string email,
        out string password)
    {
        email = "";
        password = "";

        if (isBusy)
        {
            ShowMessage("Dang xu ly...");
            return false;
        }

        if (!firebaseReady || auth == null)
        {
            ShowMessage("Firebase chua san sang.");
            return false;
        }

        if (emailInput == null ||
            passwordInput == null)
        {
            Debug.LogError(
                "Chua gan EmailInput hoac PasswordInput!"
            );

            ShowMessage(
                "Chua gan o nhap tai khoan."
            );

            return false;
        }

        email = emailInput.text.Trim();
        password = passwordInput.text;

        // Kiem tra email
        if (string.IsNullOrWhiteSpace(email) ||
            !email.Contains("@") ||
            !email.Contains("."))
        {
            ShowMessage("Email khong hop le.");
            return false;
        }

        // Kiem tra mat khau
        if (string.IsNullOrEmpty(password))
        {
            ShowMessage("Vui long nhap mat khau.");
            return false;
        }

        // Mat khau dang ky phai >= 6 ky tu
        if (isRegister && password.Length < 6)
        {
            ShowMessage(
                "Mat khau phai co it nhat 6 ky tu."
            );

            return false;
        }

        return true;
    }

    // ==========================================
    // 9. DANG KY TAI KHOAN
    // ==========================================

    public void Register()
    {
        if (currentScreen != AuthScreen.Register)
            return;

        if (!CheckInput(
            true,
            out string email,
            out string password))
        {
            return;
        }

        SetBusy(true);

        ShowMessage("Dang tao tai khoan...");

        auth.CreateUserWithEmailAndPasswordAsync(
            email,
            password
        ).ContinueWithOnMainThread(task =>
        {
            if (this == null)
                return;

            SetBusy(false);

            if (task.IsCanceled)
            {
                ShowMessage("Dang ky da bi huy.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError(
                    "REGISTER FAILED: " + task.Exception
                );

                ShowMessage(
                    GetFirebaseErrorMessage(
                        task.Exception, true)
                );

                return;
            }

            Debug.Log("REGISTER SUCCESS!");

            // Firebase tu dang nhap sau dang ky
            // Dang xuat de nguoi choi dang nhap lai
            auth.SignOut();

            // Chuyen sang giao dien LOGIN
            ShowLoginForm();

            // Giu lai email vua dang ky
            if (emailInput != null)
                emailInput.text = email;

            ClearPassword();

            ShowMessage(
                "Dang ky thanh cong! Hay dang nhap."
            );
        });
    }

    // ==========================================
    // 10. DANG NHAP
    // ==========================================

    public void Login()
    {
        if (currentScreen != AuthScreen.Login)
            return;

        if (!CheckInput(
            false,
            out string email,
            out string password))
        {
            return;
        }

        SetBusy(true);

        ShowMessage("Dang dang nhap...");

        auth.SignInWithEmailAndPasswordAsync(
            email,
            password
        ).ContinueWithOnMainThread(task =>
        {
            if (this == null)
                return;

            SetBusy(false);

            if (task.IsCanceled)
            {
                ShowMessage("Dang nhap da bi huy.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError(
                    "LOGIN FAILED: " + task.Exception
                );

                ShowMessage(
                    GetFirebaseErrorMessage(
                        task.Exception, false)
                );

                return;
            }

            if (auth == null ||
                auth.CurrentUser == null)
            {
                ShowMessage(
                    "Khong xac nhan duoc tai khoan."
                );

                return;
            }

            Debug.Log(
                "LOGIN SUCCESS: " +
                auth.CurrentUser.Email
            );

            ClearInputFields();

            // CHUYEN DEN START MENU
            if (Application.CanStreamedLevelBeLoaded(
                startMenuSceneName))
            {
                SceneManager.LoadScene(
                    startMenuSceneName
                );
            }
            else
            {
                Debug.LogError(
                    "StartMenu not found in Build Settings."
                );

                ShowMessage(
                    "Khong tim thay Scene StartMenu."
                );
            }
        });
    }

    // ==========================================
    // 11. NUT BACK
    // ==========================================

    public void Back()
    {
        if (isBusy)
            return;

        // Tro ve man hinh hoi tai khoan
        // Khong duoc bo qua dang nhap
        ShowWelcome();
    }

    // ==========================================
    // 12. XU LY LOI FIREBASE
    // ==========================================

    string GetFirebaseErrorMessage(
        Exception exception,
        bool isRegister)
    {
        FirebaseException firebaseException =
            exception?.GetBaseException()
            as FirebaseException;

        if (firebaseException == null &&
            exception is AggregateException aggregate)
        {
            foreach (Exception inner in
                aggregate.Flatten().InnerExceptions)
            {
                firebaseException =
                    inner.GetBaseException()
                    as FirebaseException;

                if (firebaseException != null)
                    break;
            }
        }

        if (firebaseException == null)
        {
            return isRegister
                ? "Dang ky that bai. Hay thu lai."
                : "Dang nhap that bai. Hay thu lai.";
        }

        AuthError error =
            (AuthError)firebaseException.ErrorCode;

        switch (error)
        {
            case AuthError.EmailAlreadyInUse:
                return "Email nay da duoc dang ky.";

            case AuthError.InvalidEmail:
                return "Email khong hop le.";

            case AuthError.WeakPassword:
                return "Mat khau qua yeu.";

            case AuthError.WrongPassword:
            case AuthError.UserNotFound:
            case AuthError.InvalidCredential:
                return "Email hoac mat khau khong dung.";

            case AuthError.UserDisabled:
                return "Tai khoan da bi vo hieu hoa.";

            case AuthError.NetworkRequestFailed:
                return "Loi mang. Kiem tra Internet.";

            case AuthError.TooManyRequests:
                return "Thu qua nhieu lan. Hay thu lai sau.";

            case AuthError.OperationNotAllowed:
                return "Chua bat Email/Password tren Firebase.";

            default:
                return isRegister
                    ? "Dang ky that bai. Hay thu lai."
                    : "Dang nhap that bai. Hay thu lai.";
        }
    }

    // ==========================================
    // 13. BAT / TAT NUT
    // ==========================================

    void SetFormButtons(
        bool showLogin,
        bool showRegister)
    {
        if (loginButtonObject != null)
        {
            loginButtonObject.SetActive(showLogin);
        }

        if (registerButtonObject != null)
        {
            registerButtonObject.SetActive(showRegister);
        }
    }

    // ==========================================
    // 14. TRANG THAI DANG XU LY
    // ==========================================

    void SetBusy(bool busy)
    {
        isBusy = busy;

        SetButtonInteractable(
            loginButtonObject,
            !busy
        );

        SetButtonInteractable(
            registerButtonObject,
            !busy
        );
    }

    void SetButtonInteractable(
        GameObject obj,
        bool interactable)
    {
        if (obj == null)
            return;

        Button button = obj.GetComponent<Button>();

        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    // ==========================================
    // 15. XOA MAT KHAU
    // ==========================================

    void ClearPassword()
    {
        if (passwordInput != null)
        {
            passwordInput.text = "";
            passwordInput.ForceLabelUpdate();
        }
    }

    // ==========================================
    // 16. XOA THONG TIN
    // ==========================================

    void ClearInputFields()
    {
        if (emailInput != null)
        {
            emailInput.text = "";
            emailInput.ForceLabelUpdate();
        }

        ClearPassword();
    }

    // ==========================================
    // 17. HIEN THONG BAO
    // ==========================================

    void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
}
