using System.Collections.Generic;

namespace AsImpL
{
    /// <summary>
    /// Loading progress information for a single OBJ loader
    /// </summary>
    public class FileLoadingProgress
    {
        public string fileName;
        public string message;
        public float percentage = 0;
        // TODO: split percentages, e.g.: public float parsingPercentage = 0;
        public int numObjects = 0;
        public int numSubObjects = 0;
        public bool error = false;
    }

    /// <summary>
    /// Overall loading progress for all the active OBJ loaders (list of ObjLoadingProgress).
    /// See <see cref="ObjLoadingProgress"/>.
    /// </summary>
    public class LoadingProgress
    {
        public List<FileLoadingProgress> fileProgress = new List<FileLoadingProgress>();
    }
}
