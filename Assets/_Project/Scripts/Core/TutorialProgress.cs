using System;

namespace EcosDelAzar.Core
{
    /// <summary>
    /// Run-scoped progress of the lobby tutorial. The sequence itself lives in
    /// the lobby (TutorialSequence); this holds the stage so the elevator and the
    /// HUD can read it from any scene and it survives reloads within the run.
    /// Order: move → concierge (coins) → oxygen machine (sell air for coins) → elevator → first table.
    /// The machine step comes before the elevator on purpose: it is the safety
    /// valve the player will need the first time a table leaves them broke.
    /// </summary>
    public static class TutorialProgress
    {
        public enum Stage { Move = 0, TalkToConcierge = 1, TradeOxygen = 2, UseElevator = 3, PlayTable = 4, Done = 5 }

        const string StageKey = "tutorial.stage";
        const int StepCount = 5;

        public static Stage Current
        {
            get => (Stage)RunPrefs.GetInt(StageKey, (int)Stage.Move);
            private set { RunPrefs.SetInt(StageKey, (int)value); RunPrefs.Save(); }
        }

        public static bool IsDone => Current == Stage.Done;

        /// <summary>The elevator stays shut until the player has met the concierge and tried the O2 machine.</summary>
        public static bool ElevatorLocked => Current < Stage.UseElevator;

        /// <summary>Why the elevator is shut, for its world label.</summary>
        public static string ElevatorLockedHint => Current < Stage.TradeOxygen
            ? "Habla antes con el conserje"
            : "Prueba antes la máquina de O2";

        /// <summary>Current objective, or null when the tutorial is done.</summary>
        public static Objective? CurrentObjectiveOrNull => ObjectiveFor(Current);

        public static event Action<Objective?> OnObjectiveChanged;

        /// <summary>Re-emits the current objective (scene load, HUD re-enable).</summary>
        public static void Rebroadcast() => OnObjectiveChanged?.Invoke(CurrentObjectiveOrNull);

        /// <summary>Moves forward only; stages can never regress.</summary>
        public static void Advance(Stage stage)
        {
            if (stage <= Current) return;
            Current = stage;
            OnObjectiveChanged?.Invoke(ObjectiveFor(stage));
        }

        /// <summary>What the HUD shows: step counter, icon glyph and text. Null when nothing is pending.</summary>
        public readonly struct Objective
        {
            public readonly int Step;
            public readonly int Total;
            public readonly string Icon;
            public readonly string Text;

            public Objective(int step, string icon, string text)
            {
                Step = step; Total = StepCount; Icon = icon; Text = text;
            }
        }

        static Objective? ObjectiveFor(Stage stage) => stage switch
        {
            Stage.Move => new Objective(1, "→", "Muévete con W A S D"),
            Stage.TalkToConcierge => new Objective(2, "?", "Habla con el conserje  [E]"),
            Stage.TradeOxygen => new Objective(3, "O2", "Vende un poco de aire en la máquina de O2  [E]"),
            Stage.UseElevator => new Objective(4, "▲", "Sube en el ascensor  [E]"),
            Stage.PlayTable => new Objective(5, "$", "Siéntate en una mesa y apuesta  [E]"),
            _ => null
        };
    }
}
