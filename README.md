# AsImpL (OBJ)
### Asynchronous Importer and run-time Loader for Unity
**Requires Unity 5.5.4 or higher.**

Load 3D models (currently **only OBJ**) into Unity scene,
both at run-time and in Editor mode, importing them into unity project
as assets.
To exploit the same features implemented in the run-time loader it can
also be used as asset importer (a prefab with its dependencies is
created in the project).

**3D models can be loaded both from files and from URLs.**

 ![image](https://raw.githubusercontent.com/gpvigano/AsImpL/master/images/test_AsImpL.jpg)

*OBJ model imported with AsImpL*

Some improvements are made in OBJ import compared with the current version of Unity:
* bump map
  * bump map is converted into a normal map
  * computation of normal maps and tangents
* specular map
  * specular map is converted to glossness map (metallic texture)
* reflection map
  * reflection map is replaced with the ambient reflection
    (skybox or reflection probes, if available)
* opacity map
  * diffuse and opacity maps are blended together in the albedo texture
* vertex colors
  * if vertex colors are available (and no material library is defined) they are loaded and used to color the mesh
* flag to use an Unlit shader for materials with the lighting rendered to the diffuse texture
* vertical axis conversion
* mesh colliders
  * mesh colliders can optionally be generated from the loaded mesh,
  even with convex mesh generation


 ![image](https://raw.githubusercontent.com/gpvigano/AsImpL/master/images/unity_vs_AsImpL_Zup.png)
 
*Example of a model imported with AsImpL (left) and Unity 5.5 (right)*

 ![image](https://raw.githubusercontent.com/gpvigano/AsImpL/master/images/AsImpLvsUnity.png)
 
 *Example of models imported with AsImpL (above) and Unity 5.5 (below)*

This project was started because of these requirements (partly addressed by some existing projects):
* load models in an asynchronous way (without "freezing" the scene during loading)
* load more models concurrently
* show the loading progress on a UI
* import the loaded models as assets into Unity projects
* prepare the loader to be extended with different file formats

## Features
* OBJ format import/loading:
  * for Unity up to 2017.2 meshes with more than 65K vertices/indices
    are splitted and loaded as children of the same game object
  * in Unity 2017.3 and above 32 bit indices are used, but only for
    meshes with more than 65K vertices (like Unity importer does)
  * support for faces with an arbitrary number of vertices, even concave
  * groups are loaded into game (sub) objects
    * if no sub object is defined faces are grouped by material
      into sub-objects
  * extended material support
    * diffuse and opacity maps are blended together in the albedo texture
    * reflection map is replaced with the ambient reflection
      (skybox or reflection probes, if available)
    * specular map is converted to glossness map (metallic texture)
    * bump map is converted into a normal map
  * computation of normal maps and tangents
  * automatic generation of mesh colliders (even convex)
  * progressive loading (using Unity coroutines)
  * reusing data for instancing multiple objects (at run-time)
* Separate import manager:
  * concurrent loading: a loader is created for each model
  * option for importing files as a prefab+assets (meshes and textures)
    in Unity project
  * support for in-scene UI (progress display and messages)
* Import dialog for Unity Editor
  * option to convert vertical axis (from Z to Y)
  * object scaling
  * option to consider diffuse texture as diffuse + precomputed lightmap
  * option to get a full double-sided geometry (faces cloned & flipped before being imported)
  * file browser both for the model path and a default folder path
  (**note: a file browser is available only in Editor**)
  * option to store the loaded files as assets in the project
  * settings can be saved and restored
  * progress bar with messages displayed while importing OBJ files
  * cancel button to abort importing process
* Examples provided
  * single model import
  * multiple models concurrent import
  * UI for progress display

## Documentation
A menu *AsImpL* is added to the Unity Editor main menu, with a sub-menu
*Import OBJ model* that opens a window. In this window you can set paths
and import settings, then you can press *Import* to start importing the
selected model. A progress bar shows the import progress and phase until
the model has been loaded (or until you press *Cancel* to abort the
process). A utility menu item `Capture screenshot` was added to take
a screenshot, the file is named automatically and saved into the main
project folder, then the folder is opened in your file manager.

A sub-menu *Import OBJ model [AsImpL]* is added to the Unity Editor Asset menu.
It opens a window where you can set paths and import settings, then you can press *Import* to start importing the
selected model. A progress bar shows the import progress and phase until
the model has been loaded (or until you press *Cancel* to abort the
process). To the Window menu a sub-menu item `Capture screenshot [AsImpL]` is added to take
a screenshot, the file is named automatically and saved into the main
project folder, then the folder is opened in your file manager.

An example scene is provided to demonstrate how the importer can be
connected to a UI and extended with new features.

The code in this project *should* be prepared to be extended for supporting other file formats.
Even if the only supported format is currently OBJ, the idea is to create a common framework on which the support for other formats could be developed, allowing the exchange of data with other applications.

The import process is divided into separate phases:
* *texture asset import (if importing assets)*
* file parsing and data filling
* game object creation in Unity scene
* *materials, meshes and prefab assets creation (if importing assets)*

You can find the complete AsImpL documentation in `Documentation` folder
both as [compressed HTML] and [zipped HTML].

To load some OBJ files you can add a ObjectImporter to a game object and
call its ImportModelAsync() method from a MonoBehavior,
see `001_Import_SimpleTest` example scene in `Assets/AsImpL/Examples`, where
you can find also a more advanced example in `002_Import_CustomImporter`.
In [EXAMPLES.md](https://github.com/gpvigano/AsImpL/blob/master/EXAMPLES.md) you can find details about each example.

### Acknowledgements:

This work started looking at the [Runtime OBJ Loader],
from which some source code (in particular TextureLoader.cs and parts of LoaderObj) came.
The OBJ file loader is inspired by [Runtime OBJ Loader], [unity-obj-loader], [unity-remote-obj-loader] and all the people who shared their ideas (e.g. [Bartek Drozdz]).
The first asynchronous loading implementation comes from [unity-remote-obj-loader].
The triangulation implementation is derived from [Erik Nordeus's tutorial](https://www.habrador.com/tutorials/math/10-triangulation/).

Thanks in advance to all the people who contributed and will contribute in any way to this project.

### Contributing

Contributions from you are welcome!

If you find bugs or you have any new idea for improvements and new features you can raise an issue on GitHub (please follow the suggested template, filling the proper sections). To open issues or make pull requests please follow the instructions in [CONTRIBUTING.md](https://github.com/gpvigano/AsImpL/blob/master/CONTRIBUTING.md).

### License

Code released under the [MIT License](https://github.com/gpvigano/AsImpL/blob/master/LICENSE.txt).


---
This is [on GitHub](https://github.com/gpvigano/AsImpL).

To try this project with Unity press the button **Clone or download** and choose [**Download ZIP**](https://github.com/gpvigano/AsImpL/archive/master.zip). Save and unzip the archive to your hard disk and then you can open it with Unity.

[Runtime OBJ Loader]: http://forum.unity3d.com/threads/free-runtime-obj-loader.365884/
[unity-obj-loader]: https://github.com/hammmm/unity-obj-loader
[unity-remote-obj-loader]: https://github.com/cmdr2/unity-remote-obj-loader
[compressed HTML]: https://github.com/gpvigano/AsImpL/blob/master/Documentation/AsImpL.chm
[zipped HTML]: https://github.com/gpvigano/AsImpL/blob/master/Documentation/AsImpL_html.zip
[Bartek Drozdz]: http://www.everyday3d.com/blog/index.php/2010/05/24/loading-3d-models-runtime-unity3d/

