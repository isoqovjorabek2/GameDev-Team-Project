using System;

namespace SetGame.Core
{
    public enum CardShape  { Diamond, Oval, Squiggle }
    public enum CardColor  { Red, Green, Purple }
    public enum CardCount  { One = 1, Two = 2, Three = 3 }
    public enum CardShading{ Solid, Striped, Open }

    [Serializable]
    public struct CardData : IEquatable<CardData>
    {
        public CardShape   Shape;
        public CardColor   Color;
        public CardCount   Count;
        public CardShading Shading;

        public CardData(CardShape shape, CardColor color, CardCount count, CardShading shading)
        {
            Shape = shape; Color = color; Count = count; Shading = shading;
        }

        public bool Equals(CardData other) =>
            Shape == other.Shape && Color == other.Color &&
            Count == other.Count && Shading == other.Shading;

        public override bool Equals(object obj) => obj is CardData d && Equals(d);

        public override int GetHashCode() =>
            ((int)Shape * 27) + ((int)Color * 9) + ((int)Count * 3) + (int)Shading;

        public override string ToString() =>
            $"{Count} {Color} {Shading} {Shape}";
    }
}
