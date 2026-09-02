namespace Randomizer.CatQuest3
{
    public class CollectibleRewardData
    {
        public string PrefabName { get; }

        public int Quantity { get; }
        public int Value { get; }

        public bool RandomQuantity { get; }
        public int QuantityMin { get; }
        public int QuantityMax { get; }

        public bool RandomValue { get; }
        public int ValueMin { get; }
        public int ValueMax { get; }

        public bool IgnoreCollectibleMultiplier { get; }

        public int QuantityMultiplier { get; }
        public int ValueMultiplier { get; }

        public CollectibleRewardData(
            string prefabName,
            int quantity,
            int value,
            bool randomQuantity,
            int quantityMin,
            int quantityMax,
            bool randomValue,
            int valueMin,
            int valueMax,
            bool ignoreCollectibleMultiplier,
            int quantityMultiplier = 1,
            int valueMultiplier = 1)
        {
            PrefabName = prefabName;

            Quantity = quantity;
            Value = value;

            RandomQuantity = randomQuantity;
            QuantityMin = quantityMin;
            QuantityMax = quantityMax;

            RandomValue = randomValue;
            ValueMin = valueMin;
            ValueMax = valueMax;

            IgnoreCollectibleMultiplier = ignoreCollectibleMultiplier;

            QuantityMultiplier = quantityMultiplier;
            ValueMultiplier = valueMultiplier;
        }
    }
}