using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
using GameTemplate.Runtime.WGUI;
#endif

namespace GameTemplate.Runtime.WGUI
{
    [CreateAssetMenu(menuName = "WGUI/UI Configs")]
    public class UIConfigSO : ScriptableObject
    {

        [SerializeField] private List<UIConfig> _configs;
        private Dictionary<Type, UIConfig> _configDictionary;
        private Dictionary<Type, Type> _resolvedTypes;

        public IReadOnlyList<UIConfig> Configs => _configs;

        public void EnsureDictionary()
        {
            if (_configDictionary != null) return;

            _configDictionary = new Dictionary<Type, UIConfig>();
            _resolvedTypes = new Dictionary<Type, Type>();

            foreach (var config in _configs)
            {
                if (string.IsNullOrEmpty(config.TypeName))
                    continue;

                Type type = Type.GetType(config.TypeName);

                if (type == null)
                    continue;

                if (!_configDictionary.TryAdd(type, config))
                {
                    Debug.LogError($"Duplicate UIConfig for type {type.FullName} in {name}");
                }
            }

        }
        public void ClearDictionary()
        {
            _configDictionary?.Clear();
            _configDictionary = null;
            _resolvedTypes = null;
        }

        /// <summary>
        /// The registered screen type for <paramref name="type"/>: the type itself, or the single registered subclass of it,
        /// so shared code can show a base screen (e.g. a template WinScreenBase) that each game subclasses.
        /// </summary>
        public Type ResolveType(Type type)
        {
            if (type == null)
                return null;

            EnsureDictionary();

            if (_configDictionary.ContainsKey(type))
                return type;

            if (_resolvedTypes.TryGetValue(type, out Type resolved))
                return resolved;

            foreach (Type candidate in _configDictionary.Keys)
            {
                if (!type.IsAssignableFrom(candidate))
                    continue;

                if (resolved != null)
                {
                    Debug.LogError($"[UIConfigSO] {resolved.Name} and {candidate.Name} both derive from {type.Name}; show the concrete type instead.");
                    break;
                }

                resolved = candidate;
            }

            _resolvedTypes[type] = resolved;
            return resolved;
        }

        public UIConfig GetConfig(Type type)
        {
            if (type == null)
            {
                Debug.LogError("Type is null");
                return null;
            }

            Type resolved = ResolveType(type);
            if (resolved == null)
                return null;

            _configDictionary.TryGetValue(resolved, out UIConfig config);
            return config;
        }

        /// <summary>The screen type registered for the prefab behind <paramref name="reference"/>.</summary>
        public bool TryGetType(AssetReference reference, out Type type)
        {
            type = null;
            if (reference == null || string.IsNullOrEmpty(reference.AssetGUID))
                return false;

            foreach (UIConfig config in _configs)
            {
                if (config.AssetReference == null || config.AssetReference.AssetGUID != reference.AssetGUID)
                    continue;

                if (!string.IsNullOrEmpty(config.TypeName))
                    type = Type.GetType(config.TypeName);

                return type != null;
            }

            return false;
        }

        public int ResolveSortingOrder(UIConfig uiConfig, int defaultOrder)
        {
            if (uiConfig == null)
                return defaultOrder;

            return uiConfig.SortingOrder >= 0
                ? uiConfig.SortingOrder
                : defaultOrder;
        }
#if UNITY_EDITOR

        private void OnValidate()
        {
            foreach (var config in _configs)
            {
                UpdateType(config);
            }

            ClearDictionary();
        }

        private void UpdateType(UIConfig config)
        {
            if (config.AssetReference == null)
                return;

            string path = AssetDatabase.GUIDToAssetPath(config.AssetReference.AssetGUID);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                return;

            UIBase ui = prefab.GetComponent<UIBase>();

            if (ui == null)
                return;

            config.TypeName = ui.GetType().AssemblyQualifiedName;
        }

#endif

    }
}
