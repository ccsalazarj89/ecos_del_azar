using UnityEngine;
using EcosDelAzar.MiniGames.HighCard;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// Variante de HighCardGame para el boss. Añade la posibilidad de forzar
    /// la victoria de la siguiente ronda pagando oxígeno.
    /// El coste y la lógica de oxígeno los gestiona BossOxygenModifier.
    ///
    /// En el Inspector, asigna este componente como miniGame en MiniGameSession
    /// en lugar de HighCardGame.
    /// </summary>
    public class BossHighCardGame : HighCardGame
    {
        /// <summary>True si hay una victoria forzada pendiente para la próxima ronda.</summary>
        public bool IsForceWinQueued { get; private set; }

        /// <summary>
        /// Marca la siguiente ronda como victoria garantizada.
        /// Solo llamar desde BossOxygenModifier.TryActivateForceWin().
        /// </summary>
        public void QueueForceWin()
        {
            IsForceWinQueued = true;
            Debug.Log("[BossHighCardGame] Victoria forzada en cola — la próxima ronda será Win.");
        }

        protected override RoundResult EvaluateResult()
        {
            if (!IsForceWinQueued)
                return base.EvaluateResult();

            IsForceWinQueued = false;
            Debug.Log($"[BossHighCardGame] Victoria forzada aplicada. Carta: {PlayerCard}");

            return new RoundResult(
                outcome: RoundOutcome.Win,
                playerValue: PlayerCard.Value,
                opponentValue: 0,
                description: $"[Victoria Forzada] {PlayerCard}"
            );
        }
    }
}
