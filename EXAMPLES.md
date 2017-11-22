# Examples

Examples work in Unity Editor, to make them work as standalone builds you must copy the `models` folder (with its sub-folders) into the same folder as the executable. The `models` folder was placed outside the `Asset` folder to prevent Unity Editor to import them automatically.
To run the examples on a mobile device you must copy the *models* folder (with its sub-folders) into the `Application.persistentDataPath` (on Android: "My Files/Device storage/Android/data/*com.companyname.appname*/files").
To build on Android open *Edit/Project Settings/Player/* (Settings for Android) and expand *Other Settings*.
* Change *com.Company.ProductName* to a valid Bundle Identifier in *Identification/Bundle Identifier*.
* Set *Write Permission* to `External (SDCard)` in *Configuration/Write Permission*.

## 001_Import_SimpleTest

This is the basic example, with a simple script (`AsImpLSample.cs`) attached to a scene game object (`Test`). This script simply loads a sample model on start.
You can change the `File Path` in `Test` game object to load a different model, **also an URL can be used**.

## 002_Import_CustomImporter

This example demonstrates how a customized loader can be created from the `ObjectImporter` and linked to a UI through a `ObjectImporterUI` script attached to the same game object.

Some models (as described in `models/object_list_test.xml`) are loaded when you run the example. You can save and reload the configuration pressing proper buttons in the UI. A slider allows to switch among different scales.

The XML file describing which and how the models are loaded is in the folder `models` and must be manually copied along with that folder to a proper location (executable path for standalone, `Application.persistentDataPath` for mobile).

The camera can be moved using almost the same control as in Unity Scene Editor. This works only in Unity Editor or in standalone version, not for mobile devices (for mobile devices you can use the assets provided by Unity Standard Assets).

If you build this example for Windows you can drag and drop OBJ files onto the executable to load them into the scene (with default settings).

## 003_Import_URLTest

This example, like `002_Import_CustomImporter`, demonstrates how a customized loader can be created from the `ObjectImporter` and linked to a UI through a `ObjectImporterUI` script attached to the same game object. The difference here is that models are downloaded from URLs.

Two models are downloaded from the same URL when you run the example, to demonstrate how the reuseLoaded flag works.

The camera can be moved using almost the same control as in Unity Scene Editor. This works only in Unity Editor or in standalone version, not for mobile devices (for mobile devices you can use the assets provided by Unity Standard Assets).



