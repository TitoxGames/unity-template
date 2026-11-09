using System;
using System.IO;
using System.Linq;
using TitoX.UI;
using TitoX.UI.Components;
using TitoX.UI.Flow;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TitoX.Editor
{
    /// <summary>
    /// Creates the Shop, Collections and bottom bar screen prefabs and registers them like the other screens: Addressables
    /// entry, UIConfigSO entry, and the bar in the Home state of UIFlowConfig. Each prefab uses the game's single subclass
    /// of the base screen when there is one. Screens already registered in UIConfigSO are left untouched.
    /// </summary>
    public static class HomeTabScreensBuilder
    {
        private const float BarHeight = 220f;
        private const float SelectedRise = 30f;
        private const float IconSize = 160f;
        // Above full screens (0) and below popups (200+), so Settings still opens over the bar.
        private const int BarSortingOrder = 100;

        private static readonly Color BarColor = new Color(0.11f, 0.35f, 0.85f);
        private static readonly Color SelectedColor = new Color(0.45f, 0.85f, 0.96f);
        private static readonly Color ShopBackground = new Color(0.98f, 0.62f, 0.42f);
        private static readonly Color CollectionsBackground = new Color(0.62f, 0.5f, 0.92f);
        private static readonly HomeTab[] TabOrder = { HomeTab.Shop, HomeTab.Home, HomeTab.Collections };

        [MenuItem("Tools/Game Template/UI/Build Home Tab Screens")]
        public static void Build()
        {
            UIConfigSO uiConfig = LoadSingle<UIConfigSO>();
            UIFlowConfig flowConfig = LoadSingle<UIFlowConfig>();
            if (uiConfig == null || flowConfig == null)
                return;

            string homePath = FindRegisteredPrefabPath(uiConfig, typeof(HomeScreenBase));
            string folder = homePath != null ? Path.GetDirectoryName(homePath)?.Replace('\\', '/') : "Assets";
            TMP_FontAsset font = FindFont(homePath);

            EnsureScreen<ShopScreenBase>(uiConfig, folder, CanvasType.FullScreen, -1,
                (_, root) => BuildPlaceholderPage(root, "Shop", ShopBackground, font));
            EnsureScreen<CollectionsScreenBase>(uiConfig, folder, CanvasType.FullScreen, -1,
                (_, root) => BuildPlaceholderPage(root, "Collections", CollectionsBackground, font));
            string bar = EnsureScreen<HomeBottomBarScreenBase>(uiConfig, folder, CanvasType.Popup, BarSortingOrder,
                (screen, root) => BuildBar((HomeBottomBarScreenBase)screen, root, font));

            AddToFlowState(flowConfig, UIFlowState.Home, bar);

            AssetDatabase.SaveAssets();
            Debug.Log($"[HomeTabScreensBuilder] Home tab screens are ready in {folder}. Assign the tab icons in the bottom bar prefab.");
        }

        private static string EnsureScreen<TBase>(UIConfigSO uiConfig, string folder, CanvasType canvasType, int sortingOrder,
            Action<UIBase, RectTransform> build) where TBase : UIBase
        {
            string registered = FindRegisteredPrefabPath(uiConfig, typeof(TBase));
            if (registered != null)
            {
                Debug.Log($"[HomeTabScreensBuilder] {typeof(TBase).Name} is already registered: {registered}.");
                return AssetDatabase.AssetPathToGUID(registered);
            }

            Type type = ResolveScreenType<TBase>();
            string name = type.Name.EndsWith("Base") ? type.Name.Substring(0, type.Name.Length - "Base".Length) : type.Name;
            string path = $"{folder}/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                CreatePrefab(path, name, type, build);

            string guid = AssetDatabase.AssetPathToGUID(path);
            AddressableAssetSettings addressables = AddressableAssetSettingsDefaultObject.Settings;
            if (addressables.FindAssetEntry(guid) == null)
                addressables.CreateOrMoveEntry(guid, addressables.DefaultGroup);

            AddUIConfig(uiConfig, guid, type, canvasType, sortingOrder);
            return guid;
        }

        private static Type ResolveScreenType<TBase>() where TBase : UIBase
        {
            Type[] subclasses = TypeCache.GetTypesDerivedFrom<TBase>()
                .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition)
                .ToArray();

            if (subclasses.Length == 1)
                return subclasses[0];

            if (subclasses.Length > 1)
                Debug.LogWarning($"[HomeTabScreensBuilder] Several subclasses of {typeof(TBase).Name}; using the base screen.");

            return typeof(TBase);
        }

        private static void CreatePrefab(string path, string name, Type type, Action<UIBase, RectTransform> build)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");
            var root = (RectTransform)go.transform;
            Stretch(root);

            var screen = (UIBase)go.AddComponent(type);
            build(screen, root);

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void BuildPlaceholderPage(RectTransform root, string title, Color background, TMP_FontAsset font)
        {
            RectTransform backgroundRect = CreateRect("Background", root);
            Stretch(backgroundRect);
            backgroundRect.gameObject.AddComponent<Image>().color = background;

            RectTransform safeArea = CreateRect("SafeArea", root);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeArea>();

            TMP_Text titleText = CreateLabel("Title", safeArea, title, font, 90f);
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.88f);
            titleRect.sizeDelta = new Vector2(900f, 160f);
        }

        private static void BuildBar(HomeBottomBarScreenBase screen, RectTransform root, TMP_FontAsset font)
        {
            RectTransform safeArea = CreateRect("SafeArea", root);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<SafeArea>();

            RectTransform bar = CreateRect("BottomBar", safeArea);
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(0f, BarHeight);
            bar.anchoredPosition = Vector2.zero;

            RectTransform background = CreateRect("Background", bar);
            Stretch(background);
            background.gameObject.AddComponent<Image>().color = BarColor;

            var extend = bar.gameObject.AddComponent<ExtendToScreenBottom>();
            SetArray(extend, "targets", 1, (element, _) => element.objectReferenceValue = background);

            RectTransform tabsRoot = CreateRect("Tabs", bar);
            Stretch(tabsRoot);
            var layout = tabsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var tabs = new HomeTabButton[TabOrder.Length];
            for (int i = 0; i < TabOrder.Length; i++)
                tabs[i] = BuildTab(TabOrder[i], tabsRoot, font);

            SetArray(screen, "tabs", tabs.Length, (element, i) => element.objectReferenceValue = tabs[i]);
        }

        private static HomeTabButton BuildTab(HomeTab tab, Transform parent, TMP_FontAsset font)
        {
            RectTransform rect = CreateRect($"{tab}Tab", parent);
            var hitArea = rect.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            rect.gameObject.AddComponent<Button>().targetGraphic = hitArea;

            RectTransform selected = CreateRect("Selected", rect);
            Stretch(selected);
            selected.offsetMax = new Vector2(0f, SelectedRise);
            var selectedImage = selected.gameObject.AddComponent<Image>();
            selectedImage.color = SelectedColor;
            selectedImage.raycastTarget = false;
            selected.gameObject.SetActive(false);

            RectTransform icon = CreateRect("Icon", rect);
            icon.sizeDelta = new Vector2(IconSize, IconSize);
            var iconImage = icon.gameObject.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            TMP_Text label = CreateLabel("Label", rect, tab.ToString(), font, 36f);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = new Vector2(1f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(0f, 50f);
            labelRect.anchoredPosition = new Vector2(0f, 10f);
            label.gameObject.SetActive(false);

            var tabButton = rect.gameObject.AddComponent<HomeTabButton>();
            var serialized = new SerializedObject(tabButton);
            serialized.FindProperty("tab").intValue = (int)tab;
            serialized.FindProperty("selectedBackground").objectReferenceValue = selected.gameObject;
            serialized.FindProperty("icon").objectReferenceValue = icon;
            serialized.FindProperty("label").objectReferenceValue = label.gameObject;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return tabButton;
        }

        private static string FindRegisteredPrefabPath(UIConfigSO uiConfig, Type baseType)
        {
            foreach (UIConfig config in uiConfig.Configs)
            {
                if (config.AssetReference == null || string.IsNullOrEmpty(config.TypeName))
                    continue;

                Type type = Type.GetType(config.TypeName);
                string path = AssetDatabase.GUIDToAssetPath(config.AssetReference.AssetGUID);
                if (type != null && baseType.IsAssignableFrom(type) && !string.IsNullOrEmpty(path))
                    return path;
            }

            return null;
        }

        private static void AddUIConfig(UIConfigSO uiConfig, string guid, Type type, CanvasType canvasType, int sortingOrder)
        {
            var serialized = new SerializedObject(uiConfig);
            SerializedProperty configs = serialized.FindProperty("_configs");
            for (int i = 0; i < configs.arraySize; i++)
            {
                if (configs.GetArrayElementAtIndex(i).FindPropertyRelative("_assetReference.m_AssetGUID").stringValue == guid)
                    return;
            }

            configs.arraySize++;
            SerializedProperty entry = configs.GetArrayElementAtIndex(configs.arraySize - 1);
            entry.FindPropertyRelative("_assetReference.m_AssetGUID").stringValue = guid;
            entry.FindPropertyRelative("_canvasType").intValue = (int)canvasType;
            entry.FindPropertyRelative("_showAnimationType").intValue = (int)UIAnimationType.None;
            entry.FindPropertyRelative("_hideAnimationType").intValue = (int)UIAnimationType.None;
            entry.FindPropertyRelative("_lifeCyclePolicy").intValue = (int)UILifeCyclePolicy.SaveInstance;
            entry.FindPropertyRelative("_sortingOrder").intValue = sortingOrder;
            entry.FindPropertyRelative("_animationDuration").floatValue = 0.2f;
            entry.FindPropertyRelative("_typeName").stringValue = type.AssemblyQualifiedName;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            uiConfig.ClearDictionary();
            EditorUtility.SetDirty(uiConfig);
        }

        private static void AddToFlowState(UIFlowConfig flowConfig, UIFlowState state, string guid)
        {
            var serialized = new SerializedObject(flowConfig);
            SerializedProperty states = serialized.FindProperty("states");
            for (int i = 0; i < states.arraySize; i++)
            {
                SerializedProperty entry = states.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("State").intValue != (int)state)
                    continue;

                SerializedProperty screens = entry.FindPropertyRelative("Screens");
                for (int j = 0; j < screens.arraySize; j++)
                {
                    if (screens.GetArrayElementAtIndex(j).FindPropertyRelative("m_AssetGUID").stringValue == guid)
                        return;
                }

                screens.arraySize++;
                screens.GetArrayElementAtIndex(screens.arraySize - 1).FindPropertyRelative("m_AssetGUID").stringValue = guid;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(flowConfig);
                return;
            }

            Debug.LogError($"[HomeTabScreensBuilder] {flowConfig.name} has no {state} state; add the bottom bar to it by hand.");
        }

        private static TMP_FontAsset FindFont(string homePrefabPath)
        {
            GameObject homeScreen = homePrefabPath != null ? AssetDatabase.LoadAssetAtPath<GameObject>(homePrefabPath) : null;
            TMP_Text sample = homeScreen != null ? homeScreen.GetComponentInChildren<TMP_Text>(true) : null;
            return sample != null ? sample.font : TMP_Settings.defaultFontAsset;
        }

        private static T LoadSingle<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length != 1)
            {
                Debug.LogError($"[HomeTabScreensBuilder] Expected one {typeof(T).Name} asset, found {guids.Length}.");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static TMP_Text CreateLabel(string name, Transform parent, string text, TMP_FontAsset font, float size)
        {
            RectTransform rect = CreateRect(name, parent);
            Stretch(rect);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetArray(Object target, string property, int size, Action<SerializedProperty, int> setElement)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty array = serialized.FindProperty(property);
            array.arraySize = size;
            for (int i = 0; i < size; i++)
                setElement(array.GetArrayElementAtIndex(i), i);

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
