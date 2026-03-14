using Microsoft.Web.WebView2.Core;
using ProductivityManager.Models;
using ProductivityManager.Services;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;


namespace ProductivityManager
{



    public partial class MainWindow : Window
    {
        private string _currentStatus = "Active";

        private readonly LoginService _loginService;
        private readonly SignupService _signupService;
        private readonly LogoutService _logoutService;
        private StatusService _statusService;
        private readonly UserSessionContext _sessionContext = new();

        private DispatcherTimer _idleTimer;
        private DateTime _lastActivityTime;
        private const int IdleMinutes = 1;

        public MainWindow()
        {
            InitializeComponent();
            this.MouseMove += (s, e) => RegisterActivity();
            this.KeyDown += (s, e) => RegisterActivity();
            _loginService = new LoginService();
            _signupService = new SignupService();
            _logoutService = new LogoutService();
            Loaded += MainWindow_Loaded;

        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await WebView.EnsureCoreWebView2Async();

            // Load login.html
            string htmlPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "WebUI",
                "login.html");

            WebView.CoreWebView2.Navigate(htmlPath);

            // Listen to messages from HTML
            WebView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
        }

        private void WebView_WebMessageReceived(
            object sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {


            try
            {
               // MessageBox.Show("WebMessageReceived fired");
                var json = e.WebMessageAsJson;

                var message = JsonSerializer.Deserialize<WebMessage>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (message == null)
                    return;


                System.Diagnostics.Debug.WriteLine($"Action received: {message.Action}"); // Change Detected.


                if (message.Action == "login")
                {
                    HandleLogin(message.userName, message.password);
                }


                if (message.Action == "signup")
                {
                    HandleSignup(message.userName, message.password);
                }

                if (message.Action == "logout")
                {
                    HandleLogout();
                }

                if (message.Action == "activity")
                {
                    RegisterActivity();
                }




            }
            catch (Exception ex)
            {
                SendMessageToWeb("error", "Unexpected error occurred");
            }
        }

        private void HandleLogin(string username, string password)
        {
            var result = _loginService.Login(username, password);

            if (!result.IsSuccess)
            {
                SendMessageToWeb("loginFailed", result.ErrorMessage);
                return;
            }

            // Success → send role back to UI
            _sessionContext.UserId = result.User.UserId;
            _sessionContext.SessionId = result.SessionId;
            _sessionContext.Username = result.User.Username;
            _sessionContext.Role = result.User.Role;
            _statusService = new StatusService();

            // 🔥 FIRST STATUS = ACTIVE
            _statusService.ChangeStatus(
                _sessionContext.UserId,
                _sessionContext.SessionId,
                "Active",
                "Login"
            );
            _lastActivityTime = DateTime.Now;
            StartIdleTracking();
            SendMessageToWeb("loginSuccess", result.User.Role);
        }


        /* private void HandleLogout()
         {
             try
             {

                 MessageBox.Show(
     $"UserId: {_sessionContext.UserId}, SessionId: {_sessionContext.SessionId}");

                 // Close current open status first
                 _statusService.CloseStatusOnly(
                     _sessionContext.UserId,
                     _sessionContext.SessionId
                 );
                 _logoutService.Logout(_sessionContext.UserId,
     _sessionContext.SessionId);


                 _idleTimer?.Stop();
                 _idleTimer = null;

                 // Clear in-memory context
                 _sessionContext.Clear();


                 // Navigate back to login
                 WebView.CoreWebView2.Navigate(
                     Path.Combine(
                         AppDomain.CurrentDomain.BaseDirectory,
                         "WebUI",
                         "login.html"));
             }
             catch (Exception ex)
             {
                 SendMessageToWeb("error", "Logout failed");
             }
         }
        */

        // Not  MY VERSION
        private void HandleLogout()
        {
            try
            {
                _statusService.ChangeStatus(
                    _sessionContext.UserId,
                    _sessionContext.SessionId,
                    "Inactive",
                    "Logout");

                _logoutService.Logout(
                    _sessionContext.UserId,
                    _sessionContext.SessionId);

                _idleTimer?.Stop();
                _sessionContext.Clear();

                string loginPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "WebUI",
                    "login.html");

                WebView.CoreWebView2.Navigate(new Uri(loginPath).AbsoluteUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void HandleSignup(string username, string password)
        {
            var result = _signupService.Signup(username, password);

            if (!result.IsSuccess)
            {
                SendMessageToWeb("signupFailed", result.ErrorMessage);
                return;
            }

            SendMessageToWeb("signupSuccess", "Account created successfully, Login to Continue");
            
        }


        private void StartIdleTracking()
        {
            _lastActivityTime = DateTime.Now;

            _idleTimer = new DispatcherTimer();
            _idleTimer.Interval = TimeSpan.FromSeconds(30);
            _idleTimer.Tick += CheckIdle;
            _idleTimer.Start();
        }


        private void CheckIdle(object sender, EventArgs e)
        {



            if (_sessionContext.UserId == 0)
                return;

            var idleMinutes = (DateTime.Now - _lastActivityTime).TotalMinutes;

            if (idleMinutes >= IdleMinutes && _currentStatus != "Idle")
            {
                _statusService.ChangeStatus(
                    _sessionContext.UserId,
                    _sessionContext.SessionId,
                    "Idle",
                    "AutoIdle");

                _currentStatus = "Idle";
            }
        }


        private void RegisterActivity()
        {


            // MessageBox.Show("Activity detected");

            System.Diagnostics.Debug.WriteLine("Activity detected");   





            _lastActivityTime = DateTime.Now;

            if (_sessionContext.UserId == 0)
                return;



            if (_currentStatus == "Idle")
            {
                _statusService.ChangeStatus(
                    _sessionContext.UserId,
                    _sessionContext.SessionId,
                    "Active",
                    "AutoResume");
                _currentStatus = "Active";

            }



        }

        private void SendMessageToWeb(string action, string message)
        {
            var response = new
            {
                action,
                message
            };

            WebView.CoreWebView2.PostWebMessageAsJson(
                JsonSerializer.Serialize(response));
        }


       
    }
}
