namespace com.wrapper.centeralizedconfiguration
{
    public class Helper
    {
        public static Dictionary<string, object> GetSubSectionConfigs(IConfigurationSection section, string parentKey = "")
        {
            Dictionary<string, object> configValues = new Dictionary<string, object>();

            foreach (IConfigurationSection child in section.GetChildren())
            {
                var key = string.IsNullOrEmpty(parentKey)? child.Key : $"{parentKey}:{child.Key}";
                if (child.GetChildren().Any())
                {
                    var nestedValue = GetSubSectionConfigs(child, key);
                    foreach(var value in nestedValue)
                    {
                        configValues[value.Key] = value.Value;
                    }
                }
                else
                {
                    configValues[key] = child.Value;
                }
            }

            if (!section.GetChildren().Any())
            {
                configValues[section.Key] = section.Value;
            }

            return configValues;
        }
    }
}
