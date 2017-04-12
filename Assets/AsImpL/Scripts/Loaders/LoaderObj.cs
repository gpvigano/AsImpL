using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
#if UNITY_EDITOR
#endif

namespace AsImpL
{
    /// <summary>
    /// Class for loading OBJ files into Unity scene at run-time and in editor mode.
    /// </summary>
    /// <remarks>
    /// Partially derived from "Runtime OBJ Loader"
    /// (http://forum.unity3d.com/threads/free-runtime-obj-loader.365884/)
    /// and from "runtime .OBJ file loader for Unity3D"
    /// (https://github.com/hammmm/unity-obj-loader) and 
    /// (https://github.com/cmdr2/unity-remote-obj-loader)
    /// 
    /// New features:
    /// <list type="bullet">
    /// <item><description>meshes with more than 65K vertices/indices are splitted and loaded</description></item>
    /// <item><description>groups are loaded into game (sub) objects</description></item>
    /// <item><description>extended material support</description></item>
    /// <item><description>computation of normal maps and tangents</description></item>
    /// <item><description>computation of albedo texture from diffuse and opacity textures</description></item>
    /// <item><description>progressive loading</description></item>
    /// <item><description>reusing data for multiple objects</description></item>
    /// <item><description>create a loader for each model for parallel loading</description></item>
    /// <item><description>support for asset import</description></item>
    /// </list>
    /// <seealso cref="DataSet"/>
    /// <seealso cref="MaterialData"/>
    /// <seealso cref="ObjectBuilder"/>
    /// </remarks>
    public class LoaderObj : Loader
    {
        private string mtlLib;

        /// <summary>
        /// Parse dependencies of the given OBJ file.
        /// </summary>
        /// <param name="absolutePath">absolute file path</param>
        /// <returns>The list of dependencies (textures files, if any).</returns>
        public override string[] ParseTexturePaths(string absolutePath)
        {
            List<string> mtlTexPathList = new List<string>();
            string basePath = GetDirName(absolutePath);

            string mtlLibName = ParseMaterialLibName(absolutePath);

            if (!string.IsNullOrEmpty(mtlLibName))
            {
                //mtlDepPathList.Add(mtlLibName);
                string mtlPath = basePath + mtlLibName;
                string[] lines = File.ReadAllLines(mtlPath);
                List<MaterialData> mtlData = new List<MaterialData>();
                ParseMaterialData(lines, mtlData);
                foreach (MaterialData mtl in mtlData)
                {
                    if (!string.IsNullOrEmpty(mtl.diffuseTexPath))
                    {
                        mtlTexPathList.Add(mtl.diffuseTexPath);
                    }
                    if (!string.IsNullOrEmpty(mtl.specularTexPath))
                    {
                        mtlTexPathList.Add(mtl.specularTexPath);
                    }
                    if (!string.IsNullOrEmpty(mtl.bumpTexPath))
                    {
                        mtlTexPathList.Add(mtl.bumpTexPath);
                    }
                    if (!string.IsNullOrEmpty(mtl.opacityTexPath))
                    {
                        mtlTexPathList.Add(mtl.opacityTexPath);
                    }
                }
            }

            return mtlTexPathList.ToArray();
        }

        protected override IEnumerator LoadModelFile(string absolutePath)
        {
            string url = absolutePath.Contains("//") ? absolutePath : "file:///" + absolutePath;
            WWW www = new WWW(url);
            yield return www;

            if (string.IsNullOrEmpty(www.text))
            {
                Debug.LogError("Failed to load " + www.url);
                objLoadingProgress.message = "Failed to load " + www.url;
                objLoadingProgress.error = true;
                //totalProgress.fileProgress.Remove( objLoadingProgress );
                yield break;
            }
            //Debug.LogFormat("Parsing geometry data in {0}...", www.url);

            yield return ParseGeometryData(www.text);
        }

        protected override IEnumerator LoadMaterialLibrary(string absolutePath)
        {
            string mtlPath;
            if(absolutePath.Contains("//"))
            {
                mtlPath = absolutePath.Remove(absolutePath.LastIndexOf('/')+1) + mtlLib;
            }
            else
            {
                string basePath = GetDirName(absolutePath);
                mtlPath = "file:///" + basePath + mtlLib;
            }
            WWW loader = new WWW(mtlPath);
            yield return loader;

            if (loader.error != null)
            {
                Debug.LogError(loader.error);
            }
            else
            {
                //Debug.LogFormat("Parsing material libray {0}...", loader.url);
                objLoadingProgress.message = "Parsing material libray...";
                ParseMaterialData(loader.text);
            }

        }

        private void GetFaceIndicesByOneFaceLine(DataSet.FaceIndices[] faces, string[] p, bool isFaceIndexPlus)
        {
            if (isFaceIndexPlus)
            {
                for (int j = 1; j < p.Length; j++)
                {
                    string[] c = p[j].Trim().Split("/".ToCharArray());
                    DataSet.FaceIndices fi = new DataSet.FaceIndices();
                    // vertex
                    int vi = int.Parse(c[0]);
                    fi.vertIdx = vi - 1;
                    // uv
                    if (c.Length > 1 && c[1] != "")
                    {
                        int vu = int.Parse(c[1]);
                        fi.uvIdx = vu - 1;
                    }
                    // normal
                    if (c.Length > 2 && c[2] != "")
                    {
                        int vn = int.Parse(c[2]);
                        fi.normIdx = vn - 1;
                    }
                    else
                    {
                        fi.normIdx = -1;
                    }
                    faces[j - 1] = fi;
                }
            }
            else
            { // for minus index
                int vertexCount = dataSet.vertList.Count;
                int uvCount = dataSet.uvList.Count;
                for (int j = 1; j < p.Length; j++)
                {
                    string[] c = p[j].Trim().Split("/".ToCharArray());
                    DataSet.FaceIndices fi = new DataSet.FaceIndices();
                    // vertex
                    int vi = int.Parse(c[0]);
                    fi.vertIdx = vertexCount + vi;
                    // uv
                    if (c.Length > 1 && c[1] != "")
                    {
                        int vu = int.Parse(c[1]);
                        fi.uvIdx = uvCount + vu;
                    }
                    // normal
                    if (c.Length > 2 && c[2] != "")
                    {
                        int vn = int.Parse(c[2]);
                        fi.normIdx = vertexCount + vn;
                    }
                    else
                    {
                        fi.normIdx = -1;
                    }
                    faces[j - 1] = fi;
                }
            }
        }

        /// <summary>
        /// Convert coordinates according to import options.
        /// </summary>
        private Vector3 ConvertVec3(float x, float y, float z)
        {
            if (Scaling != 1f)
            {
                x *= Scaling;
                y *= Scaling;
                z *= Scaling;
            }
            if (ConvertVertAxis) return new Vector3(-x, z, -y);
            return new Vector3(-x, y, z);
        }

        /// <summary>
        /// Parse the OBJ file to extract geometry data.
        /// </summary>
        /// <param name="objDataText">OBJ file text</param>
        /// <returns>Execution is splitted into steps to not freeze the caller method.</returns>
        protected IEnumerator ParseGeometryData(string objDataText)
        {
            yield return null;

            string[] lines = objDataText.Split("\n".ToCharArray());

            bool isFirstInGroup = true;
            bool isFaceIndexPlus = true;

            objLoadingProgress.message = "Parsing geometry data...";
            // store separators, used multiple times
            char[] separators = new char[] { ' ', '\t' };
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.Length > 0 && line[0] == '#')
                { // comment line
                    continue;
                }
                string[] p = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length == 0)
                { // empty line
                    continue;
                }
                string parameters = null;
                if (line.Length > p[0].Length)
                {
                    parameters = line.Substring(p[0].Length + 1).Trim();
                }

                switch (p[0])
                {
                    case "o":
                        dataSet.AddObject(parameters);
                        isFirstInGroup = true;
                        break;
                    case "g":
                        isFirstInGroup = true;
                        dataSet.AddGroup(parameters);
                        break;
                    case "v":
                        dataSet.AddVertex(ConvertVec3(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3])));
                        break;
                    case "vt":
                        dataSet.AddUV(new Vector2(float.Parse(p[1]), float.Parse(p[2])));
                        break;
                    case "vn":
                        dataSet.AddNormal(ConvertVec3(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3])));
                        break;
                    case "f":
                        {
                            int numVerts = p.Length - 1;
                            DataSet.FaceIndices[] faces = new DataSet.FaceIndices[numVerts];
                            if (isFirstInGroup)
                            {
                                isFirstInGroup = false;
                                string[] c = p[1].Trim().Split("/".ToCharArray());
                                isFaceIndexPlus = (int.Parse(c[0]) >= 0);
                            }
                            GetFaceIndicesByOneFaceLine(faces, p, isFaceIndexPlus);
                            if (numVerts == 3)
                            {
                                dataSet.AddFaceIndices(faces[0]);
                                dataSet.AddFaceIndices(faces[2]);
                                dataSet.AddFaceIndices(faces[1]);
                            }
                            else if (numVerts == 4)
                            {
                                dataSet.AddFaceIndices(faces[0]);
                                dataSet.AddFaceIndices(faces[3]);
                                dataSet.AddFaceIndices(faces[1]);
                                dataSet.AddFaceIndices(faces[3]);
                                dataSet.AddFaceIndices(faces[2]);
                                dataSet.AddFaceIndices(faces[1]);
                            }
                            else
                            {
                                Debug.LogWarning("face vertex count :" + (p.Length - 1) + " larger than 4");
                                // TODO: support for faces with more than 4 vertices
                            }
                        }
                        break;
                    case "mtllib":
                        if (!string.IsNullOrEmpty(parameters))
                        {
                            mtlLib = parameters;
                        }
                        break;
                    case "usemtl":
                        if (!string.IsNullOrEmpty(parameters))
                        {
                            dataSet.AddMaterialName(parameters);
                        }
                        break;
                }

                // update progress only sometimes
                if (i % 7000 == 0)
                {
                    objLoadingProgress.percentage = LOAD_PHASE_PERC * i / lines.Length;
                    yield return null;
                }
            }
            objLoadingProgress.percentage = LOAD_PHASE_PERC;
            //dataSet.PrintSummary();
        }

        /// <summary>
        /// Extract the material library (file) name from the OBJ file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string ParseMaterialLibName(string path)
        {
            string[] lines = File.ReadAllLines(path);

            objLoadingProgress.message = "Parsing geometry data...";

            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i].Trim();

                if (l.StartsWith("mtllib"))
                {
                    return l.Substring("mtllib".Length).Trim();
                }
            }
            return null;
        }

        /// <summary>
        /// Check if a material library file is defined.
        /// </summary>
        protected override bool HasMaterialLibrary
        {
            get
            {
                return mtlLib != null;
            }
        }

        /// <summary>
        /// Parse the material library text to get material data.
        /// </summary>
        /// <param name="data">material library text (read from file)</param>
        private void ParseMaterialData(string data)
        {
            objLoadingProgress.message = "Parsing material data...";
            string[] lines = data.Split("\n".ToCharArray());
            materialData = new List<MaterialData>();
            ParseMaterialData(lines, materialData);
        }

        /// <summary>
        /// Parse the material library lines to get material data.
        /// </summary>
        /// <param name="lines">lines read from the material library file</param>
        /// <param name="mtlData">list of material data</param>
        private void ParseMaterialData(string[] lines, List<MaterialData> mtlData)
        {
            MaterialData current = new MaterialData();

            char[] separators = new char[] { ' ', '\t' };
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // remove comments
                if (line.IndexOf("#") != -1) line = line.Substring(0, line.IndexOf("#"));
                string[] p = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length == 0 || string.IsNullOrEmpty(p[0])) continue;
                string parameters = null;
                if (line.Length > p[0].Length)
                {
                    parameters = line.Substring(p[0].Length + 1).Trim();
                }
                try
                {
                    switch (p[0])
                    {
                        case "newmtl":
                            current = new MaterialData();
                            current.materialName = parameters;
                            mtlData.Add(current);
                            break;
                        case "Ka": // Ambient component (not supported)
                            current.ambientColor = StringsToColor(p);
                            break;
                        case "Kd": // Diffuse component
                            current.diffuseColor = StringsToColor(p);
                            break;
                        case "Ks": // Specular component
                            current.specularColor = StringsToColor(p);
                            break;
                        case "Ns": // Specular exponent --> shininess
                            current.shininess = float.Parse(p[1]);
                            break;
                        case "d": // dissolve into the background (1=opaque, 0=transparent)
                            current.overallAlpha = p.Length > 1 && p[1] != "" ? float.Parse(p[1]) : 1.0f;
                            break;
                        case "Tr": // Transparency
                            current.overallAlpha = p.Length > 1 && p[1] != "" ? 1.0f - float.Parse(p[1]) : 1.0f;
                            break;
                        case "map_Kd": // Color texture, diffuse reflectivity
                            if (!string.IsNullOrEmpty(parameters))
                            {
                                current.diffuseTexPath = parameters;
                            }
                            break;
                        // TODO: different processing needed, options not supported
                        case "map_kS": // specular reflectivity of the material
                        case "map_Ns": // Scalar texture for specular exponent
                            if (!string.IsNullOrEmpty(parameters))
                            {
                                current.specularTexPath = parameters;
                            }
                            break;
                        case "map_bump": // Bump map texture
                            if (!string.IsNullOrEmpty(parameters))
                            {
                                current.bumpTexPath = parameters;
                            }
                            break;
                        case "bump":
                            ParseBumpParameters(p, current);
                            break;
                        case "map_opacity":
                        case "map_d": // Scalar texture modulating the dissolve into the background
                            if (!string.IsNullOrEmpty(parameters))
                            {
                                current.opacityTexPath = parameters;
                            }
                            break;
                        case "illum": // Illumination model. 1 - diffuse, 2 - specular (not used)
                            current.illumType = int.Parse(p[1]);
                            break;
                        case "refl": // reflection map (replaced with Unity environment reflection)
                            if (!string.IsNullOrEmpty(parameters))
                            {
                                current.hasReflectionTex = true;
                            }
                            break;
                        case "map_kA":
                            if (!string.IsNullOrEmpty(parameters))
                            {
                                Debug.Log("Map not supported:" + line);
                            }
                            break;
                        default:
                            Debug.Log("this line was not processed :" + line);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Error at line {0} in mtl file: {1}", i + 1, e);
                }
            }
        }

        /// <summary>
        /// Parse bump parameters.
        /// </summary>
        /// <param name="param">list of paramers</param>
        /// <param name="mtlData">material data to be updated</param>
        /// <remarks>Only the bump map texture path is used here.</remarks>
        /// <seealso cref="https://github.com/hammmm/unity-obj-loader"/>
        private void ParseBumpParameters(string[] param, MaterialData mtlData)
        {
            Regex regexNumber = new Regex(@"^[-+]?[0-9]*\.?[0-9]+$");

            var bumpParams = new Dictionary<string, BumpParamDef>();
            bumpParams.Add("bm", new BumpParamDef("bm", "string", 1, 1));
            bumpParams.Add("clamp", new BumpParamDef("clamp", "string", 1, 1));
            bumpParams.Add("blendu", new BumpParamDef("blendu", "string", 1, 1));
            bumpParams.Add("blendv", new BumpParamDef("blendv", "string", 1, 1));
            bumpParams.Add("imfchan", new BumpParamDef("imfchan", "string", 1, 1));
            bumpParams.Add("mm", new BumpParamDef("mm", "string", 1, 1));
            bumpParams.Add("o", new BumpParamDef("o", "number", 1, 3));
            bumpParams.Add("s", new BumpParamDef("s", "number", 1, 3));
            bumpParams.Add("t", new BumpParamDef("t", "number", 1, 3));
            bumpParams.Add("texres", new BumpParamDef("texres", "string", 1, 1));
            int pos = 1;
            string filename = null;
            while (pos < param.Length)
            {
                if (!param[pos].StartsWith("-"))
                {
                    filename = param[pos];
                    pos++;
                    continue;
                }
                // option processing
                string optionName = param[pos].Substring(1);
                pos++;
                if (!bumpParams.ContainsKey(optionName))
                {
                    continue;
                }
                BumpParamDef def = bumpParams[optionName];
                ArrayList args = new ArrayList();
                int i = 0;
                bool isOptionNotEnough = false;
                for (; i < def.valueNumMin; i++, pos++)
                {
                    if (pos >= param.Length)
                    {
                        isOptionNotEnough = true;
                        break;
                    }
                    if (def.valueType == "number")
                    {
                        Match match = regexNumber.Match(param[pos]);
                        if (!match.Success)
                        {
                            isOptionNotEnough = true;
                            break;
                        }
                    }
                    args.Add(param[pos]);
                }
                if (isOptionNotEnough)
                {
                    Debug.Log("bump variable value not enough for option:" + optionName + " of material:" + mtlData.materialName);
                    continue;
                }
                for (; i < def.valueNumMax && pos < param.Length; i++, pos++)
                {
                    if (def.valueType == "number")
                    {
                        Match match = regexNumber.Match(param[pos]);
                        if (!match.Success)
                        {
                            break;
                        }
                    }
                    args.Add(param[pos]);
                }
                // TODO: some processing of options
                Debug.Log("found option: " + optionName + " of material: " + mtlData.materialName + " args: " + string.Concat(args.ToArray()));
            }
            // set the file name, if found
            // TODO: other parsed parameters are not used for now
            if (filename != null)
            {
                mtlData.bumpTexPath = filename;
            }
        }

        private Color StringsToColor(string[] p)
        {
            return new Color(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]));
        }

        /// <summary>
        /// Bump parameter definition
        /// </summary>
        /// <remarks>Not really used for material definition, for now.</remarks>
        /// <see cref="https://github.com/hammmm/unity-obj-loader"/>
        private class BumpParamDef
        {
            public string optionName;
            public string valueType;
            public int valueNumMin;
            public int valueNumMax;
            public BumpParamDef(string name, string type, int numMin, int numMax)
            {
                optionName = name;
                valueType = type;
                valueNumMin = numMin;
                valueNumMax = numMax;
            }
        }
    }
}
