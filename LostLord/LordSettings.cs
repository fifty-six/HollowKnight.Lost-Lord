using Modding;

namespace LostLord
{
    public class LordSettings : IModSettings
    {
        public const int SETTINGS_VER = 1;
        
        public void Reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            StringValues.Clear();

            Pure = true;
            SettingsVersion = SETTINGS_VER;
        }
        
        public int SettingsVersion { get => GetInt(); private set => SetInt(value); }
        public bool Pure { get => GetBool(); private set => SetBool(value); }
    }
}