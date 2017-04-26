using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AsImpL
{
    /// <summary>
    /// Build the game onject hierarchy with meshes and materials from a DataSet and a MaterialData list.
    /// </summary>
    public class ObjectBuilder
    {
        /// <summary>
        /// Optional build options
        /// </summary>
        public ImportOptions buildOptions = null;
#if UNITY_EDITOR
        /// <summary>
        /// Alternative texture path used to route loading requests to a proper asset database folder.
        /// Set by <see cref="ObjectImporter"/>.
        /// </summary>
        public string alternativeTexPath = null;
#endif
        private BuildStatus buildStatus = new BuildStatus();
        private DataSet currDataSet;
        private GameObject currParentObj;
        private Dictionary<string, Material> currMaterials;
        private List<MaterialData> materialData;

        /// <summary>
        /// Get the indexed list of imported materials
        /// </summary>
        public Dictionary<string, Material> ImportedMaterials { get { return currMaterials; } }

        /// <summary>
        /// Get the number of imported materials or 0 if nothing has been imported.
        /// </summary>
        public int NumImportedMaterials { get { return currMaterials != null ? currMaterials.Count : 0; } }

        private static int MAX_VERTICES_LIMIT_FOR_A_MESH = 65000;
        private static int MAX_INDICES_LIMIT_FOR_A_MESH = 65000;
        // maximum number of vertices that can be used for triangles
        private static int MAX_VERT_COUNT = (MAX_VERTICES_LIMIT_FOR_A_MESH - 2) / 3 * 3;

        /// <summary>
        /// Initialize the importing of materials
        /// </summary>
        /// <param name="materialData"></param>
        public void InitBuilMaterials(List<MaterialData> materialData)
        {
            this.materialData = materialData;
            currMaterials = new Dictionary<string, Material>();
            if (materialData == null)
            {
                currMaterials.Add("default", new Material(Shader.Find("VertexLit")));
            }
        }

        /// <summary>
        /// Import materials step by step. Call this until it returns false.
        /// </summary>
        /// <param name="info">Progress information to be updated</param>
        /// <returns>Return true if in progress, false otherwise.</returns>
        public bool BuildMaterials(ProgressInfo info)
        {
            if (materialData == null)
            {
                Debug.LogWarning("No material library defined");
                return false;
            }
            if (info.materialsLoaded >= materialData.Count)
            {
                return false;
            }
            MaterialData matData = materialData[info.materialsLoaded];
            info.materialsLoaded++;
            if (currMaterials.ContainsKey(matData.materialName))
            {
                Debug.LogWarning("Duplicate material found: " + matData.materialName + ". Repeated occurence ignored");
            }
            else
            {
                currMaterials.Add(matData.materialName, BuildMaterial(matData));
            }
            return info.materialsLoaded < materialData.Count;
        }

        /// <summary>
        /// Initialize the asynchronous objects building.
        /// Call this once before calling StartBuildObjectAsync().
        /// </summary>
        /// <param name="dataSet">data set used to build the object</param>
        /// <param name="parentObj">game object to which the object will be attached</param>
        /// <param name="materials">dictionary mapping from materil name to material</param>
        public void StartBuildObjectAsync(DataSet dataSet, GameObject parentObj, Dictionary<string, Material> materials = null)
        {
            currDataSet = dataSet;
            currParentObj = parentObj;
            if (materials != null)
            {
                currMaterials = materials;
            }
        }

        /// <summary>
        /// Build an object in more steps, one game object at a time.
        /// Call StartBuildObjectAsync() once, then call this until it returns true.
        /// </summary>
        /// <param name="info">progress information data updated on each call</param>
        /// <returns></returns>
        public bool BuildObjectAsync(ref ProgressInfo info)
        {
            bool result = BuildNextObject(currParentObj, currMaterials);
            info.objectsLoaded = buildStatus.objCount;
            info.groupsLoaded = buildStatus.subObjCount;
            info.numGroups = buildStatus.numGroups;
            return result;
        }

        /// <summary>
        /// Calculate tangent space vectors for a mesh.
        /// <see cref="http://forum.unity3d.com/threads/how-to-calculate-mesh-tangents.38984/"/>
        /// </summary>
        /// <param name="origMesh">Mesh to be filled with tangents</param>
        /// TODO: move this to a general utility class?
        public static void Solve(Mesh origMesh)
        {
            if (origMesh.uv == null || origMesh.uv.Length == 0)
            {
                Debug.LogWarning("Unable to compute tangent space vectors - texture coordinates not defined.");
                return;
            }
            if (origMesh.vertices == null || origMesh.vertices.Length == 0)
            {
                Debug.LogWarning("Unable to compute tangent space vectors - vertices not defined.");
                return;
            }
            if (origMesh.normals == null || origMesh.normals.Length == 0)
            {
                Debug.LogWarning("Unable to compute tangent space vectors - normals not defined.");
                return;
            }
            if (origMesh.triangles == null || origMesh.triangles.Length == 0)
            {
                Debug.LogWarning("Unable to compute tangent space vectors - triangles not defined.");
                return;
            }
            int vertexCount = origMesh.vertexCount;
            Vector3[] vertices = origMesh.vertices;
            Vector3[] normals = origMesh.normals;
            Vector2[] texcoords = origMesh.uv;
            int[] triangles = origMesh.triangles;
            int triVertCount = origMesh.triangles.Length;
            if (vertices.Length < triVertCount || normals.Length < vertexCount || texcoords.Length < triVertCount || vertexCount < triVertCount)
            {
                Debug.LogWarning("Unable to compute tangent space vectors - not enough data.");
                return;
            }
            int triangleCount = triangles.Length / 3;

            Vector4[] tangents = new Vector4[vertexCount];
            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            int tri = 0;

            for (int i = 0; i < (triangleCount); i++)
            {
                int i1 = triangles[tri];
                int i2 = triangles[tri + 1];
                int i3 = triangles[tri + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = texcoords[i1];
                Vector2 w2 = texcoords[i2];
                Vector2 w3 = texcoords[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;

                tri += 3;
            }

            for (int i = 0; i < (vertexCount); i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan1[i];

                // Gram-Schmidt orthogonalize
                Vector3.OrthoNormalize(ref n, ref t);

                tangents[i].x = t.x;
                tangents[i].y = t.y;
                tangents[i].z = t.z;

                // Calculate handedness
                tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
            }

            origMesh.tangents = tangents;
        }

        /// <summary>
        /// Build an object once at a time, to be reiterated until false is returned.
        /// </summary>
        /// <param name="parentObj">Game object to which the new objects will be attached</param>
        /// <param name="mats">Materials from the previously loaded library</param>
        /// <returns>Return true until no more objects can be added, then false.</returns>
        protected bool BuildNextObject(GameObject parentObj, Dictionary<string, Material> mats)
        {
            if (buildStatus.objCount >= currDataSet.objectList.Count) return false;
            DataSet.ObjectData objData = currDataSet.objectList[buildStatus.objCount];

            if (buildStatus.newObject)
            {
                if (buildStatus.objCount == 0 && objData.name == "default")
                {
                    buildStatus.currObjGameObject = parentObj;
                }
                else
                {
                    buildStatus.currObjGameObject = new GameObject();
                    buildStatus.currObjGameObject.transform.parent = parentObj.transform;
                    buildStatus.currObjGameObject.name = objData.name;
                    // restore the scale if the parent was rescaled
                    buildStatus.currObjGameObject.transform.localScale = Vector3.one;
                }
                buildStatus.subObjParent = buildStatus.currObjGameObject;

                //if (od.Name != "default") go.name = od.Name;
                //Debug.Log("Object: " + objData.name);
                buildStatus.newObject = false;
                buildStatus.subObjCount = 0;
                buildStatus.idxCount = 0;
                buildStatus.grpIdx = 0;
                buildStatus.grpFaceIdx = 0;
                buildStatus.meshPartIdx = 0;
                buildStatus.totFaceIdxCount = 0;
                buildStatus.numGroups = Mathf.Max(1, objData.faceGroups.Count);
            }

            // data for sub-object
            DataSet.ObjectData subObjData = new DataSet.ObjectData();
            subObjData.hasNormals = objData.hasNormals;

            HashSet<int> vertIdxSet = new HashSet<int>();

            bool splitGrp = false;

            DataSet.FaceGroupData grp = new DataSet.FaceGroupData();
            grp.name = objData.faceGroups[buildStatus.grpIdx].name;
            grp.materialName = objData.faceGroups[buildStatus.grpIdx].materialName;

            // copy blocks of face indices to each sub-object data
            for (int f = buildStatus.grpFaceIdx; f < objData.faceGroups[buildStatus.grpIdx].faces.Count; f++)
            {
                // if passed the max num of vertices and not at the last iteration
                if (vertIdxSet.Count / 3 > MAX_VERT_COUNT / 3 || subObjData.allFaces.Count / 3 > MAX_INDICES_LIMIT_FOR_A_MESH / 3)
                {
                    // split the group across more objects
                    splitGrp = true;
                    buildStatus.grpFaceIdx = f;
                    Debug.LogWarningFormat("Maximum vertex number for a mesh exceeded.\nSplitting object {0} ({1}th group, starting from index {2})...", grp.name, buildStatus.grpIdx, f);
                    break;
                }
                DataSet.FaceIndices fi = objData.faceGroups[buildStatus.grpIdx].faces[f];
                subObjData.allFaces.Add(fi);
                grp.faces.Add(fi);
                vertIdxSet.Add(fi.vertIdx);
            }
            if (splitGrp || buildStatus.meshPartIdx > 0)
            {
                buildStatus.meshPartIdx++;
            }
            // create an empty (group) object in case the group has been splitted
            if (buildStatus.meshPartIdx == 1)
            {
                GameObject grpObj = new GameObject();
                grpObj.transform.SetParent(buildStatus.currObjGameObject.transform, false);
                grpObj.name = grp.name;
                buildStatus.subObjParent = grpObj;
            }

            // add a suffix to the group name in case the group has been splitted
            if (buildStatus.meshPartIdx > 0)
            {
                grp.name = buildStatus.subObjParent.name + "_MeshPart" + buildStatus.meshPartIdx;
            }
            subObjData.name = grp.name;

            // add the group to the sub object data
            subObjData.faceGroups.Add(grp);

            // update the start index
            buildStatus.idxCount += subObjData.allFaces.Count;

            if (!splitGrp)
            {
                buildStatus.grpFaceIdx = 0;
                buildStatus.grpIdx++;
            }
            buildStatus.totFaceIdxCount += subObjData.allFaces.Count;
            GameObject subobj = ImportSubObject(buildStatus.subObjParent, subObjData, mats);
            if (subobj == null)
            {
                Debug.LogWarningFormat("Error loading sub object n.{0}.", buildStatus.subObjCount);
            }
            //else Debug.LogFormat( "Imported face indices: {0} to {1}", buildStatus.totFaceIdxCount - sub_od.AllFaces.Count, buildStatus.totFaceIdxCount );

            buildStatus.subObjCount++;

            if (buildStatus.totFaceIdxCount >= objData.allFaces.Count || buildStatus.grpIdx >= objData.faceGroups.Count)
            {
                if (buildStatus.totFaceIdxCount != objData.allFaces.Count)
                {
                    Debug.LogWarningFormat("Imported face indices: {0} of {1}", buildStatus.totFaceIdxCount, objData.allFaces.Count);
                    return false;
                }
                buildStatus.objCount++;
                buildStatus.newObject = true;
            }
            return true;
        }

        private GameObject ImportSubObject(GameObject parentObj, DataSet.ObjectData objData, Dictionary<string, Material> mats)
        {
            GameObject go = new GameObject();
            go.name = objData.name;
            int count = 0;
            if (parentObj.transform)
            {
                while (parentObj.transform.FindChild(go.name))
                {
                    count++;
                    go.name = objData.name + count;
                }
            }
            go.transform.SetParent(parentObj.transform, false);

            //Debug.Log( "Importing sub object:" + objData.Name );

            // count vertices needed for all the faces and map face indices to new vertices
            Dictionary<DataSet.FaceIndices, int> vIdxCont = new Dictionary<DataSet.FaceIndices, int>();
            int vcount = 0;
            foreach (DataSet.FaceIndices fi in objData.allFaces)
            {
                // avoid duplicates
                if (!vIdxCont.ContainsKey(fi))
                {
                    vIdxCont[fi] = vcount;
                    vcount++;
                }
            }

            Vector3[] newVertices = new Vector3[vcount];
            Vector2[] newUVs = new Vector2[vcount];
            Vector3[] newNormals = new Vector3[vcount];

            foreach (DataSet.FaceIndices fi in objData.allFaces)
            {
                int k = vIdxCont[fi];
                newVertices[k] = currDataSet.vertList[fi.vertIdx];
                if (currDataSet.uvList.Count > 0) newUVs[k] = currDataSet.uvList[fi.uvIdx];
                if (currDataSet.normalList.Count > 0 && fi.normIdx >= 0) newNormals[k] = currDataSet.normalList[fi.normIdx];
            }

            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.name = go.name;
            meshFilter.sharedMesh = mesh;

            mesh.vertices = newVertices;
            if (currDataSet.uvList.Count > 0) mesh.uv = newUVs;
            bool objectHasNormals = (currDataSet.normalList.Count > 0 && objData.hasNormals);
            if (objectHasNormals) mesh.normals = newNormals;

            Material material;

            int n = objData.faceGroups[0].faces.Count;

            string matName = (objData.faceGroups[0].materialName != null) ? objData.faceGroups[0].materialName : "default";

            if (mats.ContainsKey(matName))
            {
                material = mats[matName];

                go.GetComponent<Renderer>().sharedMaterial = material;
                DynamicGI.UpdateMaterials(go.GetComponent<Renderer>());
            }
            else
            {
                Debug.LogWarning("ImportSubObject mat:" + matName + " not found.");
            }

            int[] indices = new int[n];

            for (int s = 0; s < n; s++) indices[s] = vIdxCont[objData.faceGroups[0].faces[s]];

            mesh.SetTriangles(indices, 0);


            if (!objectHasNormals)
            {
                mesh.RecalculateNormals();
            }
            Solve(mesh);
            return go;
        }

        /// <summary>
        /// Build a Unity Material from MaterialData
        /// </summary>
        /// <param name="md">material data</param>
        /// <returns>Unity material</returns>
        private Material BuildMaterial(MaterialData md)
        {
            string shaderName = "Standard";// (md.illumType == 2) ? "Standard (Specular setup)" : "Standard";
            bool specularMode = false;// (md.specularTex != null);
            ModelUtil.MtlBlendMode mode = md.overallAlpha < 1.0f ? ModelUtil.MtlBlendMode.TRANSPARENT : ModelUtil.MtlBlendMode.OPAQUE;

            bool useUnlit = buildOptions != null && buildOptions.litDiffuse
                && md.diffuseTex != null
                && md.bumpTex == null
                && md.opacityTex == null
                && md.specularTex == null
                && !md.hasReflectionTex;

            bool? diffuseIsTransparent = null;
            if (useUnlit)
            {// do not use unlit shader if the texture has transparent pixels
                diffuseIsTransparent = ModelUtil.ScanTransparentPixels(md.diffuseTex, ref mode);
            }

            if (useUnlit && diffuseIsTransparent.Value)
            {
                shaderName = "Unlit/Texture";
            }
            else if (specularMode)
            {
                shaderName = "Standard (Specular setup)";
            }
            Material newMaterial = new Material(Shader.Find(shaderName)); // "Standard (Specular setup)"
            newMaterial.name = md.materialName;

            float shinLog = Mathf.Log(md.shininess, 2);
            // get the metallic value from the shininess
            float metallic = Mathf.Clamp01(shinLog / 10.0f);
            // get the smoothness from the shininess
            float smoothness = Mathf.Clamp01(shinLog / 10.0f);
            if (specularMode)
            {
                newMaterial.SetColor("_SpecColor", md.specularColor);
                newMaterial.SetFloat("_Shininess", md.shininess / 1000.0f);
                //m.color = new Color( md.diffuse.r, md.diffuse.g, md.diffuse.b, md.alpha);
            }
            else
            {
                newMaterial.SetFloat("_Metallic", metallic);
                //m.SetFloat( "_Glossiness", md.shininess );
            }



            if (md.diffuseTex != null)
            {
                // diffuse

                if (md.opacityTex != null)
                {
                    // diffuse + opacity:
                    // update diffuse texture if an opacity map was found
                    int w = md.diffuseTex.width;
                    int h = md.diffuseTex.width;
                    Texture2D albedoTexture = new Texture2D(w, h, TextureFormat.ARGB32, false);
                    Color col = new Color();
                    for (int x = 0; x < albedoTexture.width; x++)
                    {
                        for (int y = 0; y < albedoTexture.height; y++)
                        {
                            col = md.diffuseTex.GetPixel(x, y);
                            col.a *= md.opacityTex.GetPixel(x, y).grayscale;
                            // blend diffuse and opacity textures
                            albedoTexture.SetPixel(x, y, col);
                        }
                    }
                    albedoTexture.name = md.diffuseTexPath;
                    albedoTexture.Apply();
                    // mode = ModelUtil.MtlBlendMode.TRANSPARENT;
                    // The map_d value is multiplied by the d value --> Fade mode
                    mode = ModelUtil.MtlBlendMode.FADE;
#if UNITY_EDITOR
                    if (!string.IsNullOrEmpty(alternativeTexPath))
                    {
                        string texAssetPath = AssetDatabase.GetAssetPath(md.opacityTex);
                        if (!string.IsNullOrEmpty(texAssetPath))
                        {
                            EditorUtil.SaveAndReimportPngTexture(ref albedoTexture, texAssetPath, "_alpha");
                        }
                    }
#endif
                    newMaterial.SetTexture("_MainTex", albedoTexture);
                }
                else
                {// md.opacityTex == null

                    // diffuse without opacity: if there are transparent pixels ==> transparent material
                    if (!diffuseIsTransparent.HasValue)
                    {
                        diffuseIsTransparent = ModelUtil.ScanTransparentPixels(md.diffuseTex, ref mode);
                    }
                    newMaterial.SetTexture("_MainTex", md.diffuseTex);
                }
                //Debug.LogFormat("Diffuse set for {0}",m.name);
            }
            else if (md.opacityTex != null)
            {
                // opacity without diffuse
                //mode = ModelUtil.MtlBlendMode.TRANSPARENT;
                mode = ModelUtil.MtlBlendMode.FADE;
                int w = md.opacityTex.width;
                int h = md.opacityTex.width;
                Texture2D albedoTexture = new Texture2D(w, h, TextureFormat.ARGB32, false);
                Color col = new Color();
                bool detected = false;
                for (int x = 0; x < albedoTexture.width; x++)
                {
                    for (int y = 0; y < albedoTexture.height; y++)
                    {
                        col = md.diffuseColor;
                        col.a = md.overallAlpha * md.opacityTex.GetPixel(x, y).grayscale;
                        ModelUtil.DetectMtlBlendFadeOrCutout(col.a, ref mode, ref detected);
                        //if (md.alpha == 1.0f && col.a == 0.0f) mode = ModelUtil.MtlBlendMode.CUTOUT;
                        albedoTexture.SetPixel(x, y, col);
                    }
                }
                albedoTexture.name = md.diffuseTexPath;
                albedoTexture.Apply();
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(alternativeTexPath))
                {
                    string texAssetPath = AssetDatabase.GetAssetPath(md.opacityTex);
                    if (!string.IsNullOrEmpty(texAssetPath))
                    {
                        EditorUtil.SaveAndReimportPngTexture(ref albedoTexture, texAssetPath, "_op");
                    }
                }
#endif
                newMaterial.SetTexture("_MainTex", albedoTexture);
            }

            md.diffuseColor.a = md.overallAlpha;
            newMaterial.SetColor("_Color", md.diffuseColor);

            if (md.bumpTex != null)
            {
                // bump map defined

                // TODO: if importing assets do not create a nomal map, change importer settings

                // let (improperly) assign a normal map to the bumb map
                // if the file name contains a specific tag
                // TODO: customize normal map tag
                if (md.bumpTexPath.Contains("_normal_map"))
                {
                    newMaterial.EnableKeyword("_NORMALMAP");
                    newMaterial.SetFloat("_BumpScale", 0.25f); // lower the bump effect with the normal map
                    newMaterial.SetTexture("_BumpMap", md.bumpTex);
                }
                else
                {
                    // calculate normal map
                    Texture2D normalMap = ModelUtil.HeightToNormalMap(md.bumpTex);
#if UNITY_EDITOR
                    if (!string.IsNullOrEmpty(alternativeTexPath))
                    {
                        string texAssetPath = AssetDatabase.GetAssetPath(md.bumpTex);
                        if (!string.IsNullOrEmpty(texAssetPath))
                        {
                            EditorUtil.SaveAndReimportPngTexture(ref normalMap, texAssetPath, "_nm", true);
                        }
                    }
                    else
#endif
                    {
                        newMaterial.SetTexture("_BumpMap", normalMap);
                        //newMaterial.SetTexture("_BumpMap", md.bumpTex);
                        newMaterial.EnableKeyword("_NORMALMAP");
                        newMaterial.SetFloat("_BumpScale", 1.0f); // adjust the bump effect with the normal map
                    }
                }
            }

            if (md.specularTex != null)
            {
                Texture2D glossTexture = new Texture2D(md.specularTex.width, md.specularTex.height, TextureFormat.ARGB32, false);
                Color col = new Color();
                float pix = 0.0f;
                for (int x = 0; x < glossTexture.width; x++)
                {
                    for (int y = 0; y < glossTexture.height; y++)
                    {
                        pix = md.specularTex.GetPixel(x, y).grayscale;

                        // red = metallic

                        col.r = metallic * pix;// md.specular.grayscale*pix;
                        col.g = col.r;
                        col.b = col.r;

                        // alpha = smoothness

                        // if reflecting set maximum smoothness value, else use a precomputed value
                        if (md.hasReflectionTex) col.a = pix;
                        else col.a = pix * smoothness;

                        glossTexture.SetPixel(x, y, col);
                    }
                }
                glossTexture.Apply();
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(alternativeTexPath))
                {
                    string texAssetPath = AssetDatabase.GetAssetPath(md.specularTex);
                    if (!string.IsNullOrEmpty(texAssetPath))
                    {
                        EditorUtil.SaveAndReimportPngTexture(ref glossTexture, texAssetPath, "_spec");
                    }
                }
#endif

                if (specularMode)
                {
                    newMaterial.EnableKeyword("_SPECGLOSSMAP");
                    newMaterial.SetTexture("_SpecGlossMap", glossTexture);
                }
                else
                {
                    newMaterial.EnableKeyword("_METALLICGLOSSMAP");
                    newMaterial.SetTexture("_MetallicGlossMap", glossTexture);
                }

                //m.SetTexture( "_MetallicGlossMap", md.specularLevelTex );
            }

            // replace the texture with Unity environment reflection
            if (md.hasReflectionTex)
            {
                if (md.overallAlpha < 1.0f)
                {
                    Color col = Color.white;
                    col.a = md.overallAlpha;
                    newMaterial.SetColor("_Color", col);
                    mode = ModelUtil.MtlBlendMode.FADE;
                }
                // the "amount of" info is missing, using a default value
                if (md.specularTex != null)
                {
                    newMaterial.SetFloat("_Metallic", metallic);// 1.0f);
                }
                // usually the reflection texture is not blurred
                newMaterial.SetFloat("_Glossiness", 1.0f);
            }

            ModelUtil.SetupMaterialWithBlendMode(newMaterial, mode);

            //#if UNITY_EDITOR
            //        if (!string.IsNullOrEmpty(alternateTexPath))
            //        {
            //            string path = alternateTexPath + "../Materials/" + m.name + ".mat";
            //            path = path.Replace("Textures/../", "");
            //            Debug.LogFormat("Creating material asset in {0}", path);
            //            AssetDatabase.CreateAsset(m, path);
            //        m = AssetDatabase.LoadAssetAtPath<Material>(path);
            //        }
            //#endif
            return newMaterial;
        }


        public class ProgressInfo
        {
            public int materialsLoaded = 0;
            public int objectsLoaded = 0;
            public int groupsLoaded = 0;
            public int numGroups = 0;
        }

        private class BuildStatus
        {
            // true if a new object must be created
            public bool newObject = true;

            // counter for objects
            public int objCount = 0;

            // counter for sub objects
            public int subObjCount = 0;

            // number of added indices
            public int idxCount = 0;

            // index of the last group
            public int grpIdx = 0;

            // number of the groups for the last object
            public int numGroups = 0;

            // index of the first face index in the group
            public int grpFaceIdx = 0;

            // index of the last mesh part if the group is splitted into parts
            public int meshPartIdx = 0;

            // total number of face indices processed
            public int totFaceIdxCount = 0;

            // current OBJ object
            public GameObject currObjGameObject = null;
            internal GameObject subObjParent;
        }

    }
}
