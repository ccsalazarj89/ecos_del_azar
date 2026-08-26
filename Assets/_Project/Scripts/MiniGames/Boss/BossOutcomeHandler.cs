using System.Collections;
using UnityEngine;
using EcosDelAzar.Core;
using EcosDelAzar.MiniGames.Betting;

namespace EcosDelAzar.MiniGames.Boss
{
    /// <summary>
    /// The boss table is the end of the run: bankrupting the boss (by rounds or
    /// by sudden death) wins the game, going broke against him loses it. Both
    /// paths end through GameManager.EndRun, which wipes the run.
    /// </summary>
    public class BossOutcomeHandler : MonoBehaviour
    {
        [SerializeField] BettingSystem bettingSystem;
        [SerializeField] SuddenDeathRound suddenDeath;
        [Tooltip("Seconds the result stays on screen before the end-of-run panel.")]
        [SerializeField] float endDelay = 2.5f;

        [Header("Texts")]
        [SerializeField] string victoryTitle = "ACCESO CONCEDIDO";
        [SerializeField, TextArea] string victorySubtitle =
            "Has conseguido el acceso a la planta preferencial. Nos vemos en el siguiente capítulo.";
        [SerializeField] string defeatTitle = "LA CASA SIEMPRE GANA";
        [SerializeField, TextArea] string defeatSubtitle =
            "El director se queda con tus fichas, tus monedas y tu aire. Vuelve a empezar desde el lobby.";

        bool ended;

        void OnEnable()
        {
            if (bettingSystem != null) bettingSystem.OnGameOver += OnGameOver;
            if (suddenDeath != null) suddenDeath.OnSuddenDeathComplete += OnGameOver;
        }

        void OnDisable()
        {
            if (bettingSystem != null) bettingSystem.OnGameOver -= OnGameOver;
            if (suddenDeath != null) suddenDeath.OnSuddenDeathComplete -= OnGameOver;
        }

        void OnGameOver(bool playerWon)
        {
            if (ended) return;
            ended = true;
            StartCoroutine(EndAfterDelay(playerWon));
        }

        IEnumerator EndAfterDelay(bool playerWon)
        {
            yield return new WaitForSeconds(endDelay);

            if (playerWon)
                GameManager.Instance?.EndRun(victoryTitle, victorySubtitle);
            else
                GameManager.Instance?.EndRun(defeatTitle, defeatSubtitle);
        }
    }
}
