using System;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// One-line messages for the bottom of the HUD: why an interaction was
    /// refused, what the player should do next. World labels stay short (they
    /// float in 3D and cannot hold a sentence); the explanation goes here.
    /// </summary>
    public static class UINotice
    {
        public static event Action<string> OnMessage;

        public static void Show(string message)
        {
            if (!string.IsNullOrEmpty(message)) OnMessage?.Invoke(message);
        }
    }
}
