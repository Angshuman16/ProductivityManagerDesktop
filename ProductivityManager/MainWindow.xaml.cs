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

        public MainWindow()
        {
            InitializeComponent();
            _loginService = new LoginService();
            _signupService = new SignupService();
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
                var json = e.WebMessageAsJson;

                var message = JsonSerializer.Deserialize<WebMessage>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (message == null)
                    return;

                if (message.Action == "login")
                {
                    HandleLogin(message.userName, message.password);
                }


                if (message.Action == "signup")
                {
                    HandleSignup(message.userName, message.password);
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
            SendMessageToWeb("loginSuccess", result.User.Role);
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
