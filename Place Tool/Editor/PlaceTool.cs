using UnityEditor;
using UnityEngine;

public class PlaceTool : EditorWindow {
    private GameObject prefabToSpawn;
    private float gridSnapSize = 1f;
    private bool isBuilderActive = false;

    // Добавляет пункт в верхнее системное меню Unity
    [MenuItem("Tools/Dungeon Window Tool")]
    public static void ShowWindow() {
        // Открывает или фокусирует существующее окно
        GetWindow<PlaceTool>("Place tool");
    }

    // 1. Отрисовка интерфейса внутри самого ОКНА
    private void OnGUI() {
        GUILayout.Label("Настройки строителя", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Поле выбора префаба
        prefabToSpawn = (GameObject)EditorGUILayout.ObjectField("Prefab to Spawn", prefabToSpawn, typeof(GameObject), false);

        // Поле ввода сетки
        gridSnapSize = EditorGUILayout.FloatField("Grid Snap Size", gridSnapSize);
        gridSnapSize = Mathf.Max(0.1f, gridSnapSize);

        GUILayout.Space(10);

        // Кнопка включения/выключения режима рисования
        GUI.backgroundColor = isBuilderActive ? Color.green : Color.red;
        if (GUILayout.Button(isBuilderActive ? "РЕЖИМ СТРОИТЕЛЯ: АКТИВЕН" : "РЕЖИМ СТРОИТЕЛЯ: ВЫКЛЮЧЕН", GUILayout.Height(30))) {
            isBuilderActive = !isBuilderActive;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);
        EditorGUILayout.HelpBox("Инструкция: Включите режим, выберите префаб и кликайте ЛКМ в окне Сцены для спавна объекта.", MessageType.Info);
    }

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
  
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    // 2. Логика отслеживания мыши внутри Scene View
    private void OnSceneGUI(SceneView sceneView) {
        if (!isBuilderActive) return;

        // Блокируем выделение стандартных объектов Unity
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event currentEvent = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter)) {
            Vector3 hitPoint = ray.GetPoint(enter);

            // Сетка
            float snappedX = Mathf.Round(hitPoint.x / gridSnapSize) * gridSnapSize;
            float snappedZ = Mathf.Round(hitPoint.z / gridSnapSize) * gridSnapSize;
            Vector3 spawnPosition = new Vector3(snappedX, 0f, snappedZ);

            // Рисуем рамку
            Handles.color = prefabToSpawn != null ? Color.green : Color.red;
            Handles.DrawWireCube(spawnPosition + Vector3.up * 0.5f, Vector3.one * gridSnapSize);

            // Клик мышкой
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0) {
                if (prefabToSpawn != null) {
                    Undo.IncrementCurrentGroup();
                    GameObject spawnedObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                    spawnedObj.transform.position = spawnPosition;

                    Undo.RegisterCreatedObjectUndo(spawnedObj, "Spawn Room Element");
                    currentEvent.Use();
                }
            }
        }

        // Перерисовываем сцену для плавной рамки
        sceneView.Repaint();
    }
}