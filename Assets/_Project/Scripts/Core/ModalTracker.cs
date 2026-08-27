using System;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Counts the world panels currently open (elevator, O2 machine, minibar,
    /// dialogue). Those panels consume Esc to close themselves; the pause menu
    /// only opens when nothing else is listening.
    /// </summary>
    public static class ModalTracker
    {
        public static int OpenCount { get; private set; }
        public static bool IsAnyOpen => OpenCount > 0;

        public static event Action<bool> OnAnyOpenChanged;

        public static void Opened()
        {
            OpenCount++;
            if (OpenCount == 1) OnAnyOpenChanged?.Invoke(true);
        }

        public static void Closed()
        {
            if (OpenCount == 0) return;
            OpenCount--;
            if (OpenCount == 0) OnAnyOpenChanged?.Invoke(false);
        }

        /// <summary>Scene loads destroy panels without closing them; start clean.</summary>
        public static void Reset()
        {
            bool wasOpen = OpenCount > 0;
            OpenCount = 0;
            if (wasOpen) OnAnyOpenChanged?.Invoke(false);
        }
    }
}
