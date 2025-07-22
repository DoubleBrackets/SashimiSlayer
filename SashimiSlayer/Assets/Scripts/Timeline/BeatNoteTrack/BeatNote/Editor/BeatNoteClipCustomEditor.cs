using System.Collections.Generic;
using Beatmapping.Editor;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Timeline.BeatNoteTrack.BeatNote.Editor
{
    /// <summary>
    ///     Draws custom in-scene handles for the BeatNoteClip
    /// </summary>
    [CustomEditor(typeof(BeatNoteClip))]
    public class BeatNoteClipCustomEditor : UnityEditor.Editor
    {
        private static readonly List<Color> _handleColors = new()
        {
            Color.blue,
            new Color(1f, 0.73f, 0f),
            new Color(1f, 0f, 0.54f)
        };

        private GUIStyle _labelStyle;

        private void OnEnable()
        {
            _labelStyle = new GUIStyle
            {
                normal = { textColor = Color.black },
                fontStyle = FontStyle.Bold
            };
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView view)
        {
            var noteClip = target as BeatNoteClip;
            if (noteClip == null)
            {
                return;
            }

            BeatNoteBehavior noteData = noteClip.Template;
            if (noteData == null)
            {
                return;
            }

            float radius = noteData.NoteConfig.HitboxRadius;

            // Start pos handle
            Vector3 startPos = noteData.NoteData.StartPosition;
            Vector3 newStartPos = LabeledPositionHandle(startPos, Color.green, radius, "Start Pos");
            if (startPos != newStartPos)
            {
                Undo.RecordObject(noteClip, "Edited Timeline Note Clip Position");
                noteData.NoteData.StartPosition = newStartPos;
                OnPositionHandleChange();
            }

            Vector2 lineStart = newStartPos;

            // Interaction handles
            List<SequencedNoteInteraction> interactions = noteData.NoteData.Interactions;
            for (var i = 0; i < interactions.Count; i++)
            {
                SequencedNoteInteraction sequencedInteraction = interactions[i];
                NoteInteractionData interactionData = sequencedInteraction.InteractionData;
                Color c = _handleColors[i % _handleColors.Count];

                for (var j = 0; j < interactionData.Positions.Count; j++)
                {
                    Vector2 pos = interactionData.Positions[j];
                    Vector2 newPos =
                        LabeledPositionHandle(pos, c, radius, $"{interactionData.InteractionType}_{i}-{j}");
                    if (pos != newPos)
                    {
                        Undo.RecordObject(noteClip, "Edited Timeline Note Clip Position");
                        interactionData.Positions[j] = newPos;
                        sequencedInteraction.InteractionData = interactionData;
                        OnPositionHandleChange();
                    }

                    Handles.DrawLine(lineStart, newPos);
                    lineStart = newPos;
                }
            }

            // End pos handle
            Vector3 endPos = noteData.NoteData.EndPosition;
            Vector3 newEndPos = LabeledPositionHandle(endPos, Color.red, radius, "End Pos");
            if (endPos != newEndPos)
            {
                Undo.RecordObject(noteClip, "Edited Timeline Note Clip Position");
                noteData.NoteData.EndPosition = newEndPos;
                OnPositionHandleChange();
            }

            Handles.DrawLine(lineStart, newEndPos);
        }

        private void OnPositionHandleChange()
        {
            if (SashimiSlayerUtilWindow.AutoRefreshTimeline)
            {
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }

            // Mark timeline as dirty
            EditorUtility.SetDirty(target);

            Repaint();
        }

        private Vector2 LabeledPositionHandle(Vector2 pos, Color c, float radius, string label)
        {
            Handles.color = c;
            Vector2 newPos = Handles.PositionHandle(pos, Quaternion.identity);
            Handles.DrawWireDisc(newPos, Vector3.forward, radius);
            Handles.Label(newPos, label, _labelStyle);
            return newPos;
        }
    }
}