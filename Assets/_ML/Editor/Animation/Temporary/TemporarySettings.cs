#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Animancer.Editor;

namespace ML.Editor.Animation
{
    internal class TemporarySettings : ScriptableObject
    {
        #region Instance
        private static TemporarySettings _Instance;

        private static TemporarySettings Instance
        {
            get
            {
                if (_Instance == null)
                {
                    var instances = Resources.FindObjectsOfTypeAll<TemporarySettings>();
                    if (instances.Length > 0)
                    {
                        _Instance = instances[0];
                    }
                    else
                    {
                        _Instance = CreateInstance<TemporarySettings>();
                        _Instance.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
                    }
                }

                return _Instance;
            }
        }

        protected virtual void OnEnable()
        {
            OnEnableSelection();
        }

        protected virtual void OnDisable()
        {
            OnDisableSelection();
        }
        #endregion

        #region Event Selection

        private readonly Dictionary<Object, Dictionary<string, int>>
            ObjectToPropertyPathToSelectedEvent = new Dictionary<Object, Dictionary<string, int>>();

        public static int GetSelectedEvent(SerializedProperty property)
        {
            var instance = Instance;
            if (!instance.ObjectToPropertyPathToSelectedEvent.TryGetValue(property.serializedObject.targetObject, out var pathToSelection))
                return -1;
            else if (pathToSelection.TryGetValue(property.propertyPath, out var selection))
                return selection;
            else
                return -1;
        }

        public static void SetSelectedEvent(SerializedProperty property, int eventIndex)
        {
            var pathToSelection = GetOrCreatePathToSelection(property.serializedObject.targetObject);
            if (eventIndex >= 0)
                pathToSelection[property.propertyPath] = eventIndex;
            else
                pathToSelection.Remove(property.propertyPath);
        }

        private static Dictionary<string, int> GetOrCreatePathToSelection(Object obj)
        {
            var instance = Instance;
            if (!instance.ObjectToPropertyPathToSelectedEvent.TryGetValue(obj, out var pathToSelection))
            {
                instance.ObjectToPropertyPathToSelectedEvent.Add(obj,
                    pathToSelection = new Dictionary<string, int>());
            }

            return pathToSelection;
        }

        [SerializeField] private Animancer.Editor.Serialization.ObjectReference[] _EventSelectionObjects;
        [SerializeField] private string[] _EventSelectionPropertyPaths;
        [SerializeField] private int[] _EventSelectionIndices;

        private void OnDisableSelection()
        {
            var objects = new List<Animancer.Editor.Serialization.ObjectReference>();
            var paths = new List<string>();
            var indices = new List<int>();

            foreach (var objectToSelection in ObjectToPropertyPathToSelectedEvent)
            {
                foreach (var pathToSelection in objectToSelection.Value)
                {
                    objects.Add(objectToSelection.Key);
                    paths.Add(pathToSelection.Key);
                    indices.Add(pathToSelection.Value);
                }
            }

            _EventSelectionObjects = objects.ToArray();
            _EventSelectionPropertyPaths = paths.ToArray();
            _EventSelectionIndices = indices.ToArray();
        }
        private void OnEnableSelection()
        {
            if (_EventSelectionObjects == null ||
                _EventSelectionPropertyPaths == null ||
                _EventSelectionIndices == null)
                return;

            var count = _EventSelectionObjects.Length;
            if (count > _EventSelectionPropertyPaths.Length)
                count = _EventSelectionPropertyPaths.Length;
            if (count > _EventSelectionIndices.Length)
                count = _EventSelectionIndices.Length;

            for (int i = 0; i < count; i++)
            {
                var obj = _EventSelectionObjects[i];
                if (obj.IsValid())
                {
                    var pathToSelection = GetOrCreatePathToSelection(obj);
                    pathToSelection.Add(_EventSelectionPropertyPaths[i], _EventSelectionIndices[i]);
                }
            }
        }
        #endregion

        #region Preview Models
        [SerializeField]
        private List<GameObject> _PreviewModels;
        public static List<GameObject> PreviewModels
        {
            get
            {
                var instance = Instance;
                Animancer.Editor.AnimancerEditorUtilities.RemoveMissingAndDuplicates(ref instance._PreviewModels);
                return instance._PreviewModels;
            }
        }
        #endregion
    }
}

#endif
