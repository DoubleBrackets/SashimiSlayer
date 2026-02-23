using Beatmapping;
using Core.Protag.Core;

namespace Framework
{
    public static class ExtensionMethods
    {
        public static SlicePresentationTypes ToSlicePresentationType(this TimingWindow.TimingResult result)
        {
            if (result.IsPerfect())
            {
                return SlicePresentationTypes.Perfect;
            }

            if (result.IsEarly())
            {
                return SlicePresentationTypes.Early;
            }

            if (result.IsLate())
            {
                return SlicePresentationTypes.Late;
            }

            return SlicePresentationTypes.Miss;
        }

        public static BlockPresentationTypes ToBlockPresentationType(this TimingWindow.TimingResult result)
        {
            if (result.IsPerfect())
            {
                return BlockPresentationTypes.Perfect;
            }

            if (result.IsEarly())
            {
                return BlockPresentationTypes.Early;
            }

            if (result.IsLate())
            {
                return BlockPresentationTypes.Late;
            }

            return BlockPresentationTypes.Miss;
        }
    }
}