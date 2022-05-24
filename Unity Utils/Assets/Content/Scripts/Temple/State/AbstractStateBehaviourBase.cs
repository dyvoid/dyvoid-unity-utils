using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Temple.State
{
    public abstract class AbstractStateBehaviourBase : MonoBehaviour
    {
        public abstract string StateName { get; }

        public bool IsActive { get; protected set; }

        public abstract void ChangeToThisState();

        public virtual void OnEnter()
        {
            IsActive = true;
        }

        public virtual void OnExit()
        {
            IsActive = false;
        }

        public virtual void OnUpdateState()
        {

        }

        public virtual void OnFixedUpdateState()
        {

        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AbstractStateBehaviourBase), true)]
    public class StateBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!(target is AbstractStateBehaviourBase stateHandler)) return;

            if (!Application.isPlaying)
            {
                GUILayout.Space(10);
                DrawStateLabel(stateHandler, false);
                return;
            }

            GUILayout.Space(10);

            DrawStateLabel(stateHandler, stateHandler.IsActive);

            GUI.enabled = !stateHandler.IsActive;
            GUILayout.Space(10);

            DrawStateSwitchButton(stateHandler);
        }

        private static void DrawStateLabel(AbstractStateBehaviourBase stateBehaviour, bool isActiveState)
        {
            var contentColor = GUI.contentColor;

            if (Application.isPlaying)
            {
                GUI.contentColor = isActiveState ? Color.green : new Color(1, 0.5f, 0);
            }

            GUILayout.Label($"Active on: {stateBehaviour.StateName}");
            GUI.contentColor = contentColor;
        }

        private static void DrawStateSwitchButton(AbstractStateBehaviourBase stateBehaviour)
        {
            if (GUILayout.Button("Switch to State", GUILayout.Height(20), GUILayout.Width(120)))
            {
                stateBehaviour.ChangeToThisState();
            }
        }

    }
#endif
}