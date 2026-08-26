using System;

namespace EcosDelAzar.UI
{
    public interface IInteractable
    {
        event Action<bool> OnPlayerRangeChanged;
        event Action OnInteractionStarted;
        event Action OnInteractionBlocked;

        /// <summary>Text the world label should show instead of its default hint, or null.</summary>
        string HintOverride { get; }
    }
}
