using System.Collections.Generic;

namespace Beatmapping.Data
{
    public struct BeatmapTickFinalResults
    {
        public IList<NoteTickedResults> NoteTickedResults;
        public bool BeatmapOver;
    }
}