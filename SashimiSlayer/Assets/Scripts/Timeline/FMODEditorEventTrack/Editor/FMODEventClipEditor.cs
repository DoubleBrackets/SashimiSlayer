using System;
using Beatmapping;
using DevTools.Editor.SashimiSlayer;
using FMODUnity;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Timeline.FMODEditorEventTrack.Editor
{
    [CustomTimelineEditor(typeof(FMODEventPlayable))]
    public class FMODEventClipEditor : ClipEditor
    {
        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            BeatmapConfigSo beatmapConfig = SashimiSlayerToolsWindow.CurrentEditingBeatmap;

            if (beatmapConfig == null)
            {
                return;
            }

            // Assume the clip is snapped to subdivisions
            DrawBeatGuides(beatmapConfig, clip, region);

            base.DrawBackground(clip, region);
        }

        private void DrawBeatGuides(BeatmapConfigSo beatmap, TimelineClip clip, ClipBackgroundRegion region)
        {
            // region.startTime is the time of the visible area RELATIVE to the start of the ENTIRE clip
            // Calculate the corresponding beatmap timespace times
            double clipStartTime = clip.start - beatmap.StartTime;
            double drawnAreaStartTime = clipStartTime + region.startTime;
            double drawnAreaEndTime = clipStartTime + region.endTime;

            // Draw lines on every measure
            var subdivisionInterval = (float)(60 /
                                              beatmap.Bpm /
                                              beatmap.Subdivisions);

            int subdivsPerMeasure = beatmap.BeatsPerMeasure *
                                    beatmap.Subdivisions;

            double measureInterval = subdivisionInterval * subdivsPerMeasure;

            // Find the enclosing bounds, exclusive
            var startSubdiv = (int)Math.Ceiling(drawnAreaStartTime / subdivisionInterval);
            var endSubdiv = (int)Math.Floor(drawnAreaEndTime / subdivisionInterval);

            for (int i = startSubdiv; i <= endSubdiv; i++)
            {
                // time to place the marking, in beatmap timespace
                float markingTime = i * subdivisionInterval;

                // time to place the marking, drawn region timespace
                float markingClipTime = markingTime - (float)clipStartTime;

                float normalizedBeatTime =
                    Mathf.InverseLerp((float)region.startTime, (float)region.endTime, markingClipTime);

                Rect linePos = region.position;
                linePos.x += normalizedBeatTime * linePos.width;

                Rect numberPos = linePos;
                numberPos.x += 2;
                numberPos.y += 5;

                linePos.width = 1;
                if (i % subdivsPerMeasure == 0)
                {
                    // Start of a measure
                    EditorGUI.DrawRect(linePos, Color.white);
                    // Draw measure number
                    // Add one, since measures are 1-indexed (in FMOD at least)
                    int measureNumber = (int)Math.Round((markingTime + beatmap.StartTime) / measureInterval) + 1;
                    GUI.Label(numberPos,
                        measureNumber.ToString());
                }
                else if (i % beatmap.Subdivisions == 0)
                {
                    // Start of a beat
                    linePos.height /= 2;
                    EditorGUI.DrawRect(linePos, Color.white);
                }
                else
                {
                    // Start of a subdiv
                    linePos.height = 2;
                    EditorGUI.DrawRect(linePos, Color.gray);
                }
            }
        }
    }
}