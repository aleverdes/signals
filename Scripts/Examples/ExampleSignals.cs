using System;
using UnityEngine;

namespace AleVerDes.Signals.Examples
{
    /// <summary>
    /// Example signal types demonstrating common usage patterns.
    /// </summary>
    public static class ExampleSignals
    {
        /// <summary>
        /// A simple event signal with no data.
        /// </summary>
        public readonly struct GameStartedSignal
        {
            public DateTime Timestamp { get; }
            public GameStartedSignal() => Timestamp = DateTime.Now;
        }

        /// <summary>
        /// A signal carrying player-related data.
        /// </summary>
        public readonly struct PlayerHealthChangedSignal
        {
            public readonly int PlayerId;
            public readonly float Health;
            public readonly float MaxHealth;

            public PlayerHealthChangedSignal(int playerId, float health, float maxHealth)
            {
                PlayerId = playerId;
                Health = health;
                MaxHealth = maxHealth;
            }

            public float HealthPercentage => Health / MaxHealth;
        }

        /// <summary>
        /// A signal for UI interactions.
        /// </summary>
        public readonly struct ButtonClickedSignal
        {
            public readonly string ButtonName;
            public readonly Vector2 ScreenPosition;

            public ButtonClickedSignal(string buttonName, Vector2 screenPosition)
            {
                ButtonName = buttonName;
                ScreenPosition = screenPosition;
            }
        }

        /// <summary>
        /// A signal for inventory management.
        /// </summary>
        public readonly struct ItemCollectedSignal
        {
            public readonly string ItemId;
            public readonly int Quantity;
            public readonly Vector3 WorldPosition;

            public ItemCollectedSignal(string itemId, int quantity, Vector3 worldPosition)
            {
                ItemId = itemId;
                Quantity = quantity;
                WorldPosition = worldPosition;
            }
        }

        /// <summary>
        /// A signal for level/scene transitions.
        /// </summary>
        public readonly struct SceneLoadedSignal
        {
            public readonly string SceneName;
            public readonly LoadSceneMode LoadMode;

            public SceneLoadedSignal(string sceneName, LoadSceneMode loadMode)
            {
                SceneName = sceneName;
                LoadMode = loadMode;
            }
        }

        /// <summary>
        /// A complex signal with multiple data types.
        /// </summary>
        public readonly struct AchievementUnlockedSignal
        {
            public readonly string AchievementId;
            public readonly string Title;
            public readonly string Description;
            public readonly int Points;
            public readonly bool IsRare;

            public AchievementUnlockedSignal(string achievementId, string title, string description, int points, bool isRare)
            {
                AchievementId = achievementId;
                Title = title;
                Description = description;
                Points = points;
                IsRare = isRare;
            }
        }
    }
}
