using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Installer.Editor.Validator
{
    [InitializeOnLoad]
    public static class FrameworkValidator
    {
        private const string SessionKey = "TitoX.Framework.Validator";

        public class Item
        {
            public string Name;
            public string Description;
            public Func<bool> Validator;
            public Action Install;
        }

        public static readonly List<Item> Items = new()
        {
            new Item
            {
                Name = "Odin Inspector",
                Description = "Advanced inspector framework.",
                Validator = HasOdin,
                Install = () =>
                {
                    Application.OpenURL(FrameworkLinks.Odin);
                }
            },

            new Item
            {
                Name = "DOTween Pro",
                Description = "Tween animation library.",
                Validator = HasDotween,
                Install = () =>
                {
                    Application.OpenURL(FrameworkLinks.Dotween);
                }
            },

            new Item
            {
                Name = "DOTween ASMDEF",
                Description = "Generate DOTween Assembly Definition.",
                Validator = HasDotweenAsmdef,
                Install = OpenDotweenUtility
            },

            new Item
            {
                Name = "UNITASK_DOTWEEN_SUPPORT",
                Description = "Enable UniTask DOTween support.",
                Validator = HasUnitaskDotweenSupport,
                Install = InstallUnitaskDotweenSupport
            },

            new Item
            {
                Name = "Addressables",
                Description = "Unity Addressables package.",
                Validator = HasAddressables,
                Install = () =>
                {
#if UNITY_2021_3_OR_NEWER
                    PackageInstaller.Install("com.unity.addressables");
#endif
                }
            },

            new Item
            {
                Name = "UniTask",
                Description = "Async/Await library.",
                Validator = HasUniTask,
                Install = () =>
                {
#if UNITY_2021_3_OR_NEWER
                    PackageInstaller.Install(
                        "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
#endif
                }
            },
            new Item
            {
                Name = "NuGetForUnity",
                Description = "Required to install NuGet packages such as MemoryPack.",
                Validator = HasNuGetForUnity,
                Install = () =>
                {
#if UNITY_2021_3_OR_NEWER
                    PackageInstaller.Install(
                        "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity");
#endif
                }
            },
            new Item
            {
                Name = "MemoryPack",
                Description = "High-performance serialization library.",
                Validator = HasMemoryPack,
                Install = () => {}
            },
            new Item
            {
                Name = "Newtonsoft Json",
                Description = "Newtonsoft Json package.",
                Validator = HasNewtonsoftJson,
                Install = () =>
                {
                    PackageInstaller.Install("com.unity.nuget.newtonsoft-json");
                }
            }
        };
        
        private static bool HasPackageAssembly(string assemblyName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Any(x => x.GetName().Name == assemblyName);
        }

        static FrameworkValidator()
        {
            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);

            EditorApplication.delayCall += () =>
            {
                if (!IsFrameworkReady())
                {
                    FrameworkValidatorWindow.ShowWindow();
                }
            };
        }

        #region Public

        public static bool IsFrameworkReady()
        {
            return Items.All(item => item.Validator());
        }

        public static int InstalledCount()
        {
            return Items.Count(item => item.Validator());
        }

        public static int TotalCount()
        {
            return Items.Count;
        }

        #endregion

        #region Validators

        private static bool HasOdin()
        {
            return Type.GetType(
                "Sirenix.OdinInspector.ButtonAttribute, Sirenix.OdinInspector.Attributes") != null;
        }

        private static bool HasDotween()
        {
            return HasPackageAssembly("DOTween");
        }

        /// <summary>
        /// Check whether DOTween Assembly Definition has been generated.
        /// </summary>
        private static bool HasNewtonsoftJson()
        {
            return HasPackageAssembly("Newtonsoft.Json");
        }
        
        private static bool HasNuGetForUnity()
        {
            return HasPackageAssembly("NuGetForUnity");
        }
        
        private static bool HasMemoryPack()
        {
            const string root = "Assets/Packages";

            if (!Directory.Exists(root))
                return false;

            return Directory.GetDirectories(root)
                .Any(dir => Path.GetFileName(dir).StartsWith("MemoryPack."));
        }

        private static bool HasUniTask()
        {
            return HasPackageAssembly("UniTask");
        }

        private static bool HasAddressables()
        {
            return HasPackageAssembly("Unity.Addressables");
        }

        private static bool HasDotweenAsmdef()
        {
            string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.EndsWith("DOTween.Modules.asmdef"))
                    return true;
            }

            return false;
        }

        private static bool HasUnitaskDotweenSupport()
        {
#if UNITY_2021_3_OR_NEWER

            var group = EditorUserBuildSettings.selectedBuildTargetGroup;

#pragma warning disable CS0618
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#pragma warning restore CS0618

            return symbols
                .Split(';')
                .Contains("UNITASK_DOTWEEN_SUPPORT");

#else
            return false;
#endif
        }

        #endregion

        #region Installers

        private static void OpenDotweenUtility()
        {
            EditorUtility.DisplayDialog(
                "DOTween Setup Required",
                "Please open:\n\n" +
                "Tools > Demigiant > DOTween Utility Panel\n\n" +
                "Then click:\n" +
                "• Setup DOTween...\n" +
                "or\n" +
                "• Generate ASMDEF",
                "OK");
        }

        private static void InstallUnitaskDotweenSupport()
        {
#if UNITY_2021_3_OR_NEWER

            var group = EditorUserBuildSettings.selectedBuildTargetGroup;

#pragma warning disable CS0618
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#pragma warning restore CS0618

            var list = symbols
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (list.Contains("UNITASK_DOTWEEN_SUPPORT"))
            {
                EditorUtility.DisplayDialog(
                    "Framework Validator",
                    "UNITASK_DOTWEEN_SUPPORT already exists.",
                    "OK");

                return;
            }

            list.Add("UNITASK_DOTWEEN_SUPPORT");

#pragma warning disable CS0618
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                group,
                string.Join(";", list));
#pragma warning restore CS0618

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Framework Validator",
                "UNITASK_DOTWEEN_SUPPORT has been added.\n\nUnity will recompile scripts.",
                "OK");

#endif
        }

        #endregion

        #region Menu

        [MenuItem("Tools/Game Template/Framework Validator", priority = 0)]
        public static void OpenWindow()
        {
            FrameworkValidatorWindow.ShowWindow();
        }

        #endregion
    }
}