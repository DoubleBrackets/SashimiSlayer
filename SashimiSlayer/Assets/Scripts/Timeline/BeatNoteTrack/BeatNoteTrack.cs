using System.ComponentModel;
using Beatmapping.Service;
using Timeline.BeatNoteTrack.BeatNote;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.BeatNoteTrack
{
    [TrackColor(1, 0, 0)]
    [TrackClipType(typeof(BeatNoteClip))]
    [TrackBindingType(typeof(BeatmapService))]
    [DisplayName("Beat Note Track")]
    public class BeatNoteTrack : TrackAsset
    {
        // Creates a runtime instance of the track, represented by a PlayableBehaviour.
        // The runtime instance performs mixing on the timeline clips.
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<BeatNoteMixer>.Create(graph, inputCount);
        }

        // Invoked by the timeline editor to put properties into preview mode. This permits the timeline
        // to temporarily change fields for the purpose of previewing in EditMode.
        // If not done properly, the properties will NOT be reverted back to their true values when exiting edit/preview mode
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var trackBinding = director.GetGenericBinding(this) as BeatmapService;

            if (trackBinding == null)
            {
                return;
            }

            // The field names are the name of the backing serializable field. These can be found from the class source,
            // or from the unity scene file that contains an object of that type.
            // driver.AddFromName<Light>(trackBinding.gameObject, "m_Color");

            base.GatherProperties(director, driver);
        }
    }
}