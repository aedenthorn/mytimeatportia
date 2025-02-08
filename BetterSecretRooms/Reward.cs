namespace Chests
{
    internal class Reward
    {
        public DoubleValue<int, FloatR>[] rewardsArray;
        public IntR levelRange;

        public Reward(DoubleValue<int, FloatR>[] rewards, IntR range)
        {
            rewardsArray = rewards;
            levelRange = range;
        }
    }
}