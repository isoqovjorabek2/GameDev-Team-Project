using System.Collections.Generic;
using UnityEngine;

namespace SetGame.Core
{
    public class Deck
    {
        private readonly List<CardData> _cards = new();

        public int Remaining => _cards.Count;

        public Deck() => Rebuild();

        public void Rebuild()
        {
            _cards.Clear();
            foreach (CardShape   shape   in System.Enum.GetValues(typeof(CardShape)))
            foreach (CardColor   color   in System.Enum.GetValues(typeof(CardColor)))
            foreach (CardCount   count   in System.Enum.GetValues(typeof(CardCount)))
            foreach (CardShading shading in System.Enum.GetValues(typeof(CardShading)))
                _cards.Add(new CardData(shape, color, count, shading));
            Shuffle();
        }

        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public CardData? Draw()
        {
            if (_cards.Count == 0) return null;
            var card = _cards[^1];
            _cards.RemoveAt(_cards.Count - 1);
            return card;
        }
    }
}
