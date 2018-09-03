using Modding;

namespace LostLord
{
    public class LostSettings : IModSettings
    {
        public bool DefeatedLord { get => GetBool(); set => SetBool(value); } 
    }
}