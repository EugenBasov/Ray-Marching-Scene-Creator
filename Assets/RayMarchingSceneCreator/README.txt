
Project was created on Unity 2022.3.11f1

Instructions for use:

Make sure you installed MyBox package from https://github.com/Deadcows/MyBox
Make sure you installed Unity Mathematics package from https://docs.unity3d.com/Packages/com.unity.mathematics@1.3/manual/index.html
Put EditorCools folder into assets folder in your project.
Put RayMarchingSceneCreator folder into assets folder in your project.

First add the RayMarching component to the working camera. Then create an object and add two components to it --- RayMarchingObjectManager and RayMarchingLightManager. 
Place this object in the appropriate fields in the RayMarching component in the camera. Place file, named RayMarching.compute in the appropriate fields in the RayMarching component in the camera. 
Create an empty object and add a RayMarchingGroup component on it. Place this object in the appropriate fields in the RayMarchingObjectManager component.

In the RayMarchingLightManager component, you can add Unity's light sources and customize shadows.

To create objects, create an empty object and add the RayMarchingObjProps component to it. Move it to RayMarchingGroup object to create hierarchy. To group and combine objects, use the RayMarchingGroup object.

To apply the Union operation, use the MIN function.
To apply the Intersection operation, use the MAX function.
To apply the Subract operation, use the MAX function and enable the IsNegative option in the object or group you want to subract.

Possible issues

If the shader gives an error, you may need to specify the full path to the UnityCG.cginc file in the include part of RayMarching.compute 
(the path will probably look like this - [program files folder]\Unity\Editor\Data\CGIncludes\UnityCG.cginc). This is due to a bug in Unity.

If objects are displayed on the scene “upside down” or in some other strange way, a possible solution is to swap 1 and 0, 3 and 2 at the end of lines 214-217 in the RayMarching.cs file. 
Although most likely this bug will not happen. 