using GrowAGarden.Core._01.Scripts.Core.Dependencies;
using GrowAGarden.Core._01.Scripts.Core.Entities;
using UnityEngine;

namespace GrowAGarden.Player._01.Scripts.Player
{
    [Provide]
    public class PlayerEntity : Entity, IDependencyProvider
    {
        [Header("Player Settings")]
        [SerializeField] private string playerName = "Player";
        [SerializeField] private int playerLevel = 1;
        [SerializeField] private float playerExperience = 0f;
        [SerializeField] private Vector3 spawnPosition = Vector3.zero;
        
        // Player State
        private bool _isAlive = true;
        private bool _canMove = true;
        private bool _canInteract = true;
        
        // Properties
        public string PlayerName => playerName;
        public int PlayerLevel => playerLevel;
        public float PlayerExperience => playerExperience;
        public bool IsAlive => _isAlive;
        public bool CanMove => _canMove;
        public bool CanInteract => _canInteract;
        
        // Events
        public System.Action<int> OnLevelChanged;
        public System.Action<float> OnExperienceChanged;
        public System.Action<bool> OnMovementStateChanged;

        protected override void Awake()
        {
            // Set player-specific entity settings
            SetEntityName(playerName);
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            
            // Set initial position
            if (spawnPosition != Vector3.zero)
            {
                transform.position = spawnPosition;
            }
        }
        
        [Provide]
        public PlayerEntity ProvidePlayerEntity() => this;

        // Player-specific methods
        public void AddExperience(float amount)
        {
            playerExperience += amount;
            OnExperienceChanged?.Invoke(playerExperience);
            
            // Check for level up
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            int newLevel = CalculateLevel(playerExperience);
            if (newLevel > playerLevel)
            {
                playerLevel = newLevel;
                OnLevelChanged?.Invoke(playerLevel);
                Debug.Log($"Player leveled up to level {playerLevel}!");
            }
        }

        private int CalculateLevel(float experience)
        {
            // Simple experience formula: level = sqrt(experience / 100) + 1
            return Mathf.FloorToInt(Mathf.Sqrt(experience / 100f)) + 1;
        }

        public void SetMovementEnabled(bool enabled)
        {
            _canMove = enabled;
            OnMovementStateChanged?.Invoke(_canMove);
        }

        public void SetInteractionEnabled(bool enabled)
        {
            _canInteract = enabled;
        }

        public void SetPlayerName(string newName)
        {
            playerName = newName;
            SetEntityName(newName);
        }

        public void Respawn()
        {
            _isAlive = true;
            transform.position = spawnPosition;
            SetMovementEnabled(true);
            SetInteractionEnabled(true);
        }

        public void Die()
        {
            _isAlive = false;
            SetMovementEnabled(false);
            SetInteractionEnabled(false);
            // Add death effects here
        }
    }
}