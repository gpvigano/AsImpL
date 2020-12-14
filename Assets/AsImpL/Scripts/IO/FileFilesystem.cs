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

        public IEnumerator DownloadTexture(string uri)
        {
#if UNITY_2018_3_OR_NEWER
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
            {
                yield return uwr.SendWebRequest();

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.LogError(uwr.error);
                    yield return null;
                }
                else
                {
                    // Get downloaded asset bundle
                    yield return DownloadHandlerTexture.GetContent(uwr);
                }
            }
#else
            WWW loader = new WWW(uri);
            yield return loader;

            if (loader.error != null)
            {
                Debug.LogError(loader.error);
                yield return null;
            }
            else
            {
                yield return LoadTexture(loader);
            }
#endif
        }

#if !UNITY_2018_3_OR_NEWER
        /// <summary>
        /// Load a texture from the URL got from the parameter.
        /// </summary>
        private Texture2D LoadTexture(WWW loader)
        {
            string ext = Path.GetExtension(loader.url).ToLower();
            Texture2D tex = null;

            // TODO: add support for more formats (bmp, gif, dds, ...)
            if (ext == ".tga")
            {
                tex = TextureLoader.LoadTextureFromUrl(loader.url);
                //tex = TgaLoader.LoadTGA(new MemoryStream(loader.bytes));
            }
            else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                tex = loader.texture;
            }
            else
            {
                Debug.LogWarning("Unsupported texture format: " + ext);
            }

            if (tex == null)
            {
                Debug.LogErrorFormat("Failed to load texture {0}", loader.url);
            }
            else
            {
                //tex.alphaIsTransparency = true;
                //tex.filterMode = FilterMode.Trilinear;
            }

            return tex;
        }
#endif
    }
}