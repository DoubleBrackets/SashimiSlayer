namespace Core.Protag
{
    public struct SliceResultData
    {
        public enum SlicedObject
        {
            Note,
            MenuItem
        }

        /// <summary>
        ///     Number of objects that were sliced.
        /// </summary>
        public int SliceCount;

        public SlicedObject SlicedObjectType;

        public SliceResultData(int sliceCount, SlicedObject slicedObjectType)
        {
            SliceCount = sliceCount;
            SlicedObjectType = slicedObjectType;
        }
    }
}