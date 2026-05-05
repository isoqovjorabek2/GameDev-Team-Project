using UnityEngine;

namespace CircuitSolver.Data
{
    /// <summary>
    /// Central art-style palette as described in the design doc.
    /// Stored as static Colors so any system can reuse them without
    /// shipping an asset.
    /// </summary>
    public static class Theme
    {
        public static readonly Color BackgroundNavy   = Hex("#0D1B2A");
        public static readonly Color PanelNavy        = Hex("#152238");
        public static readonly Color PanelNavyLight   = Hex("#1B2B45");
        public static readonly Color BoardCream       = Hex("#F0EDE8");
        public static readonly Color BoardGrid        = Hex("#D6D2CB");
        public static readonly Color AccentGreen      = Hex("#00FF87");
        public static readonly Color AccentGreenSoft  = Hex("#2FBF71");
        public static readonly Color WireOrange       = Hex("#FF6B35");
        public static readonly Color ResistorOrange   = Hex("#FF8C42");
        public static readonly Color BatteryGreen     = Hex("#00FF87");
        public static readonly Color TextPrimary      = Hex("#F5F7FA");
        public static readonly Color TextMuted        = Hex("#8FA0B8");
        public static readonly Color TextOnBoard      = Hex("#1A2238");
        public static readonly Color DangerRed        = Hex("#FF3B5C");
        public static readonly Color WarningYellow    = Hex("#FFD166");

        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.magenta;
        }
    }
}
