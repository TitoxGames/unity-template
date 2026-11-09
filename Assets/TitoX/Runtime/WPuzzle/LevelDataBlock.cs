using MemoryPack;
using WData;

namespace WPuzzle
{
    [MemoryPackable]
    public partial class LevelData
    {
        public int CurrentLevelNumber = 1;
    }

    public sealed class LevelDataBlock
        : DataBlock<LevelDataBlock, LevelData>
    {
        public int CurrentLevelNumber => Data.CurrentLevelNumber;

        protected override void Init()
        {
            base.Init();

            if (Data.CurrentLevelNumber < 1)
            {
                Data.CurrentLevelNumber = 1;
                Save();
            }
        }

        public void SetCurrentLevelNumber(int levelNumber)
        {
            if (Data.CurrentLevelNumber == levelNumber)
                return;

            Data.CurrentLevelNumber = levelNumber;

            Save();
        }
    }
}