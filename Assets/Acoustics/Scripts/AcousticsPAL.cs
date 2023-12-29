// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Microsoft.Acoustics
{
    public class AcousticsPALPublic
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonVec3f
        {
            public float x;
            public float y;
            public float z;

            public TritonVec3f(float a, float b, float c) { x = a; y = b; z = c; }
            public TritonVec3f(Vector3 vec) { x = vec.x; y = vec.y; z = vec.z; }
            public Vector3 ToVector3() { return new Vector3(x, y, z); }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonVec3d
        {
            public double x;
            public double y;
            public double z;

            public TritonVec3d(double a, double b, double c) { x = a; y = b; z = c; }
            public TritonVec3d(Vector3 vec) { x = vec.x; y = vec.y; z = vec.z; }
            public Vector3 ToVector3() { return new Vector3((float)x, (float)y, (float)z); }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonVec3i
        {
            public int x;
            public int y;
            public int z;

            public TritonVec3i(int a, int b, int c) { x = a; y = b; z = c; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonVec3u
        {
            public uint x;
            public uint y;
            public uint z;

            public TritonVec3u(uint a, uint b, uint c) { x = a; y = b; z = c; }
        }

        //! Propagation parameters for initial sound arriving from source at listener, 
        //! typically rendered as the "dry" or "direct" component in audio engines
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonDryParams
        {
            public float GeomDist;

            //! The shortest-path length in meters for sound to get from the audio source to listener,
            //! potentially detouring around intervening scene geometry.
            //!
            //! Typical usage: drive distance attenuation based on this value, rather 
            //! than the line-of-sight distance.
            public float PathLengthMeters;

            //! This is the dB attenuation due to diffraction around the scene for the shortest
            //! path connecting source to listener. By design, this only accounts for the
            //! attenuation due to the obstruction effect of intervening geometry.
            //! Distance attenuation curves can be designed as you wish in your audio engine, 
            //! driven by PathLength parameter.
            //!
            //! Typical usage: Apply on the dry sound as an obstruction value. You can freely 
            //! interpret this into volume and/or low-pass filter. 
            //! 
            //! Note that this value can also be positive such as when the sound source is in 
            //! a corner so that there are multiple very-early reflections. You can either clamp
            //! to 0, or model an additional bass-boost filter in such cases.
            public float LoudnessDb;

            //! The world direction unit vector from which the dry sound arrives at the listener.
            //! This will model "portaling," respond to intervening environmental features, 
            //! detouring around portals or obstructions.
            //!
            //! Typical usage: spatialize the sound from this direction, instead of line-of-sight
            //! direction.
            public TritonVec3f ArrivalDirection;
        }

        //! Propagation parameters for diffuse reverberation, typically rendered as the 
        //! "wet" or "reverb" component in audio engines.
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonWetParams
        {
            //! The initial root-mean-square (RMS) power of reverberation, in dB. Models how the reverb
            //! loudness varies in detail in complex scenes, such as remaining high through the 
            //! length of a tunnel, or increasing due to reflective material, and how wet-to-dry ratio 
            //! increases as listener walks away from a source in a room.
            //! 
            //! Typical usage: Drive your reverb bank with this gain. Assuming that the 
            //! filters have their volume adjusted so their power envelope starts at an RMS gain of 1.
            public float LoudnessDb;

            //! The average world direction from which various reverberant paths arrive at the listener.
            //! Will respond to intervening environmental features like portals or obstructions.
            //!
            //! Typical usage: Spatialize the reverberation for the sound source in this direction.
            public TritonVec3f ArrivalDirection;

            //! Perceived width of reverberation, in degrees; Varies continuously with 0 indicating 
            //! localized reverberation such as heard through a small window, 360 means fully immersive 
            //! isotropic reverb in the center of a room. 
            //! 
            //! Typical usage: Spatialize the reverb for the sound source with this angular width 
            //! around the provided ArrivalDirection parameter.
            public float AngularSpreadDegrees;

            //! The reverberation time: duration in seconds, that it takes for reverb to decay by 60dB.
            //! 
            //! Typical usage: Set the reverb bank to render this decay time for the sound source. 
            public float DecayTimeSeconds;
        }

        //! An object that holds the parameters returned from QueryAcoustics calls, summarizing
        //! the acoustics between dynamic source and listener location.
        //! All directional information is in Triton's canonical coordinate system.
        //! Triton's coordinate system is: metric, right-handed, +X goes left-to-right on screen, 
        //! and +Z goes up (against direction of gravity), so that +Y goes into screen.
        //! Note that since Triton computes propagation in world coordinates,
        //! all its directions are locked to the world, not the listener's head.
        //! This means that the user's head rotation must be applied on top of these parameters
        //! to reproduce the acoustics. This is the job of the spatializer, assumed to be a separate component.
        //! \image html TritonCoordinates.png
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonAcousticParameters
        {
            //! Special value (-FLT_MAX) that indicates failure to compute a parameter.
            //! Far outside of the normal range of parameter values.
            public static readonly float FailureCode = -3.402823466e+38F; // -FLT_MAX

            //! Propagation parameters for initial sound arriving from source at listener
            public TritonDryParams Dry;
            //! Propagation parameters for diffuse reverberation
            public TritonWetParams Wet;


            public void CopyValuesFrom(TritonAcousticParameters other)
            {
                Dry.ArrivalDirection = other.Dry.ArrivalDirection;
                Dry.LoudnessDb = other.Dry.LoudnessDb;
                Dry.PathLengthMeters = other.Dry.PathLengthMeters;
                Wet.AngularSpreadDegrees = other.Wet.AngularSpreadDegrees;
                Wet.ArrivalDirection = other.Wet.ArrivalDirection;
                Wet.DecayTimeSeconds = other.Wet.DecayTimeSeconds;
                Wet.LoudnessDb = other.Wet.LoudnessDb;
            }

            public void ResetToError()
            {
                Dry.ArrivalDirection.x = FailureCode;
                Dry.ArrivalDirection.y = FailureCode;
                Dry.ArrivalDirection.z = FailureCode;
                Dry.LoudnessDb = FailureCode;
                Dry.PathLengthMeters = FailureCode;
                Wet.AngularSpreadDegrees = FailureCode;
                Wet.ArrivalDirection.x = FailureCode;
                Wet.ArrivalDirection.y = FailureCode;
                Wet.ArrivalDirection.z = FailureCode;
                Wet.DecayTimeSeconds = FailureCode;
                Wet.LoudnessDb = FailureCode;
            }
        }
    }

    internal class AcousticsPAL
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct ATKMatrix4x4
        {
            public float m11;
            public float m12;
            public float m13;
            public float m14;
            public float m21;
            public float m22;
            public float m23;
            public float m24;
            public float m31;
            public float m32;
            public float m33;
            public float m34;
            public float m41;
            public float m42;
            public float m43;
            public float m44;

            public ATKMatrix4x4(Matrix4x4 a)
            {
                m11 = a.m00;
                m12 = a.m01;
                m13 = a.m02;
                m14 = a.m03;
                m21 = a.m10;
                m22 = a.m11;
                m23 = a.m12;
                m24 = a.m13;
                m31 = a.m20;
                m32 = a.m21;
                m33 = a.m22;
                m34 = a.m23;
                m41 = a.m30;
                m42 = a.m31;
                m43 = a.m32;
                m44 = a.m33;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonAcousticParametersDebug
        {
            public int SourceId;
            public AcousticsPALPublic.TritonVec3d SourcePosition;
            public AcousticsPALPublic.TritonVec3d ListenerPosition;
            public float Outdoorness;
            public AcousticsPALPublic.TritonAcousticParameters AcousticParameters;
        }

        public enum ProbeLoadState
        {
            Loaded,
            NotLoaded,
            LoadFailed,
            LoadInProgress,
            DoesNotExist,
            Invalid
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ProbeMetadata
        {
            // Current loading state of this probe
            public ProbeLoadState State;

            // World location of this probe
            public AcousticsPALPublic.TritonVec3d Location;

            // Corners of the cubical region around this probe
            // for which it has data
            public AcousticsPALPublic.TritonVec3d DataMinCorner;
            public AcousticsPALPublic.TritonVec3d DataMaxCorner;
        }

        // Structs for Wwise integration
        [StructLayout(LayoutKind.Sequential)]
        public struct UserDesign
        {
            public float OcclusionMultiplier;
            public float WetnessAdjustment;
            public float DecayTimeMultiplier;
            public float OutdoornessAdjustment;
            public float DRRDistanceWarp;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TritonWwiseParams
        {
            public UInt64 ObjectId;
            public AcousticsPALPublic.TritonAcousticParameters TritonParams;
            public float Outdoorness;
            public UserDesign Design;
        }

        // Import the functions from the DLL
        // Define the dll name based on the target platform
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_ANDROID || UNITY_XBOXONE
    const string TritonDll = "Triton";
    const string SpatializerDll = "AudioPluginMicrosoftAcoustics";
#elif UNITY_STANDALONE_OSX
    // Triton dylib is included inside AudioPluginMicrosoftAcoustics bundle file
    // (virtual directory) on MacOS, specify bundle name in order to bind to 
    // libTriton.dylib exports
    const string TritonDll = "AudioPluginMicrosoftAcoustics";
    const string SpatializerDll = "AudioPluginMicrosoftAcoustics";
#else
        // No other platforms are currently supported
        const string TritonDll = " ";
        const string SpatializerDll = " ";
#endif

        // Spatializer Exports
        [DllImport(SpatializerDll)]
        public static extern bool Spatializer_SetTritonHandle(IntPtr triton);

        [DllImport(SpatializerDll)]
        public static extern void Spatializer_SetAceFileLoaded(bool loaded);

        [DllImport(SpatializerDll)]
        public static extern void Spatializer_SetTransforms(ATKMatrix4x4 worldToLocal, ATKMatrix4x4 localToWorld);

        [DllImport(SpatializerDll)]
        public static extern bool Spatializer_GetDebugInfo(out IntPtr debugInfo, out int count);

        [DllImport(SpatializerDll)]
        public static extern void Spatializer_FreeDebugInfo(IntPtr debugInfo);

        // Triton API Exports
        [DllImport(TritonDll)]
        public static extern bool Triton_CreateInstance(bool debug, out IntPtr triton);

        [DllImport(TritonDll)]
        public static extern bool Triton_LoadAceFile(IntPtr triton, [MarshalAs(UnmanagedType.LPStr)] string filename);

        [DllImport(TritonDll)]
        public static extern bool Triton_LoadAll(IntPtr triton, bool block);

        [DllImport(TritonDll)]
        public static extern bool Triton_UnloadAll(IntPtr triton, bool block);

        [DllImport(TritonDll)]
        public static extern bool Triton_LoadRegion(IntPtr triton, AcousticsPALPublic.TritonVec3d center, AcousticsPALPublic.TritonVec3d length, bool unloadOutside, bool block, out int probesLoaded);

        [DllImport(TritonDll)]
        public static extern bool Triton_UnloadRegion(IntPtr triton, AcousticsPALPublic.TritonVec3d center, AcousticsPALPublic.TritonVec3d length, bool block);

        [DllImport(TritonDll)]
        public static extern bool Triton_QueryAcoustics(IntPtr triton, AcousticsPALPublic.TritonVec3d sourcePosition, AcousticsPALPublic.TritonVec3d listenerPosition, out AcousticsPALPublic.TritonAcousticParameters acousticParams);

        [DllImport(TritonDll)]
        public static extern bool Triton_GetOutdoornessAtListener(IntPtr triton, AcousticsPALPublic.TritonVec3d listenerPosition, out float outdoorness);

        [DllImport(TritonDll)]
        public static extern bool Triton_DestroyInstance(IntPtr triton);

        [DllImport(TritonDll)]
        public static extern bool Triton_Clear(IntPtr triton);

        [DllImport(TritonDll)]
        public static extern bool Triton_GetProbeCount(IntPtr triton, out int count);

        [DllImport(TritonDll)]
        public static extern bool Triton_GetProbeMetadata(IntPtr triton, int index, out ProbeMetadata metadata);

        [DllImport(TritonDll)]
        public static extern bool Triton_GetVoxelMapSection(
            IntPtr triton,
            AcousticsPALPublic.TritonVec3d minCorner,
            AcousticsPALPublic.TritonVec3d maxCorner,
            out IntPtr section);

        [DllImport(TritonDll)]
        public static extern bool Triton_GetPreappliedTransform(
            IntPtr triton,
            out ATKMatrix4x4 preappliedTransform);

        [DllImport(TritonDll)]
        public static extern bool Triton_CalculateReverbSendWeights(
            float targetReverbTime, uint numReverbs, float[] reverbTimes, float[] reverbSendWeights);

        [DllImport(TritonDll)]
        public static extern bool VoxelMapSection_Destroy(IntPtr section);

        [DllImport(TritonDll)]
        public static extern bool VoxelMapSection_GetCellCount(IntPtr section, out AcousticsPALPublic.TritonVec3u count);

        [DllImport(TritonDll)]
        public static extern bool VoxelMapSection_IsVoxelWall(IntPtr section, AcousticsPALPublic.TritonVec3u cell);

        [DllImport(TritonDll)]
        public static extern bool VoxelMapSection_GetMinCorner(IntPtr section, out AcousticsPALPublic.TritonVec3d value);

        [DllImport(TritonDll)]
        public static extern bool VoxelMapSection_GetCellIncrementVector(IntPtr section, out AcousticsPALPublic.TritonVec3f vector);
        
    }
}