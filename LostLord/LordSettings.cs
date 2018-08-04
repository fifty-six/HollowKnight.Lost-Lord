using Modding;

namespace LostLord
{
    public class LordSettings : IModSettings
    {
        public bool DefeatedLord { get => GetBool(); set => SetBool(value); }
    }
}