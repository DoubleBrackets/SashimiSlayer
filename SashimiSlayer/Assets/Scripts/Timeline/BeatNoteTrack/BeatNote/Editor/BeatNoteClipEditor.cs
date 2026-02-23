using System;
using System.Collections.Generic;
using System.Linq;
using Beatmapping;
using Beatmapping.NoteInteraction.DataTypes;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using CommonTypes;
using DevTools.Editor.SashimiSlayer;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Timeline.BeatNoteTrack.BeatNote.Editor
{
    /// <summary>
    ///     Draws custom indicators in the timeline editor for notes
    /// </summary>
    [CustomTimelineEditor(typeof(BeatNoteClip))]
    public class BeatNoteClipEditor : ClipEditor
    {
        public override void OnClipChanged(TimelineClip clip)
        {
            BeatmapConfigSo beatmapConfig = SashimiSlayerToolsWindow.CurrentEditingBeatmap;

            if (beatmapConfig == null)
            {
                return;
            }

            // Clip shouldn't start before the beatmap
            clip.start = Math.Max(clip.start, beatmapConfig.StartTime);

            SnapClipToBPM(clip, beatmapConfig);

            var clipAsset = clip.asset as BeatNoteClip;
            if (clipAsset == null)
            {
                return;
            }

            BeatNoteBehavior template = clipAsset.Template;

            // Update the internal note data to match the clip
            template.NoteData.NoteStartTime = clip.start - beatmapConfig.StartTime;
            template.NoteData.NoteEndTime = clip.end - beatmapConfig.StartTime;
            template.NoteData.NoteBeatCount = clip.duration * beatmapConfig.Bpm / 60;

            EnforceInteractionUsage(template);
        }

        private void EnforceInteractionUsage(BeatNoteBehavior template)
        {
            List<INoteInteractionUser.InteractionUsage> interactionUsages =
                template.NoteConfig.Prefab.GetInteractionUsages().ToList();

            List<SequencedNoteInteraction> sequencedInteractions = template.NoteData.Interactions;

            int requiredNumInteractions =
                interactionUsages.Count > 0 ? interactionUsages.Max(a => a.InteractionIndex) + 1 : 0;

            // Add interactions if needed
            if (sequencedInteractions.Count < requiredNumInteractions)
            {
                sequencedInteractions.AddRange(Enumerable.Repeat(new SequencedNoteInteraction(),
                    requiredNumInteractions - sequencedInteractions.Count));
            }

            // Validate each interaction
            foreach (INoteInteractionUser.InteractionUsage usage in interactionUsages)
            {
                int interactionIndex = usage.InteractionIndex;

                SequencedNoteInteraction sequencedInteraction = sequencedInteractions[interactionIndex];
                NoteInteractionData interactionData = sequencedInteraction.InteractionData;

                interactionData.NoteInteractionType = usage.NoteInteractionType;

                int numPositions = usage.PositionCount;

                if (interactionData.Positions == null)
                {
                    interactionData.Positions = new List<Vector2>();
                }

                // Add or Remove positions if needed
                if (interactionData.Positions.Count < numPositions)
                {
                    interactionData.Positions.AddRange(
                        Enumerable.Repeat(Vector2.zero, numPositions - interactionData.Positions.Count));
                }
                else if (interactionData.Positions.Count > numPositions)
                {
                    interactionData.Positions.RemoveRange(numPositions, interactionData.Positions.Count - numPositions);
                }

                // Kinda awkward to copy around structs...
                sequencedInteraction.InteractionData = interactionData;
                sequencedInteractions[interactionIndex] = sequencedInteraction;
            }
        }

        private void SnapClipToBPM(TimelineClip clip, BeatmapConfigSo beatmapConfig)
        {
            double start = clip.start;
            double duration = clip.duration;

            clip.start = beatmapConfig.QuantizeTime(start);
            clip.duration = beatmapConfig.QuantizeTime(start + duration) - clip.start;
        }

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var actionClip = clip.asset as BeatNoteClip;

            if (actionClip == null)
            {
                return;
            }

            BeatmapConfigSo beatmapConfig = SashimiSlayerToolsWindow.CurrentEditingBeatmap;

            if (beatmapConfig == null)
            {
                return;
            }

            // We assume clip.Start time is properly snapped to the interval

            DrawInteractions(beatmapConfig, actionClip, region);

            base.DrawBackground(clip, region);
        }

        private void DrawInteractions(BeatmapConfigSo beatmap, BeatNoteClip noteClip, ClipBackgroundRegion region)
        {
            double noteBeatLength = noteClip.Template.NoteData.NoteBeatCount;
            foreach (SequencedNoteInteraction sequencedInteraction in noteClip.Template.NoteData.Interactions)
            {
                double interactionStartTime =
                    sequencedInteraction.GetBeatsFromNoteStart(noteBeatLength) * 60 / beatmap.Bpm;

                float normalizedPos = Mathf.InverseLerp(
                    (float)region.startTime,
                    (float)region.endTime,
                    (float)interactionStartTime);

                NoteInteractionData interactionData = sequencedInteraction.InteractionData;

                // Add 0.01f for precision errors(?)
                if (interactionStartTime >= region.startTime && interactionStartTime <= region.endTime + 0.01f)
                {
                    if (interactionData.NoteInteractionType == NoteInteractionType.Block)
                    {
                        DrawAttackInteraction(interactionData, region, normalizedPos);
                    }
                    else if (interactionData.NoteInteractionType == NoteInteractionType.Slice)
                    {
                        DrawVulnerableInteraction(region, normalizedPos);
                    }
                }
            }
        }

        private void DrawAttackInteraction(
            NoteInteractionData interactionData,
            ClipBackgroundRegion region,
            float normalizedPos)
        {
            int posePositionCount = Enum.GetValues(typeof(BlockPoses)).Length;

            // Draw a vertical line at the time of the attack
            Rect linePos = region.position;
            linePos.x += normalizedPos * linePos.width;
            linePos.width = 2;

            EditorGUI.DrawRect(linePos, Color.red);

            // Draw a series of lines to indicate the block poses
            linePos.height = 5;
            linePos.width = 5;
            linePos.x -= 2;

            var pose = (int)interactionData.BlockPose;

            var vertMargin = 5;
            linePos.y = vertMargin;

            float heightOffsetPerTick = (region.position.height - vertMargin * 2) / (posePositionCount - 1);

            for (var i = 0; i < posePositionCount; i++)
            {
                if (i == pose)
                {
                    EditorGUI.DrawRect(linePos, Color.cyan);
                }
                else
                {
                    EditorGUI.DrawRect(linePos, Color.gray);
                }

                linePos.y += heightOffsetPerTick;
            }
        }

        private void DrawVulnerableInteraction(ClipBackgroundRegion region, float normalizedPos)
        {
            Rect linePos = region.position;
            linePos.x += normalizedPos * linePos.width;
            linePos.width = 2;
            linePos.x = Mathf.Clamp(linePos.x, region.position.x, region.position.x + region.position.width - 2);

            EditorGUI.DrawRect(linePos, Color.yellow);

            Rect crossPos = linePos;
            crossPos.width = 5;
            crossPos.x -= 2;
            crossPos.y = linePos.height / 2 - 2;
            crossPos.height = 5;
            EditorGUI.DrawRect(crossPos, Color.yellow);
        }
    }
}