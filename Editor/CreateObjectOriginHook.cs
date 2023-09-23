using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NKStudio
{
    [InitializeOnLoad]
    public static class CreateObjectOriginHook
    {
        private static bool s_triggerOrigin;
        private static readonly Type HierarchyWindowType;

        static CreateObjectOriginHook()
        {
            ObjectChangeEvents.changesPublished += ChangesPublished;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            Assembly editorAssembly = typeof(EditorWindow).Assembly;
            HierarchyWindowType = editorAssembly.GetType("UnityEditor.SceneHierarchyWindow");
        }

        // 쉬프트 키를 눌렀는지 감지합니다.
        private static void HierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
        {
            EditorWindow currentWindow = EditorWindow.mouseOverWindow;
            if (currentWindow && currentWindow.GetType() == HierarchyWindowType)
            {
                Event current = Event.current;

                if (current != null)
                    s_triggerOrigin = current.shift;
            }
        }

        // Event.current는 GUI 이벤트에서만 동작합니다.
        // 그래서 EditorApplication.hierarchyWindowItemOnGUI를 통해 쉬프트를 눌렀는지를 체크하여 여기서 사용합니다.
        static void ChangesPublished(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; ++i)
            {
                ObjectChangeKind type = stream.GetEventType(i);
                switch (type)
                {
                    case ObjectChangeKind.CreateGameObjectHierarchy:
                        stream.GetCreateGameObjectHierarchyEvent(i, out var createGameObjectHierarchyEvent);
                        GameObject newGameObject =
                            EditorUtility.InstanceIDToObject(createGameObjectHierarchyEvent.instanceId) as GameObject;

                        if (s_triggerOrigin)
                            if (newGameObject != null)
                                newGameObject.transform.position = Vector3.zero;

                        break;
                }
            }
        }
    }
}