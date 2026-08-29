using UnityEngine;
using UnityEngine.UIElements;
using EcosDelAzar.MiniGames;
using EcosDelAzar.MiniGames.Betting;

namespace EcosDelAzar.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class BettingUI : MonoBehaviour
    {
        [SerializeField] MiniGameSession session;
        [SerializeField] int betStep = 10;

        // Phases
        VisualElement betPhase;
        VisualElement playingPhase;
        VisualElement resultPhase;
        VisualElement proposalPhase;
        VisualElement gameOverPhase;

        // Coins
        Label playerCoinsLabel;
        Label opponentCoinsLabel;
        Label opponentNameLabel;

        // Bet phase
        Label betAmountLabel;
        Button btnBetDown;
        Button btnBetUp;
        Button btnBetMin;
        Button btnBetHalf;
        Button btnBetMax;
        Button btnPlay;
        Button btnStandUp;
        Label tableMinNote;

        // Playing phase
        Label currentBetDisplay;
        Label playingStatus;

        // Result phase
        Label resultText;
        Label resultAmount;

        // Proposal phase
        Label proposalText;
        Button btnMatch;
        Button btnDouble;
        Button btnFold;
        Button btnPushLuck;
        Button btnShield;
        Label stanceHint;

        // Game over phase
        Label gameOverText;
        Label gameOverSubtext;
        Button btnLeave;

        int currentBet;

        // The NPC proposal and game-over screens are deferred until the round is
        // truly ready, instead of showing them the instant the round resolves.
        bool pendingProposal;
        int pendingProposalBet;
        bool pendingGameOver;
        bool pendingGameOverWon;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            if (doc?.rootVisualElement == null) return;

            var root = doc.rootVisualElement;

            betPhase = root.Q("bet-phase");
            playingPhase = root.Q("playing-phase");
            resultPhase = root.Q("result-phase");
            proposalPhase = root.Q("proposal-phase");
            gameOverPhase = root.Q("gameover-phase");

            playerCoinsLabel = root.Q<Label>("player-coins");
            opponentCoinsLabel = root.Q<Label>("opponent-coins");
            opponentNameLabel = root.Q<Label>("opponent-name");

            betAmountLabel = root.Q<Label>("bet-amount");
            btnBetDown = root.Q<Button>("btn-bet-down");
            btnBetUp = root.Q<Button>("btn-bet-up");
            btnBetMin = root.Q<Button>("btn-bet-min");
            btnBetHalf = root.Q<Button>("btn-bet-half");
            btnBetMax = root.Q<Button>("btn-bet-max");
            btnPlay = root.Q<Button>("btn-play");
            btnStandUp = root.Q<Button>("btn-stand-up");
            tableMinNote = root.Q<Label>("table-min-note");

            currentBetDisplay = root.Q<Label>("current-bet-display");
            playingStatus = root.Q<Label>("playing-status");

            resultText = root.Q<Label>("result-text");
            resultAmount = root.Q<Label>("result-amount");

            proposalText = root.Q<Label>("proposal-text");
            btnMatch = root.Q<Button>("btn-match");
            btnDouble = root.Q<Button>("btn-double");
            btnFold = root.Q<Button>("btn-fold");
            btnPushLuck = root.Q<Button>("btn-pushluck");
            btnShield = root.Q<Button>("btn-shield");
            stanceHint = root.Q<Label>("stance-hint");

            gameOverText = root.Q<Label>("gameover-text");
            gameOverSubtext = root.Q<Label>("gameover-subtext");
            btnLeave = root.Q<Button>("btn-leave");

            BindButtons();
            BindEvents();
            SetPhase(null);
        }

        void OnDisable()
        {
            UnbindButtons();
            UnbindEvents();
        }

        // ─── Binding ───

        void BindButtons()
        {
            btnBetDown?.RegisterCallback<ClickEvent>(OnBetDown);
            btnBetUp?.RegisterCallback<ClickEvent>(OnBetUp);
            btnBetMin?.RegisterCallback<ClickEvent>(OnBetMin);
            btnBetHalf?.RegisterCallback<ClickEvent>(OnBetHalf);
            btnBetMax?.RegisterCallback<ClickEvent>(OnBetMax);
            btnPlay?.RegisterCallback<ClickEvent>(OnPlay);
            btnStandUp?.RegisterCallback<ClickEvent>(OnStandUp);
            btnMatch?.RegisterCallback<ClickEvent>(OnMatch);
            btnDouble?.RegisterCallback<ClickEvent>(OnDouble);
            btnFold?.RegisterCallback<ClickEvent>(OnFoldClicked);
            btnPushLuck?.RegisterCallback<ClickEvent>(OnPushLuck);
            btnShield?.RegisterCallback<ClickEvent>(OnShield);
            btnLeave?.RegisterCallback<ClickEvent>(OnLeave);
        }

        void UnbindButtons()
        {
            btnBetDown?.UnregisterCallback<ClickEvent>(OnBetDown);
            btnBetUp?.UnregisterCallback<ClickEvent>(OnBetUp);
            btnBetMin?.UnregisterCallback<ClickEvent>(OnBetMin);
            btnBetHalf?.UnregisterCallback<ClickEvent>(OnBetHalf);
            btnBetMax?.UnregisterCallback<ClickEvent>(OnBetMax);
            btnPlay?.UnregisterCallback<ClickEvent>(OnPlay);
            btnStandUp?.UnregisterCallback<ClickEvent>(OnStandUp);
            btnMatch?.UnregisterCallback<ClickEvent>(OnMatch);
            btnDouble?.UnregisterCallback<ClickEvent>(OnDouble);
            btnFold?.UnregisterCallback<ClickEvent>(OnFoldClicked);
            btnPushLuck?.UnregisterCallback<ClickEvent>(OnPushLuck);
            btnShield?.UnregisterCallback<ClickEvent>(OnShield);
            btnLeave?.UnregisterCallback<ClickEvent>(OnLeave);
        }

        void BindEvents()
        {
            if (session == null) return;

            session.OnSessionStarted += OnSessionReady;

            if (session.Game != null)
            {
                session.Game.OnRoundStarted += ShowPlayingPhase;
                session.Game.OnReadyForNextRound += HandleReadyForNextRound;
                session.Game.OnStatusTextChanged += RefreshPlayingStatus;
            }

            if (session.Betting != null)
            {
                session.Betting.OnCoinsUpdated += RefreshCoins;
                session.Betting.OnRoundSettled += ShowResultPhase;
                session.Betting.OnNpcProposal += HandleNpcProposal;
                session.Betting.OnGameOver += HandleGameOver;
            }
        }

        void UnbindEvents()
        {
            if (session == null) return;

            session.OnSessionStarted -= OnSessionReady;

            if (session.Game != null)
            {
                session.Game.OnRoundStarted -= ShowPlayingPhase;
                session.Game.OnReadyForNextRound -= HandleReadyForNextRound;
                session.Game.OnStatusTextChanged -= RefreshPlayingStatus;
            }

            if (session.Betting != null)
            {
                session.Betting.OnCoinsUpdated -= RefreshCoins;
                session.Betting.OnRoundSettled -= ShowResultPhase;
                session.Betting.OnNpcProposal -= HandleNpcProposal;
                session.Betting.OnGameOver -= HandleGameOver;
            }
        }

        void OnSessionReady()
        {
            currentBet = session.Betting.MinimumBet;
            if (session.Betting.HasStandingProposal) ShowProposalPhase(session.Betting.NpcProposedBet);
            else ShowBetPhase();
        }

        // ─── Phase Display ───

        void SetPhase(VisualElement active)
        {
            SetVisible(betPhase, active == betPhase);
            SetVisible(playingPhase, active == playingPhase);
            SetVisible(resultPhase, active == resultPhase);
            SetVisible(proposalPhase, active == proposalPhase);
            SetVisible(gameOverPhase, active == gameOverPhase);
        }

        void SetVisible(VisualElement el, bool visible)
        {
            if (el != null)
                el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void ShowBetPhase()
        {
            RefreshCoins();
            ClampBet();
            RefreshBetDisplay();
            SetPhase(betPhase);

            // A dealer who raised before remembers it: the table minimum is his last proposal.
            if (tableMinNote != null)
            {
                bool remembered = session.Betting.TableMinimumActive;
                tableMinNote.text = remembered ? $"El crupier no baja de {session.Betting.MinimumBet}" : string.Empty;
                tableMinNote.style.display = remembered ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        void ShowPlayingPhase()
        {
            pendingProposal = false;
            pendingGameOver = false;
            RefreshCoins();
            if (currentBetDisplay != null)
                currentBetDisplay.text = $"Apuesta: {currentBet}";
            RefreshPlayingStatus();
            SetPhase(playingPhase);
        }

        void RefreshPlayingStatus()
        {
            if (playingStatus != null && session?.Game != null)
                playingStatus.text = session.Game.PlayingStatusText;
        }

        // Driven by BettingSystem.OnRoundSettled, which fires after coins are
        // applied — so outcome and winnings are always the current round's.
        void ShowResultPhase(RoundOutcome outcome, int winnings)
        {
            RefreshCoins();
            SetPhase(resultPhase);

            if (resultText != null)
            {
                resultText.text = outcome switch
                {
                    RoundOutcome.Win => "¡GANASTE!",
                    RoundOutcome.Lose => "PERDISTE",
                    _ => "EMPATE"
                };

                resultText.RemoveFromClassList("result--win");
                resultText.RemoveFromClassList("result--lose");
                resultText.RemoveFromClassList("result--draw");
                resultText.AddToClassList(outcome switch
                {
                    RoundOutcome.Win => "result--win",
                    RoundOutcome.Lose => "result--lose",
                    _ => "result--draw"
                });
            }

            if (resultAmount != null)
            {
                bool insured = session.Betting.LastLossInsured;

                // The stance note only makes sense where the stance changed the payout:
                // push-luck moves win and loss, the shield only softens a loss.
                string stance = session.Betting.LastStance switch
                {
                    RoundStance.PushLuck when outcome != RoundOutcome.Draw => "  (forzaste la suerte)",
                    RoundStance.Shield when outcome == RoundOutcome.Lose => "  (blindado)",
                    _ => string.Empty
                };
                resultAmount.text = insured ? "SEGURO: apuesta devuelta"
                    : (winnings > 0 ? $"+{winnings}" : winnings < 0 ? $"{winnings}" : "±0") + stance;
            }
        }

        // Resolution-time events only record what to show next; the actual
        // transition waits for OnReadyForNextRound (result has been on screen
        // and the game is back in WaitingForBet, so the next click is accepted).
        void HandleNpcProposal(int npcBet)
        {
            pendingProposalBet = npcBet;
            pendingProposal = true;
        }

        void HandleGameOver(bool playerWon)
        {
            pendingGameOverWon = playerWon;
            pendingGameOver = true;

            // Mid-round this waits for OnReadyForNextRound; but a game-over fired
            // outside a round (e.g. broke at session start) has no such follow-up,
            // so show it immediately.
            if (session?.Game == null || session.Game.State != MiniGameState.Resolved)
            {
                pendingGameOver = false;
                ShowGameOver(playerWon);
            }
        }

        void HandleReadyForNextRound()
        {
            if (pendingGameOver)
            {
                pendingGameOver = false;
                ShowGameOver(pendingGameOverWon);
            }
            else if (pendingProposal)
            {
                pendingProposal = false;
                ShowProposalPhase(pendingProposalBet);
            }
        }

        void ShowProposalPhase(int npcBet)
        {
            RefreshCoins();
            SetPhase(proposalPhase);

            if (proposalText != null)
                proposalText.text = $"El rival propone: {npcBet} monedas";

            if (btnDouble != null)
            {
                // Say the number: "SUBIR" alone hid how much the player was committing.
                btnDouble.SetEnabled(session.Betting.MaxBet >= npcBet * 2);
                btnDouble.text = $"SUBIR A {npcBet * 2}";
            }

            bool canMatch = session.Betting.MaxBet >= npcBet;
            if (btnMatch != null) btnMatch.text = $"PLANTARSE ({npcBet})";
            if (btnMatch != null) btnMatch.SetEnabled(canMatch);
            if (btnPushLuck != null) btnPushLuck.SetEnabled(canMatch);
            if (btnShield != null) btnShield.SetEnabled(canMatch);

            if (stanceHint != null)
            {
                var b = session.Betting;
                stanceHint.text = $"Forzar la suerte: ±{Mathf.RoundToInt(b.PushLuckEdge * 100)}% extra  ·  Blindarse: recuperas el {Mathf.RoundToInt(b.ShieldMitigation * 100)}% si pierdes, cuesta {Mathf.RoundToInt(b.ShieldOxygenCost * 100)}% de O2";
            }
        }

        void ShowGameOver(bool playerWon)
        {
            RefreshCoins();
            SetPhase(gameOverPhase);

            if (gameOverText != null)
                gameOverText.text = playerWon ? "¡VICTORIA!" : "DERROTA";

            if (gameOverSubtext != null)
            {
                gameOverSubtext.text = playerWon
                    ? "El rival se quedó sin monedas"
                    : "No tienes más dinero para jugar";
            }

            if (gameOverText != null)
            {
                gameOverText.RemoveFromClassList("result--win");
                gameOverText.RemoveFromClassList("result--lose");
                gameOverText.AddToClassList(playerWon ? "result--win" : "result--lose");
            }
        }

        // ─── Bet Input ───

        void OnBetDown(ClickEvent _)
        {
            currentBet = Mathf.Max(session.Betting.MinimumBet, currentBet - betStep);
            RefreshBetDisplay();
        }

        void OnBetUp(ClickEvent _)
        {
            currentBet = Mathf.Min(session.Betting.MaxBet, currentBet + betStep);
            RefreshBetDisplay();
        }

        void OnBetMin(ClickEvent _)
        {
            currentBet = session.Betting.MinimumBet;
            RefreshBetDisplay();
        }

        void OnBetHalf(ClickEvent _)
        {
            currentBet = Mathf.Max(session.Betting.MinimumBet, session.Betting.MaxBet / 2);
            RefreshBetDisplay();
        }

        void OnBetMax(ClickEvent _)
        {
            currentBet = session.Betting.MaxBet;
            RefreshBetDisplay();
        }

        void ClampBet()
        {
            if (session?.Betting == null) return;
            currentBet = Mathf.Clamp(currentBet, session.Betting.MinimumBet, session.Betting.MaxBet);
        }

        void RefreshBetDisplay()
        {
            if (betAmountLabel != null)
                betAmountLabel.text = currentBet.ToString();

            if (session?.Betting == null) return;
            if (btnBetDown != null)
                btnBetDown.SetEnabled(currentBet > session.Betting.MinimumBet);
            if (btnBetUp != null)
                btnBetUp.SetEnabled(currentBet < session.Betting.MaxBet);
        }

        // ─── Actions ───

        void OnPlay(ClickEvent _)
        {
            if (session == null) return;
            session.StartRound(currentBet);
        }

        void OnMatch(ClickEvent _)
        {
            if (session == null) return;
            currentBet = session.Betting.NpcProposedBet;
            session.RespondToProposal(BetResponse.Match);
        }

        void OnDouble(ClickEvent _)
        {
            if (session == null) return;
            currentBet = session.Betting.NpcProposedBet * 2;
            session.RespondToProposal(BetResponse.Double);
        }

        // Leaving before a round costs nothing: no bet is on the table yet.
        void OnStandUp(ClickEvent _)
        {
            if (session == null) return;
            session.Leave();
        }

        void OnPushLuck(ClickEvent _)
        {
            if (session == null) return;
            currentBet = session.Betting.NpcProposedBet;
            session.RespondToProposal(BetResponse.PushLuck);
        }

        void OnShield(ClickEvent _)
        {
            if (session == null) return;
            currentBet = session.Betting.NpcProposedBet;
            session.RespondToProposal(BetResponse.Shield);
        }

        void OnFoldClicked(ClickEvent _)
        {
            if (session == null) return;
            session.RespondToProposal(BetResponse.Fold);
        }

        void OnLeave(ClickEvent _)
        {
            if (session == null) return;
            session.Leave();
        }

        // ─── Refresh ───

        void RefreshCoins()
        {
            if (session?.Betting == null) return;
            if (playerCoinsLabel != null) playerCoinsLabel.text = session.Betting.PlayerCoins.ToString();
            if (opponentNameLabel != null && session.Betting.Opponent != null) opponentNameLabel.text = session.Betting.Opponent.DisplayName.ToUpperInvariant();
            if (opponentCoinsLabel != null) opponentCoinsLabel.text = session.Betting.OpponentCoins.ToString();
        }
    }
}
