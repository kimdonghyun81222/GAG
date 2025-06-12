using System;
using System.Threading.Tasks;
using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace _01.Scripts.Networking.Authentication
{
    [Provide]
    public class AuthenticationManager : MonoBehaviour, IDependencyProvider
    {
        [Header("Authentication Settings")]
        [SerializeField] private bool autoSignInOnStart = true;
        [SerializeField] private bool enableAnonymousSignIn = true;
        [SerializeField] private string defaultProfileName = "GrowAGarden_Player";
        
        // Authentication State
        private bool _isInitialized = false;
        private bool _isSignedIn = false;
        private string _playerId = "";
        private string _playerName = "";
        
        // Properties
        public bool IsInitialized => _isInitialized;
        public bool IsSignedIn => _isSignedIn;
        public string PlayerId => _playerId;
        public string PlayerName => _playerName;
        
        // Events
        public event Action OnInitialized;
        public event Action OnSignedIn;
        public event Action OnSignedOut;
        public event Action<string> OnSignInFailed;
        public event Action<string> OnAuthenticationError;

        private async void Start()
        {
            await InitializeServices();
            
            if (autoSignInOnStart)
            {
                await AutoSignIn();
            }
        }
        
        [Provide]
        public AuthenticationManager ProvideAuthenticationManager() => this;

        private async Task InitializeServices()
        {
            try
            {
                // Initialize Unity Services
                var options = new InitializationOptions();
                options.SetProfile(defaultProfileName);
                
                await UnityServices.InitializeAsync(options);
                
                // Setup event handlers
                AuthenticationService.Instance.SignedIn += HandleSignedIn;
                AuthenticationService.Instance.SignedOut += HandleSignedOut;
                AuthenticationService.Instance.SignInFailed += HandleSignInFailed;
                AuthenticationService.Instance.Expired += HandleSessionExpired;
                
                _isInitialized = true;
                OnInitialized?.Invoke();
                
                Debug.Log("UGS Authentication initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize UGS: {ex.Message}");
                OnAuthenticationError?.Invoke($"Initialization failed: {ex.Message}");
            }
        }

        private async Task AutoSignIn()
        {
            if (!_isInitialized) return;
            
            try
            {
                // Try to sign in with cached session token
                if (AuthenticationService.Instance.SessionTokenExists)
                {
                    await SignInAnonymouslyAsync();
                }
                else if (enableAnonymousSignIn)
                {
                    await SignInAnonymouslyAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Auto sign-in failed: {ex.Message}");
            }
        }

        public async Task<bool> SignInAnonymouslyAsync()
        {
            if (!_isInitialized)
            {
                OnSignInFailed?.Invoke("Services not initialized");
                return false;
            }
            
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                return true;
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"Anonymous sign-in failed: {ex.Message}");
                OnSignInFailed?.Invoke($"Anonymous sign-in failed: {ex.Message}");
                return false;
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"Network error during sign-in: {ex.Message}");
                OnSignInFailed?.Invoke($"Network error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SignInWithUsernamePasswordAsync(string username, string password)
        {
            if (!_isInitialized)
            {
                OnSignInFailed?.Invoke("Services not initialized");
                return false;
            }
            
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                return true;
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"Username/password sign-in failed: {ex.Message}");
                OnSignInFailed?.Invoke($"Sign-in failed: {ex.Message}");
                return false;
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"Network error during sign-in: {ex.Message}");
                OnSignInFailed?.Invoke($"Network error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SignUpWithUsernamePasswordAsync(string username, string password)
        {
            if (!_isInitialized)
            {
                OnSignInFailed?.Invoke("Services not initialized");
                return false;
            }
            
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                return true;
            }
            catch (AuthenticationException ex)
            {
                Debug.LogError($"Sign-up failed: {ex.Message}");
                OnSignInFailed?.Invoke($"Sign-up failed: {ex.Message}");
                return false;
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"Network error during sign-up: {ex.Message}");
                OnSignInFailed?.Invoke($"Network error: {ex.Message}");
                return false;
            }
        }

        public void SignOut()
        {
            if (_isSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }
        }

        public async Task<bool> UpdatePlayerNameAsync(string newName)
        {
            if (!_isSignedIn) return false;
            
            try
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
                _playerName = newName;
                Debug.Log($"Player name updated to: {newName}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to update player name: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAccountAsync()
        {
            if (!_isSignedIn) return false;
            
            try
            {
                await AuthenticationService.Instance.DeleteAccountAsync();
                Debug.Log("Account deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete account: {ex.Message}");
                return false;
            }
        }

        // Event Handlers
        private void HandleSignedIn()
        {
            _isSignedIn = true;
            _playerId = AuthenticationService.Instance.PlayerId;
            _playerName = AuthenticationService.Instance.PlayerName;
            
            if (string.IsNullOrEmpty(_playerName))
            {
                _playerName = $"Player_{_playerId.Substring(0, 6)}";
                _ = UpdatePlayerNameAsync(_playerName);
            }
            
            Debug.Log($"Signed in successfully! Player ID: {_playerId}, Name: {_playerName}");
            OnSignedIn?.Invoke();
        }

        private void HandleSignedOut()
        {
            _isSignedIn = false;
            _playerId = "";
            _playerName = "";
            
            Debug.Log("Signed out");
            OnSignedOut?.Invoke();
        }

        private void HandleSignInFailed(RequestFailedException exception)
        {
            Debug.LogError($"Sign-in failed: {exception.Message}");
            OnSignInFailed?.Invoke(exception.Message);
        }

        private async void HandleSessionExpired()
        {
            Debug.LogWarning("Authentication session expired, attempting to refresh...");
            
            try
            {
                await SignInAnonymouslyAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to refresh session: {ex.Message}");
                OnAuthenticationError?.Invoke($"Session expired: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            if (AuthenticationService.Instance != null)
            {
                AuthenticationService.Instance.SignedIn -= HandleSignedIn;
                AuthenticationService.Instance.SignedOut -= HandleSignedOut;
                AuthenticationService.Instance.SignInFailed -= HandleSignInFailed;
                AuthenticationService.Instance.Expired -= HandleSessionExpired;
            }
        }
    }
}