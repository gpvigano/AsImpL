# Examples

Examples work in Unity Editor, to make them work as standalone builds you must copy the `models` folder (with its sub-folders) into the same folder as the executable.

## 001_Import_SimpleTest

This is the basic example, with a simple script (AsImpLSample.cs) attached to a scene game object (Test). This script simply loads a sample model on start.

## 002_Import_CustomImporter

This example demonstrate haw a customized loader can be created from the `ObjectImporter` and linked to a UI through a `ObjectImporterUI` script attached to the same game object.

Some models (as described in `Assets/StreamingAssets/.object_list_test.xml`) are loaded when you run the example. You can save and reload the configuration pressing proper buttons in the UI. A slider allow to switch among different scales.

The XML file describing which and how the models are loaded has a name starting with `.` to let Unity ignore changes to that file. It is placed in the `StreamingAssets` folder so that it is automatically copied to your build.
The same does **not** hold for 3D models, they are in a separate folder `models` and must be manually copied to the build target folder.

The camera can be moved using almost the same control as in Unity Scene Editor. This works only in Unity Editor or in standalone version, not for mobile devices (for mobile devices you can pick the assets provided by Unity Standard Assets).
