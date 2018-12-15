using Modding;

namespace LostLord
{
    public class LordSettings : IModSettings
    {
        public void Reset()
        {
            BoolValues.Clear();

            Pure = true;
        }

        public bool Pure
        {
            get => GetBool();
            private set => SetBool(value);
        }
    }
}