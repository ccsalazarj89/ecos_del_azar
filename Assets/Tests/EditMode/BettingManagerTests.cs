using NUnit.Framework;
using UnityEngine;
using EcosDelAzar.Betting;
using System.Reflection;

/// <summary>
/// Tests unitarios del BettingManager.
/// Cubre los 23 casos acordados incluyendo edge cases.
/// Ejecutar desde: Window > General > Test Runner > EditMode
/// </summary>
public class BettingManagerTests
{
    private GameObject    _go;
    private BettingManager _bm;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject("BettingManagerTest");
        _bm = _go.AddComponent<BettingManager>();
        // En Edit Mode, Awake NO se llama automáticamente al usar AddComponent.
        // Lo forzamos vía reflection para garantizar la inicialización correcta.
        typeof(BettingManager)
            .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_bm, null);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_go);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Fuerza valores de fichas vía reflection para simular estados intermedios.</summary>
    private void SetChips(int player, int npc)
    {
        typeof(BettingManager).GetProperty("PlayerChips")
            .SetValue(_bm, player);
        typeof(BettingManager).GetProperty("NpcChips")
            .SetValue(_bm, npc);
    }

    /// <summary>Fuerza _lastBet vía reflection.</summary>
    private void SetLastBet(int value)
    {
        typeof(BettingManager)
            .GetField("_lastBet", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_bm, value);
    }

    // ── T01-T02: Inicialización ───────────────────────────────────────────────

    [Test]
    public void T01_FichasInicialesCorrectas()
    {
        Assert.AreEqual(1000, _bm.PlayerChips, "PlayerChips debe empezar en startingChips");
        Assert.AreEqual(1000, _bm.NpcChips,    "NpcChips debe empezar en startingChips");
    }

    [Test]
    public void T02_LastBetInicialEsMinimumBet()
    {
        int lastBet = (int)typeof(BettingManager)
            .GetField("_lastBet", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_bm);
        Assert.AreEqual(_bm.minimumBet, lastBet, "_lastBet inicial debe ser minimumBet");
    }

    // ── T03-T04: Igualar ──────────────────────────────────────────────────────

    [Test]
    public void T03_IgualarApuestaLaMismaQueElNpc()
    {
        _bm.PrepareDuel();
        int npcBet = _bm.NpcBet;
        _bm.ProcessPlayerAction(BetAction.Equal);
        Assert.AreEqual(Mathf.Min(npcBet, _bm.PlayerChips), _bm.CurrentBet,
            "Equal debe apostar lo mismo que el NPC (o todo si no alcanza)");
    }

    [Test]
    public void T04_IgualarDespuesDeRondaApuestaLaNuevaAperturadelNpc()
    {
        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Double);
        _bm.ResolveResult(RoundOutcome.Draw);

        // Segunda ronda: el NPC decide una nueva apertura
        _bm.PrepareDuel();
        int npcBet = _bm.NpcBet;
        _bm.ProcessPlayerAction(BetAction.Equal);
        Assert.AreEqual(Mathf.Min(npcBet, _bm.PlayerChips), _bm.CurrentBet,
            "Equal debe igualar la apertura del NPC de la ronda actual");
    }

    // ── T05-T06: Doblar ───────────────────────────────────────────────────────

    [Test]
    public void T05_DoblarPrimeraVezEsNpcBetPorDos()
    {
        _bm.PrepareDuel();
        int npcBet = _bm.NpcBet;
        _bm.ProcessPlayerAction(BetAction.Double);
        Assert.AreEqual(Mathf.Min(npcBet * 2, _bm.PlayerChips), _bm.CurrentBet,
            "Double debe ser NpcBet * 2 (o todas las fichas si no alcanza)");
    }

    [Test]
    public void T06_DoblarConFichasInsuficientesApuestaTodasLasFichas()
    {
        SetChips(15, 1000); // NpcBet >= 10 → NpcBet*2 >= 20 > 15 → cap a 15
        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Double);
        Assert.AreEqual(15, _bm.CurrentBet, "Double con fichas insuficientes debe apostar todas las fichas");
    }

    // ── T07: AllIn ────────────────────────────────────────────────────────────

    [Test]
    public void T07_AllInApuestaExactamentePlayerChips()
    {
        int fichasAntes = _bm.PlayerChips;
        _bm.ProcessPlayerAction(BetAction.AllIn);
        Assert.AreEqual(fichasAntes, _bm.CurrentBet, "AllIn debe apostar exactamente PlayerChips");
    }

    // ── T08-T09: Apuesta efectiva ─────────────────────────────────────────────

    [Test]
    public void T08_EffectiveBetEsMinimoCuandoNpcApuestaMenos()
    {
        SetChips(1000, 5); // NPC solo tiene 5 fichas → apuesta como máximo 5
        _bm.ProcessPlayerAction(BetAction.AllIn);
        Assert.AreEqual(Mathf.Min(_bm.CurrentBet, _bm.NpcBet), _bm.EffectiveBet,
            "EffectiveBet debe ser min(playerBet, npcBet)");
    }

    [Test]
    public void T09_EffectiveBetEsMinimoCuandoJugadorApuestaMenos()
    {
        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        Assert.AreEqual(Mathf.Min(_bm.CurrentBet, _bm.NpcBet), _bm.EffectiveBet,
            "EffectiveBet debe ser min(playerBet, npcBet)");
    }

    // ── T10-T13: ResolveResult ────────────────────────────────────────────────

    [Test]
    public void T10_VictoriaTransfiereFichasCorrectamente()
    {
        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        int bet       = _bm.EffectiveBet;
        int playerPre = _bm.PlayerChips;
        int npcPre    = _bm.NpcChips;

        _bm.ResolveResult(RoundOutcome.Win);

        Assert.AreEqual(playerPre + bet, _bm.PlayerChips, "Jugador debe ganar EffectiveBet fichas");
        Assert.AreEqual(npcPre    - bet, _bm.NpcChips,    "NPC debe perder EffectiveBet fichas");
    }

    [Test]
    public void T11_DerrotaTransfiereFichasCorrectamente()
    {
        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        int bet       = _bm.EffectiveBet;
        int playerPre = _bm.PlayerChips;
        int npcPre    = _bm.NpcChips;

        _bm.ResolveResult(RoundOutcome.Lose);

        Assert.AreEqual(playerPre - bet, _bm.PlayerChips, "Jugador debe perder EffectiveBet fichas");
        Assert.AreEqual(npcPre    + bet, _bm.NpcChips,    "NPC debe ganar EffectiveBet fichas");
    }

    [Test]
    public void T12_EmpateNoMueveFichas()
    {
        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        int playerPre = _bm.PlayerChips;
        int npcPre    = _bm.NpcChips;

        _bm.ResolveResult(RoundOutcome.Draw);

        Assert.AreEqual(playerPre, _bm.PlayerChips, "Empate no debe cambiar fichas del jugador");
        Assert.AreEqual(npcPre,    _bm.NpcChips,    "Empate no debe cambiar fichas del NPC");
    }

    [Test]
    public void T13_SumaTotalFichasSeConservaEnVictoriaYDerrota()
    {
        int total = _bm.PlayerChips + _bm.NpcChips;

        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        _bm.ResolveResult(RoundOutcome.Win);
        Assert.AreEqual(total, _bm.PlayerChips + _bm.NpcChips, "La suma total no debe cambiar tras victoria");

        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        _bm.ResolveResult(RoundOutcome.Lose);
        Assert.AreEqual(total, _bm.PlayerChips + _bm.NpcChips, "La suma total no debe cambiar tras derrota");
    }

    // ── T14-T16: Retirarse ────────────────────────────────────────────────────

    [Test]
    public void T14_RetirarseTransfierePenalidadMinima()
    {
        int playerPre = _bm.PlayerChips;
        int npcPre    = _bm.NpcChips;
        int penalty   = Mathf.Min(_bm.minimumBet, playerPre);

        _bm.ProcessPlayerAction(BetAction.FoldRound);

        Assert.AreEqual(playerPre - penalty, _bm.PlayerChips, "Jugador pierde minimumBet al retirarse");
        Assert.AreEqual(npcPre    + penalty, _bm.NpcChips,    "NPC gana minimumBet cuando jugador se retira");
    }

    [Test]
    public void T15_RetirarseConMenosFichasQueMinimumBet()
    {
        SetChips(5, 1000); // 5 < minimumBet(10) → pierde todo lo que tiene
        _bm.ProcessPlayerAction(BetAction.FoldRound);

        Assert.AreEqual(0,    _bm.PlayerChips, "Jugador debe perder todas sus fichas");
        Assert.AreEqual(1005, _bm.NpcChips,    "NPC debe recibir las fichas que tenía el jugador");
    }

    [Test]
    public void T16_RetirarseConExactamenteMinimumBetDisparaGameOver()
    {
        SetChips(_bm.minimumBet, 1000);
        bool gameOverFired = false;
        _bm.OnGameOver += () => gameOverFired = true;

        _bm.ProcessPlayerAction(BetAction.FoldRound);

        Assert.AreEqual(0, _bm.PlayerChips, "PlayerChips debe llegar a 0");
        Assert.IsTrue(gameOverFired, "OnGameOver debe dispararse");
    }

    // ── T17: Abandonar ────────────────────────────────────────────────────────

    [Test]
    public void T17_AbandonarNoMueveFichas()
    {
        int playerPre = _bm.PlayerChips;
        int npcPre    = _bm.NpcChips;
        bool abandonFired = false;
        _bm.OnGameAbandoned += () => abandonFired = true;

        _bm.ProcessPlayerAction(BetAction.AbandonGame);

        Assert.AreEqual(playerPre, _bm.PlayerChips, "Abandonar no debe cambiar fichas del jugador");
        Assert.AreEqual(npcPre,    _bm.NpcChips,    "Abandonar no debe cambiar fichas del NPC");
        Assert.IsTrue(abandonFired, "OnGameAbandoned debe dispararse");
    }

    // ── T18-T19: Game Over ────────────────────────────────────────────────────

    [Test]
    public void T18_GameOverCuandoJugadorPierdeUltimasFichas()
    {
        SetChips(10, 1000);
        bool gameOverFired = false;
        _bm.OnGameOver += () => gameOverFired = true;

        // AllIn es determinista: apuesta todas las fichas sin depender de NpcBet
        _bm.ProcessPlayerAction(BetAction.AllIn); // apuesta 10 (todo lo que tiene)
        _bm.ResolveResult(RoundOutcome.Lose);     // EffectiveBet = min(10, NpcBet) = 10 → 0 fichas

        Assert.AreEqual(0, _bm.PlayerChips, "PlayerChips debe ser 0");
        Assert.IsTrue(gameOverFired, "OnGameOver debe dispararse al quedarse sin fichas");
    }

    [Test]
    public void T19_NpcBankruptCuandoNpcPierdeUltimasFichas()
    {
        // NpcBettingAI con NpcChips=10 y playerBet=10 siempre apuesta 10 (Moderado)
        SetChips(1000, 10);
        SetLastBet(10);
        bool npcBankruptFired = false;
        _bm.OnNpcBankrupt += () => npcBankruptFired = true;

        _bm.PrepareDuel();                    // NpcBet = 10 (determinista)
        _bm.ProcessPlayerAction(BetAction.Equal); // apuesta min(10, 1000) = 10
        _bm.ResolveResult(RoundOutcome.Win);      // NpcChips -= 10 = 0

        Assert.AreEqual(0, _bm.NpcChips,   "NpcChips debe ser 0");
        Assert.IsTrue(npcBankruptFired,     "OnNpcBankrupt debe dispararse al quedarse el NPC sin fichas");
    }

    // ── T20: Edge case — NpcBet > PlayerChips ────────────────────────────────

    [Test]
    public void T20_IgualarConNpcBetMayorQuePlayerChipsNuncaHaceNegativo()
    {
        // NpcChips=1000 → NpcBet >= 10 siempre. PlayerChips=5 < 10 → siempre capado
        SetChips(5, 1000);
        _bm.PrepareDuel(); // NpcBet >= 10

        _bm.ProcessPlayerAction(BetAction.Equal); // min(NpcBet, 5) = 5
        Assert.LessOrEqual(_bm.CurrentBet, 5, "CurrentBet no puede superar PlayerChips");

        _bm.ResolveResult(RoundOutcome.Lose);
        Assert.GreaterOrEqual(_bm.PlayerChips, 0, "PlayerChips nunca debe ser negativo");
    }

    // ── T21-T23: Eventos ─────────────────────────────────────────────────────

    [Test]
    public void T21_OnBetConfirmedDisparaConValoresCorrectos()
    {
        int capturedPlayer = -1, capturedNpc = -1;
        _bm.OnBetConfirmed += (p, n) => { capturedPlayer = p; capturedNpc = n; };

        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);

        Assert.AreEqual(_bm.CurrentBet, capturedPlayer, "OnBetConfirmed debe incluir la apuesta del jugador");
        Assert.AreEqual(_bm.NpcBet,     capturedNpc,    "OnBetConfirmed debe incluir la apuesta del NPC");
    }

    [Test]
    public void T22_OnChipsChangedDisparaEnVictoriaYDerrota()
    {
        int cambios = 0;
        _bm.OnChipsChanged += () => cambios++;

        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        _bm.ResolveResult(RoundOutcome.Win);
        Assert.AreEqual(1, cambios, "OnChipsChanged debe dispararse tras victoria");

        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        _bm.ResolveResult(RoundOutcome.Lose);
        Assert.AreEqual(2, cambios, "OnChipsChanged debe dispararse tras derrota");
    }

    [Test]
    public void T22b_OnChipsChangedNoDisparaEnEmpate()
    {
        int cambios = 0;
        _bm.OnChipsChanged += () => cambios++;

        _bm.PrepareDuel();
        _bm.ProcessPlayerAction(BetAction.Equal);
        _bm.ResolveResult(RoundOutcome.Draw);

        Assert.AreEqual(0, cambios, "OnChipsChanged NO debe dispararse en empate");
    }

    [Test]
    public void T23_OnGameOverDisparaAlQuedarSinFichas()
    {
        SetChips(_bm.minimumBet, 1000);
        bool fired = false;
        _bm.OnGameOver += () => fired = true;

        _bm.ProcessPlayerAction(BetAction.FoldRound);

        Assert.IsTrue(fired, "OnGameOver debe dispararse cuando PlayerChips llega a 0");
    }
}
