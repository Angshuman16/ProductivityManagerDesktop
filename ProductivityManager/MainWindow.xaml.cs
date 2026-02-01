using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using ProductivityManager.Models;
using ProductivityManager.Services;

namespace ProductivityManager
{
    public partial class MainWindow : Window
    {
        private readonly LoginService _loginService;
        private readonly SignupService _signupService;
        private readonly LogoutService _logoutService;
        private readonly UserSessionContext _sessionContext = new();

        public MainWindow()
        {
            InitializeComponent();
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
                MessageBox.Show("WebMessageReceived fired");
                var json = e.WebMessageAsJson;

                var message = JsonSerializer.Deserialize<WebMessage>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (message == null)
                    return;


                MessageBox.Show(message.Action);

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
            SendMessageToWeb("loginSuccess", result.User.Role);
        }


        private void HandleLogout()
        {
            try
            {

                MessageBox.Show(
    $"UserId: {_sessionContext.UserId}, SessionId: {_sessionContext.SessionId}");

                _logoutService.Logout(_sessionContext.UserId,
    _sessionContext.SessionId);

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
