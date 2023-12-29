// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Microsoft.Acoustics
{
    public class VoxelMapSection
    {
        private IntPtr sectionHandle = IntPtr.Zero;

        public readonly float[] Voxels;
        public readonly int VoxelCount;
        public readonly Vector3 MinCorner;
        public readonly Vector3 MaxCorner;
        public readonly float VoxelSize;

        public VoxelMapSection(IntPtr tritonHandle, Vector3 minCorner, Vector3 maxCorner)
        {
            try
            {
                if (!AcousticsPAL.Triton_GetVoxelMapSection(
                    tritonHandle,
                    new AcousticsPALPublic.TritonVec3d(minCorner),
                    new AcousticsPALPublic.TritonVec3d(maxCorner),
                    out sectionHandle))
                {
                    throw new InvalidOperationException();
                }

                // Query and cache section properties
                AcousticsPALPublic.TritonVec3d voxelMinCorner;
                if (!AcousticsPAL.VoxelMapSection_GetMinCorner(sectionHandle, out voxelMinCorner))
                {
                    throw new InvalidOperationException();
                }
                MinCorner = voxelMinCorner.ToVector3();

                AcousticsPALPublic.TritonVec3f mapIncrement;
                if (!AcousticsPAL.VoxelMapSection_GetCellIncrementVector(sectionHandle, out mapIncrement))
                {
                    throw new InvalidOperationException();
                }
                Vector3 increment = mapIncrement.ToVector3();
                var halfIncrement = increment * 0.5f;

                // Remember, we start from x=y=z=1, not 0, so the extra CellIncrement
                var startVoxelCenter = MinCorner + increment + halfIncrement;
                var currentVoxelCenter = startVoxelCenter;
                AcousticsPALPublic.TritonVec3u numVoxels;
                if (!AcousticsPAL.VoxelMapSection_GetCellCount(sectionHandle, out numVoxels))
                {
                    throw new InvalidOperationException();
                }

                List<float> voxels = new List<float>();
                for (uint x = 1; x < numVoxels.x - 1; x++, currentVoxelCenter.x += increment.x)
                {
                    for (uint y = 1; y < numVoxels.y - 1; y++, currentVoxelCenter.y += increment.y)
                    {
                        for (uint z = 1; z < numVoxels.z - 1; z++, currentVoxelCenter.z += increment.z)
                        {
                            if (AcousticsPAL.VoxelMapSection_IsVoxelWall(sectionHandle, new AcousticsPALPublic.TritonVec3u(x, y, z)))
                            {
                                // Add this voxel to our voxel array
                                voxels.Add(currentVoxelCenter.x);
                                voxels.Add(currentVoxelCenter.y);
                                voxels.Add(currentVoxelCenter.z);
                            }
                        }
                        currentVoxelCenter.z = startVoxelCenter.z;
                    }
                    currentVoxelCenter.y = startVoxelCenter.y;
                    currentVoxelCenter.z = startVoxelCenter.z;
                }

                // Assume voxels are cubes, length of side is magnitude in any one direction
                VoxelSize = increment.x;
                Voxels = voxels.ToArray();
                VoxelCount = (voxels.Count / 3);
            }
            finally
            {
                if (sectionHandle != IntPtr.Zero)
                {
                    AcousticsPAL.VoxelMapSection_Destroy(sectionHandle);
                }
            }
        }
    }
}