using System.Collections;
using System.IO;

namespace AsImpL
{
    /// <summary>
    /// Interface for classes that access the filesystem.
    /// </summary>
    public interface IFilesystem
    {
        /// <summary>
        /// Reads all data from the specified path into a byte array.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        byte[] ReadAllBytes(string path);

        /// <summary>
        /// Reads all lines in the specified file into an array of strings.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string[] ReadAllLines(string path);

        /// <summary>
        /// Opens the file at the specified path for reading.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        FileStream OpenRead(string path);

        /// <summary>
        /// Downloads the specified URI.
        /// </summary>
        /// <param name="uri">The URI to download.</param>
        /// <param name="notifyErrors">Whether to notify of errors.</param>
        /// <returns>An enumerator usable as Coroutine in Unity.</returns>
        IEnumerator DownloadUri(string uri, bool notifyErrors);

        /// <summary>
        /// Downloads the specified URI as texture.
        /// </summary>
        /// <param name="uri">The URI to download.</param>
        /// <returns>An enumerator usable as Coroutine in Unity.</returns>
        IEnumerator DownloadTexture(string uri);
    }
}