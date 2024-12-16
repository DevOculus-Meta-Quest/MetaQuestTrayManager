using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MetaQuestTrayManager.Utils
{
    public static class JsonFunctions
    {
        // Default settings for JSON serialization and deserialization
        private static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            MaxDepth = 1024,
            NullValueHandling = NullValueHandling.Ignore,
            Error = (sender, args) =>
            {
                args.ErrorContext.Handled = true;
                ErrorLogger.LogError(args.ErrorContext.Error, "JSON Serialization/Deserialization Error.");
            },
            Formatting = Formatting.Indented // Use indented formatting for better readability
        };

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        public static string Serialize<T>(T obj, JsonSerializerSettings? settings = null)
        {
            settings ??= DefaultJsonSettings;

            try
            {
                return JsonConvert.SerializeObject(obj, settings);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to serialize object of type {typeof(T).Name}.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Deserializes a JSON string to an object.
        /// </summary>
        public static T? Deserialize<T>(string json, JsonSerializerSettings? settings = null, params string[] ignoredFields)
        {
            settings ??= DefaultJsonSettings;

            if (ignoredFields.Length > 0)
            {
                settings = CreateSettingsWithIgnoredFields<T>(settings, ignoredFields);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"Failed to deserialize JSON to type {typeof(T).Name}.");
                return default;
            }
        }

        /// <summary>
        /// Creates custom JsonSerializerSettings with specified fields ignored during serialization/deserialization.
        /// </summary>
        private static JsonSerializerSettings CreateSettingsWithIgnoredFields<T>(JsonSerializerSettings baseSettings, params string[] ignoredFields)
        {
            var customSettings = DeepCopySettings(baseSettings);
            var resolver = new IgnorableSerializerContractResolver();
            resolver.Ignore(typeof(T), ignoredFields);
            customSettings.ContractResolver = resolver;
            return customSettings;
        }

        /// <summary>
        /// Creates a deep copy of the given JsonSerializerSettings.
        /// </summary>
        private static JsonSerializerSettings DeepCopySettings(JsonSerializerSettings settings)
        {
            return new JsonSerializerSettings
            {
                Context = settings.Context,
                Culture = settings.Culture,
                ContractResolver = settings.ContractResolver,
                Converters = settings.Converters,
                ConstructorHandling = settings.ConstructorHandling,
                CheckAdditionalContent = settings.CheckAdditionalContent,
                DateFormatHandling = settings.DateFormatHandling,
                DateFormatString = settings.DateFormatString,
                DateParseHandling = settings.DateParseHandling,
                DateTimeZoneHandling = settings.DateTimeZoneHandling,
                DefaultValueHandling = settings.DefaultValueHandling,
                EqualityComparer = settings.EqualityComparer,
                FloatFormatHandling = settings.FloatFormatHandling,
                Formatting = settings.Formatting,
                FloatParseHandling = settings.FloatParseHandling,
                MaxDepth = settings.MaxDepth,
                MetadataPropertyHandling = settings.MetadataPropertyHandling,
                MissingMemberHandling = settings.MissingMemberHandling,
                NullValueHandling = settings.NullValueHandling,
                ObjectCreationHandling = settings.ObjectCreationHandling,
                PreserveReferencesHandling = settings.PreserveReferencesHandling,
                ReferenceLoopHandling = settings.ReferenceLoopHandling,
                StringEscapeHandling = settings.StringEscapeHandling,
                TraceWriter = settings.TraceWriter,
                TypeNameHandling = settings.TypeNameHandling,
                SerializationBinder = settings.SerializationBinder,
                TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling
            };
        }
    }

    /// <summary>
    /// Custom contract resolver to ignore specified properties during serialization/deserialization.
    /// </summary>
    public class IgnorableSerializerContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> _ignoredProperties = new();

        /// <summary>
        /// Adds properties to be ignored for a specific type.
        /// </summary>
        public void Ignore(Type type, params string[] propertyNames)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (propertyNames == null || propertyNames.Length == 0) throw new ArgumentNullException(nameof(propertyNames));

            if (!_ignoredProperties.ContainsKey(type))
            {
                _ignoredProperties[type] = new HashSet<string>();
            }

            foreach (var property in propertyNames)
            {
                _ignoredProperties[type].Add(property);
            }
        }

        /// <summary>
        /// Checks if a property should be ignored during serialization/deserialization.
        /// </summary>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType != null &&
                _ignoredProperties.TryGetValue(member.DeclaringType, out var properties) &&
                properties.Contains(property.PropertyName))
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }
}
