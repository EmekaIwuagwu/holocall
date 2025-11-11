# HoloCall Unity Plugins

This directory contains native and third-party plugins required for HoloCall.

## Required Libraries

### 1. WebSocket-Sharp (Required for Signaling)

**Download:**
```bash
# Option 1: NuGet package
# Download from: https://www.nuget.org/packages/WebSocketSharp/

# Option 2: GitHub Release
# Download from: https://github.com/sta/websocket-sharp/releases
```

**Installation:**
1. Download `websocket-sharp.dll`
2. Place in: `Assets/Plugins/websocket-sharp.dll`
3. Unity will auto-import

**Platform Settings:**
- Include platforms: All
- CPU: Any CPU

### 2. Unity WebRTC (Required for Peer Connections)

**Installation via Package Manager:**
```
1. Open Unity
2. Window > Package Manager
3. Click "+" > Add package from git URL
4. Enter: com.unity.webrtc
5. Click "Add"
```

**Or add to manifest.json:**
```json
{
  "dependencies": {
    "com.unity.webrtc": "3.0.0-pre.6"
  }
}
```

### 3. Native Depth Camera Plugins (Desktop Only - Optional)

#### Intel RealSense

**Download:**
- SDK: https://github.com/IntelRealSense/librealsense/releases
- Unity Wrapper: https://github.com/IntelRealSense/librealsense/tree/master/wrappers/unity

**Installation:**
1. Install RealSense SDK on your system
2. Download Unity wrapper
3. Place DLLs in `Assets/Plugins/x86_64/` (Windows) or `Assets/Plugins/` (Linux/Mac)

**Files needed:**
- Windows: `realsense2.dll`, `RsUnityWrapper.dll`
- Linux: `librealsense2.so`, `libRsUnityWrapper.so`
- macOS: `librealsense2.dylib`, `libRsUnityWrapper.dylib`

#### Azure Kinect

**Download:**
- SDK: https://learn.microsoft.com/azure/kinect-dk/sensor-sdk-download
- Unity Plugin: https://github.com/microsoft/Azure-Kinect-Sensor-SDK/tree/develop/tools/k4aviewer

**Installation:**
1. Install Azure Kinect SDK
2. Copy DLLs to Unity project:
   - `k4a.dll` → `Assets/Plugins/x86_64/`
   - `depthengine_2_0.dll` → `Assets/Plugins/x86_64/`

### 4. Platform-Specific Plugins

#### Android
- ARCore XR Plugin (installed via Package Manager)
- No additional DLLs needed

#### iOS
- ARKit XR Plugin (installed via Package Manager)
- No additional frameworks needed (Unity handles it)

#### VR (Quest, OpenXR)
- Oculus XR Plugin (installed via Package Manager)
- OpenXR Plugin (installed via Package Manager)

## Directory Structure

```
Assets/Plugins/
├── README.md                    # This file
├── websocket-sharp.dll          # WebSocket library (REQUIRED)
├── Android/                     # Android-specific plugins
│   └── (ARCore auto-managed)
├── iOS/                         # iOS-specific plugins
│   └── (ARKit auto-managed)
└── x86_64/                      # Desktop 64-bit plugins
    ├── realsense2.dll          # RealSense (optional)
    ├── RsUnityWrapper.dll      # RealSense wrapper (optional)
    ├── k4a.dll                 # Kinect (optional)
    └── depthengine_2_0.dll     # Kinect depth (optional)
```

## Download Links (Quick Reference)

1. **WebSocket-Sharp**: https://github.com/sta/websocket-sharp/releases
   - Get: `websocket-sharp.dll` (v1.0.3-rc11 or later)

2. **Unity WebRTC**: Installed via Package Manager
   - Or: https://github.com/Unity-Technologies/com.unity.webrtc

3. **RealSense SDK**: https://github.com/IntelRealSense/librealsense/releases
   - Get: Latest release for your OS

4. **Azure Kinect SDK**: https://learn.microsoft.com/azure/kinect-dk/sensor-sdk-download
   - Get: v1.4.1 or later

## Verification

After placing plugins, verify in Unity:

1. Check Console for errors
2. Open: Edit > Project Settings > Player > Other Settings
3. Scroll to "Managed Stripping Level": Set to "Minimal" or "Disabled" (for testing)
4. Rebuild player

## Troubleshooting

### "DllNotFoundException: websocket-sharp"
- Ensure `websocket-sharp.dll` is in `Assets/Plugins/`
- Check platform settings on the DLL in Inspector

### "DllNotFoundException: realsense2"
- Install RealSense SDK system-wide first
- Check DLL is in correct platform folder (`x86_64` for 64-bit)
- Verify DLL platform settings in Unity Inspector

### WebRTC not found
- Ensure package is installed: Window > Package Manager
- Check `Packages/manifest.json` includes `com.unity.webrtc`
- Restart Unity if needed

### Android build fails with "Duplicate class"
- Check for duplicate ARCore plugins
- Clean build: File > Build Settings > Android > Switch Platform

## License Notes

- **WebSocket-Sharp**: MIT License
- **Unity WebRTC**: Unity Companion License
- **RealSense SDK**: Apache 2.0
- **Azure Kinect SDK**: MIT License

Ensure you comply with all library licenses when distributing your application.
