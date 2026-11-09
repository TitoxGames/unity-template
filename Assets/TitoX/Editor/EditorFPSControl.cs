#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WPuzzle;

namespace TitoX.Editor
{
    public class EditorFPSControl : EditorWindow
    {
        int _targetFPS = 60;
        float _timeScale = 1f;
        string _levelInput = "1";

        [MenuItem("Tools/Game Template/FPS Control")]
        static void OpenWindow()
        {
            var window = GetWindow<EditorFPSControl>("FPS Control");
            window.minSize = new Vector2(250, 140);
            window.maxSize = new Vector2(400, 140);
            window.ShowUtility();
        }

        void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        void OnPlayModeChanged(PlayModeStateChange state)
        {
            _targetFPS = 60;
            _timeScale = 1f;
            Application.targetFrameRate = 60;
            Time.timeScale = 1f;
            Repaint();
        }

        void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target FPS", GUILayout.Width(70));
            _targetFPS = (int)EditorGUILayout.Slider(_targetFPS, 5, 120);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time Scale", GUILayout.Width(70));
            _timeScale = EditorGUILayout.Slider(_timeScale, 0f, 2f);
            EditorGUILayout.EndHorizontal();

            if (EditorApplication.isPlaying)
            {
                Application.targetFrameRate = _targetFPS;
                Time.timeScale = _timeScale;
            }

            EditorGUILayout.Space(4);
        
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Change Level", GUILayout.Width(85));
            _levelInput = EditorGUILayout.TextField(_levelInput, GUILayout.Width(50));
        
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Go", GUILayout.Width(40)))
            {
                if (int.TryParse(_levelInput, out int levelInt) && levelInt > 0)
                {
                    LevelManager.Instance.LoadLevelByNumber(levelInt);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.Space(4);
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to control FPS and change levels.", MessageType.Info);
            }
        }
    }
}
#endif
