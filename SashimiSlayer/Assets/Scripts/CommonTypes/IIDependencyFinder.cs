namespace CommonTypes
{
    /// <summary>
    ///     Type that needs to find (some) of its own dependencies.
    ///     Intended mainly for monobehaviours that are instantiated with the scene
    ///     TODO: Is this really needed...
    /// </summary>
    public interface IFindsDependencies
    {
        /// <summary>
        ///     Find the dependencies needed for this object to function.
        ///     Ideally, the object should be ready to use after this is called
        /// </summary>
        public void FindDependencies();
    }
}