using System.Collections.Generic;
using CircuitSolver.Data;
using UnityEngine;

namespace CircuitSolver.Components
{
    /// <summary>
    /// Applies green-pulse and red-shake feedback on the ComponentSprites
    /// that belong to the checked puzzle. Centralized so screens can
    /// trigger a unified animation pass after a CheckSolution call.
    /// </summary>
    public class HighlightSystem : MonoBehaviour
    {
        public void FlashCorrect(IEnumerable<ComponentSprite> sprites)
        {
            foreach (var s in sprites)
                if (s != null) s.Pulse(Theme.AccentGreen);
        }

        public void FlashWrong(IEnumerable<ComponentSprite> sprites)
        {
            foreach (var s in sprites)
            {
                if (s == null) continue;
                s.Pulse(Theme.DangerRed);
                s.Shake();
            }
        }
    }
}
