
using Godot;

namespace Orcinus.Scripts.Core
{
    public class EnhancedConfigFile : ConfigFile
    {
        private string _filePath;

        public EnhancedConfigFile(string filePath)
        {
            _filePath = filePath;

            Load(filePath);
        }

        public T GetValue<T>(string section, string key, object @default = null)
        {
            return (T)GetValue(section, key, @default);
        }
    }
}
