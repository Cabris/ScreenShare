/*==============================================================================
Copyright (c) 2011-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
==============================================================================*/

================================================================================
BackgroundTextureAccess
================================================================================
This sample app illustrates how to render the the camera image in Unity:
Using a separate orthographic camera, the camera image is rendered with a 
special shader to produce a negative greyscale effect that warps the image 
in response to touch events.

================================================================================
How to use the sample
================================================================================
1. Print the target located in the Assets\Editor\QCAR\ForPrint directory 
   of this package.
2. Point your device at the target.
3. When the target is augmented, touch the device screen to warp the
   background image.

================================================================================
Tips and tricks
================================================================================
Please note that this shader only works with OpenGL ES 2.0 selected.  
This is the default mode.