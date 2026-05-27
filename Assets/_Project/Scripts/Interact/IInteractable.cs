using System;

namespace EcosDelAzar.UI
{
    public interface IInteractable
    {
        event Action<bool> OnPlayerRangeChanged;
        event Action OnInteractionStarted;
        event Action OnInteractionBlocked;
    }
}
