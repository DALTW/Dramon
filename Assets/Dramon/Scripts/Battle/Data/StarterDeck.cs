using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dramon
{
    /// <summary>처음 덱 구성 (카드 + 장수)</summary>
    [CreateAssetMenu(fileName = "StarterDeck", menuName = "Dramon/Starter Deck")]
    public class StarterDeck : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public CardData card;
            [Min(0)] public int count = 1;
        }

        [SerializeField] List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;
    }
}
