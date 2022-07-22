# Houdini Engine for Unity(Modified)

## Extend some features:

+ Add quick access class to the HDA output attributes.
+ Add GPU instance API

## Usage:

1. This function will read the follow attributes in Houdini, make sure all the attributes exist and own by the correct owner.

   + orient(vector4, point). Control instance rotation.
   + pscale(vector3, point). Control instance scale.
   + P(vector3, point). Instance centroid.
   + unity_gpu_instance_ptototypeX(string, detail). The prototype of instance mesh(in developement). Prototype is the relative path of the mesh asset, and the "X" in "unity_gpu_instance_ptototypeX" is the prototype index, used to get prototype by index.
   + unity_gpu_instance_ptototypeindex(string, detail). The index of instance prototype(in developement). If this property of the point one is 3, means this point use the mesh asset stored in unity_gpu_instance_ptototype owned by detail as the instance mesh.

2. Make sure the output of your HDA is set to display.

3. Check "GPU Instance" in the "Parameter Options" section of the HDA panel in Unity.

4. "GPU Instance Path" in the "Bake" section controls the output json file path. Every time you press "Cook", "Recook", "Rebuild" button, GPU instance data will be saved if exist.

5. This json data is organized by the structure "*HoudiniEngineUnity*.HEU_GetGPUInstanceData.*GPUInstanceData*". This struct contains 2 array members:

   + points[]: type is List<*HoudiniEngineUnity*.*HEU_InstancedPointData*>, represents all points. 
     *HoudiniEngineUnity*.*HEU_InstancedPointData* is a data type storing all essential data of one point, for now it contains following members:
     + pos: point position
     + modelToWorldMat: matrix change model space position into world space
     + protoIndex: index of prototype
   + proto[]: prototype data, ordered by prototype index.
   
   Access the data by the following codes.
   
   ```csharp
   using System.IO;
   
   // HAPI_GPUINSTANCE_JSON_TMP_PATH is the default instance json path
   string instance_data_json = HoudiniEngineUnity.HEU_Defines.HAPI_GPUINSTANCE_JSON_TMP_PATH;
   StreamReader SRJson = new StreamReader(instance_data_json);
   string json_data = SRJson.ReadToEnd();
   HoudiniEngineUnity.HEU_GetGPUInstanceData.GPUInstanceData gpuinstance_data = JsonUtility.FromJson<HoudiniEngineUnity.HEU_GetGPUInstanceData.GPUInstanceData>(json_data);
   ```

---

Houdini Engine for Unity is a Unity plug-in that allows deep integration of
Houdini technology into Unity through the use of Houdini Engine.

This plug-in brings Houdini's powerful and flexible procedural workflow into
Unity through Houdini Digital Assets. Artists can interactively adjust the
asset's parameters inside Unity, and use Unity geometries as an asset's inputs.
Houdini's procedural engine will then "cook" the asset and the results will be
available right inside Unity.

The easiest way for artists to access the plug-in is to download the latest
production build of Houdini and install the Unity plug-in along with the Houdini interactive software.
Houdini Digital Assets created in Houdini can then be loaded into Unity through the plug-in. 
A growing library of Digital Assets for use in Unity will be available at the [Orbolt Smart 3D Asset
Store](http://www.orbolt.com/unity).

For more information:

* [Houdini Engine for Unity Product Info](https://www.sidefx.com/products/houdini-engine/unity-plug-in/)
* [Houdini Enigne for Unity Documentation](https://www.sidefx.com/docs/unity/index.html)
* [FAQ](https://www.sidefx.com/faq/houdini-engine-faq/)

For support and reporting bugs:

* [SideFX Houdini Engine for Unity forum](https://www.sidefx.com/forum/50/)
* [Bug Submission](https://www.sidefx.com/bugs/submit/)

## Supported Unity versions

Currently, the supported Unity versions are:

* 2018.1 and newer

## Installing from Source

1. Fork this repository to your own Github account using the Fork button at the top.
1. Clone the forked repository to your file system.
1. Download and install the correct build of Houdini. You must have the exact build number and version as HOUDINI_MAJOR, HOUDINI_MINOR, and HOUDINI_BUILD int values in Plugins/HoudiniEngineUnity/Scripts/HEU_HoudiniVersion.cs. You can get the correct build from: http://www.sidefx.com/download/daily-builds (you might need to wait for the build to finish and show up if you're updating to the very latest version of the plugin)
1. Open a project in Unity. Note that if a previous version of the plugin exists in the project (usually located at Assets/Plugins/HoudiniEngineUnity), then you'll need to remove it from the Unity project. To do so, in Unity, in the Project browser, right-click on HoudiniEngineUnity folder in Assets/Plugins and select Delete.
1. Copy the Plugins/HoudiniEngineUnity folder from the cloned repository from step 2, and paste it into your Unity project's Assets/Plugins folder. If the Plugins folder exists, you can simply merge with it.
1. Restart Unity.
1. Ensure Houdini Engine loaded successfully by going to the "HoudiniEngine" top menu and selecting "Installation Info" and making sure all the versions match.
