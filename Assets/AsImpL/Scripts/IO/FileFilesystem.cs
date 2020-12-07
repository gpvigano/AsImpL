using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AsImpL
{
    /// <summary>
    /// Filesystem implementation that uses the standard .NET File class.
    /// </summary>
    public class FileFilesystem : IFilesystem
    {
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public FileStream OpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public IEnumerator DownloadUri(string uri, bool notifyErrors)
        {
#if UNITY_2018_3_OR_NEWER
            UnityWebRequest uwr = UnityWebRequest.Get(uri);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                if (notifyErrors)
                {
                    Debug.LogError(uwr.error);
                }

                yield return null;
            }
            else
            {
                // Get downloaded asset bundle
                yield return uwr.downloadHandler.text;
            }
#else
            WWW www = new WWW(uri);
            yield return www;
            if (www.error != null)
            {
                if (notifyErrors)
                {
                    Debug.LogError("Error loading " + uri + "\n" + www.error);
                }

                yield return null;
            }
            else
            {
                yield return www.text;
            }
#endif
        }
    }
}