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
        public class CustomObjImporter : ObjectImporter
        {
            [Tooltip("Models to load on startup")]
            public List<ModelImportInfo> objectList = new List<ModelImportInfo>();

            [Tooltip("Default scale for models loaded")]
            public float defaultScale = 0.01f;

            [Tooltip("Default vertical axis for models\ntrue = Z axis, false = Y axis")]
            public bool defaultZUp = false;

            [Tooltip("Text fordisplaying the overall scale")]
            public Text objScalingText;

            [Tooltip("Configuration XML file (relative to the application data folder)")]
            public string configFile = "../object_list.xml";

            private List<ModelImportInfo> modelsToImport = new List<ModelImportInfo>();

            /// <summary>
            /// Load a set of files with their own import options
            /// </summary>
            /// <param name="modelsInfo">List of file import entries</param>
            public void ImportModelListAsync(ModelImportInfo[] modelsInfo)
            {
                if (modelsInfo == null)
                {
                    return;
                }
                for (int i = 0; i < modelsInfo.Length; i++)
                {
                    if (modelsInfo[i].skip) continue;
                    string objName = modelsInfo[i].name;
                    string filePath = modelsInfo[i].path;
                    if (string.IsNullOrEmpty(filePath))
                    {
                        Debug.LogWarning("File path missing");
                        continue;
                    }
#if (UNITY_ANDROID || UNITY_IPHONE)
                    filePath = Application.persistentDataPath + "/" + filePath;
#endif
                    ImportOptions options = modelsInfo[i].loaderOptions;
                    if (options == null)
                    {
                        options = new ImportOptions();
                        options.modelScaling = defaultScale;
                        options.zUp = defaultZUp;
                    }
                    options.reuseLoaded = true;
                    ImportModelAsync(objName, filePath, transform, options);
                }
            }

            private void Awake()
            {
#if (UNITY_ANDROID || UNITY_IPHONE)
                configFile = Application.persistentDataPath + "/" + configFile;
#endif
                ////configFile = Application.dataPath + "/" + configFile;
                //configFile = Application.streamingAssetsPath + "/" + configFile;
            }

            private void Start()
            {
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
                {
                    Reload();
                }
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
                stream.Close();
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
                    stream.Close();
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
