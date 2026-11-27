namespace Dramon
{
    /// <summary>결투 중 카드 한 장 (같은 카드 데이터라도 장마다 따로)</summary>
    public class CardInstance
    {
        static int nextId;

        public CardData Data { get; }
        public int Id { get; }

        public CardInstance(CardData data)
        {
            Data = data;
            Id = ++nextId;
        }
    }
}
