using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace AsImpL
{
    /// <summary>
    /// Examples for demonstrating AsImpL features.
    /// </summary>
    namespace Examples
    {
        /// <summary>
        /// Read a configuration file and load the object listed there with their parameters and positions.
        /// </summary>
        public class CustomObjImporter : ObjImporterTest
        {
            [Tooltip("Text for displaying the overall scale")]
            public Text objScalingText;

            [Tooltip("Configuration XML file (relative to the application data folder)")]
            public string configFile = "../object_list.xml";

            private List<ModelImportInfo> modelsToImport = new List<ModelImportInfo>();

            private void Awake()
            {
#if (UNITY_ANDROID || UNITY_IPHONE)
                configFile = Application.persistentDataPath + "/" + configFile;
#endif
                ////configFile = Application.dataPath + "/" + configFile;
                //configFile = Application.streamingAssetsPath + "/" + configFile;
            }

            protected override void Start()
            {
#if UNITY_STANDALONE
#if !UNITY_EDITOR
                string[] args = Environment.GetCommandLineArgs();

                if (args != null && args.Length > 1)
                {
                    int numImports = args.Length - 1;
                    for (int i = 0; i < numImports; i++)
                    {
                        if (args[i + 1].StartsWith("-"))
                        {
                            if (args[i + 1] == "-scale")
                            {
                                if (i + 1 < numImports)
                                {
                                    float.TryParse(args[i + 2], out defaultScale);
                                }
                                i++;
                            }
                            continue;
                        }
                        ModelImportInfo modelToImport = new ModelImportInfo();
                        modelToImport.path = args[i + 1];
                        modelToImport.name = Path.GetFileNameWithoutExtension(modelToImport.path);
                        modelToImport.loaderOptions = new ImportOptions();
                        modelToImport.loaderOptions.modelScaling = defaultScale;
                        modelToImport.loaderOptions.zUp = defaultZUp;
                        modelToImport.loaderOptions.reuseLoaded = false;
                        objectList.Add(modelToImport);
                    }
                    configFile = "";

                    ImportModelListAsync(objectList.ToArray());
                }
                else
#endif
                {
                    Reload();
                }
#else
                Debug.Log("Command line arguments not available, using default settings.");
                Reload();
#endif
            }

            public void SetScaling(float scl)
            {
                scl = Mathf.Pow(10.0f, scl);
                objScalingText.text = "Scaling: " + scl;
                transform.localScale = new Vector3(scl, scl, scl);
            }

            protected override void OnImportingComplete()
            {
                base.OnImportingComplete();
                UpdateScene();
            }

            public void Save()
            {
                if (!allLoaded || string.IsNullOrEmpty(configFile))
                {
                    return;
                }

                UpdateObjectList();
                XmlSerializer serializer = new XmlSerializer(objectList.GetType());
                FileStream stream = new FileStream(configFile, FileMode.Create);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                settings.Encoding = Encoding.UTF8;
                settings.CheckCharacters = true;

                XmlWriter w = XmlWriter.Create(stream, settings);

                serializer.Serialize(w, objectList);
                stream.Dispose();
            }

            public void Reload()
            {
                if (string.IsNullOrEmpty(configFile))
                {
                    return;
                }

                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<ModelImportInfo>));
                    FileStream stream = new FileStream(configFile, FileMode.Open);
                    objectList = (List<ModelImportInfo>)serializer.Deserialize(stream);
                    stream.Dispose();
                    UpdateScene();
                    ImportModelListAsync(modelsToImport.ToArray());
                }
                catch (IOException e)
                {
                    Debug.LogWarningFormat("Unable to open configuration file {0}: {1}", configFile, e);
                }
            }

            private void UpdateObjectList()
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform tr = transform.GetChild(i);
                    ModelImportInfo info = objectList.Find(obj => obj.name == tr.name);
                    if (info != null)
                    {
                        info.UpdateFrom(tr.gameObject);
                    }
                }
            }

            private void UpdateScene()
            {
                modelsToImport.Clear();
                List<string> names = new List<string>();
                // add or update objects that are present in the list
                foreach (ModelImportInfo info in objectList)
                {
                    names.Add(info.name);
                    Transform t = transform.Find(info.name);
                    if (t == null)
                    {
                        modelsToImport.Add(info);
                    }
                    else
                    {
                        info.ApplyTo(t.gameObject);
                    }
                }
                // destroy objects that are not present in the list
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform tr = transform.GetChild(i);
                    if (tr.gameObject != gameObject && !names.Contains(tr.gameObject.name))
                    {
                        Destroy(tr.gameObject);
                    }
                }
            }
        }
    }
}
