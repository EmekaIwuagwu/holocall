# 🔨 HoloCall Assembly - Quick Reference

## ⚡ Fast Track (for experienced developers)

### Backend (5 minutes)
```bash
cd infrastructure/docker
docker-compose up -d
curl http://localhost:8080/health  # Verify
```

### Unity Desktop (30 minutes)
```bash
# 1. Open Unity Hub > Add Project > holocall/unity
# 2. Open with Unity 2022.3 LTS (auto-installs packages)
# 3. Download websocket-sharp.dll to Assets/Plugins/
# 4. Open MainScene.unity > Play (test in editor)
# 5. HoloCall menu > Build > Desktop > Windows 64-bit
```

### Unity Android (45 minutes)
```bash
# 1. Install Android Studio
# 2. Unity > Edit > Preferences > External Tools > Set Android SDK
# 3. File > Build Settings > Android > Switch Platform
# 4. Player Settings: Set package name, enable ARCore
# 5. HoloCall menu > Build > Android > APK (Debug)
# 6. adb install Builds/Android/HoloCall.apk
```

### Unity iOS (45 minutes - macOS only)
```bash
# 1. Install Xcode from App Store
# 2. Unity > File > Build Settings > iOS > Switch Platform
# 3. Player Settings: Set bundle ID, enable ARKit
# 4. HoloCall menu > Build > iOS > Xcode Project
# 5. Open Xcode project > Select team > Build to device
```

---

## 📚 Full Guide

See **[docs/ASSEMBLY_GUIDE.md](docs/ASSEMBLY_GUIDE.md)** for complete step-by-step instructions.

---

## 🎯 What You Get

### ✅ Ready to Run (No Assembly Needed)
- **Backend**: `docker-compose up -d` - Works immediately
- **Infrastructure**: PostgreSQL, Redis, TURN, nginx - All functional

### 🔧 Needs Assembly (1-2 hours)
- **Unity Project**: Must open in Unity Editor
- **WebSocket Library**: Download `websocket-sharp.dll`
- **Platform Builds**: Build for Desktop/Android/iOS

### 🔄 Needs Integration (1-2 weeks dev time)
- **WebRTC**: Peer connection implementation
- **Hologram Rendering**: Point cloud/avatar display
- **Full Testing**: End-to-end with real devices

---

## 📦 Required Downloads

### Unity Packages (Auto-installed via manifest.json)
✅ AR Foundation 5.1+
✅ ARCore XR Plugin (Android)
✅ ARKit XR Plugin (iOS)
✅ XR Plugin Management
✅ Universal Render Pipeline

### Manual Download Required

**1. WebSocket-Sharp DLL (Required)**
```bash
# Download from:
https://github.com/sta/websocket-sharp/releases

# Place at:
holocall/unity/Assets/Plugins/websocket-sharp.dll
```

**2. Unity WebRTC Package (For full functionality)**
```bash
# Install via Package Manager:
# Window > Package Manager > + > Add package from git URL
# Enter: com.unity.webrtc
```

**3. Depth Camera SDKs (Optional - Desktop volumetric only)**
- **RealSense**: https://github.com/IntelRealSense/librealsense/releases
- **Kinect**: https://learn.microsoft.com/azure/kinect-dk/sensor-sdk-download

---

## 🛠️ Unity Build Menu

Once project is open in Unity, use these menus:

```
HoloCall/
├── Build/
│   ├── Desktop/
│   │   ├── Windows 64-bit      → Builds .exe
│   │   ├── macOS               → Builds .app
│   │   ├── Linux 64-bit        → Builds .x86_64
│   │   └── All Platforms       → Builds all desktop
│   ├── Android/
│   │   ├── APK (Debug)         → Builds .apk for testing
│   │   └── AAB (Release)       → Builds .aab for Play Store
│   ├── iOS/
│   │   └── Xcode Project       → Generates Xcode project
│   └── All Platforms           → Builds everything
├── Open Build Folder           → Opens Builds/ directory
├── Clean Builds                → Deletes all builds
└── Validate Build Settings     → Checks configuration
```

---

## 🎮 Platform-Specific Notes

### Desktop
- **Volumetric capture** requires depth camera (RealSense/Kinect)
- **Webcam fallback** works without depth camera (2D video billboard)
- **Target**: 60 FPS, 10 participants max

### Android
- **Requires**: ARCore-compatible device (Android 7.0+)
- **First build**: Takes 15-20 minutes (subsequent builds ~5 min)
- **Enable**: USB debugging on device for install
- **Target**: 30-60 FPS, 4 participants max

### iOS
- **Requires**: macOS with Xcode, ARKit device (iPhone 6s+)
- **Generates**: Xcode project (not direct .ipa)
- **Must**: Configure signing in Xcode before device install
- **Target**: 60 FPS, 6 participants max

---

## 🚧 Current Limitations

### Working ✅
- Backend services (signaling, TURN, database)
- Unity project compilation
- Scene loading and managers
- Platform builds (Desktop/Android/iOS)
- AR plane detection (mobile)
- Basic UI and lifecycle

### Not Yet Implemented ⚠️
- WebRTC peer connections (need Unity WebRTC integration)
- Hologram rendering (point cloud/avatar display)
- Avatar face tracking (data capture works, rendering pending)
- Depth camera integration (SDK wrapper needed)
- Full end-to-end testing

### Estimated Completion Time
- **Basic demo**: 1 week (WebRTC + simple rendering)
- **Full featured**: 2-3 weeks (all holograms, optimization)
- **Production ready**: 4-6 weeks (testing, polish, performance)

---

## 🐛 Common Issues

### "Cannot open project in Unity"
```
Solution: Install Unity 2022.3 LTS (exact version)
Unity Hub > Installs > Add > 2022.3.10f1
```

### "WebSocket-Sharp not found"
```
Solution: Download DLL manually
https://github.com/sta/websocket-sharp/releases
Place in: Assets/Plugins/websocket-sharp.dll
Restart Unity
```

### "Android build fails"
```
Solution: Check these in Player Settings:
- Minimum API Level: 24+
- Scripting Backend: IL2CPP
- Target Architectures: ARM64 (checked)
- XR Plugin Management: ARCore (checked on Android tab)
```

### "iOS build fails to run"
```
Solution: In Xcode:
1. Select Unity-iPhone project
2. Signing & Capabilities > Select Team
3. Enable "Automatically manage signing"
4. Build again
```

### "Backend not responding"
```
Solution: Verify services are running:
docker-compose ps
docker-compose logs backend
Restart if needed: docker-compose restart backend
```

---

## 📞 Need Help?

- **Complete Guide**: [docs/ASSEMBLY_GUIDE.md](docs/ASSEMBLY_GUIDE.md)
- **Troubleshooting**: [docs/SETUP.md#troubleshooting](docs/SETUP.md#troubleshooting)
- **Architecture**: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- **GitHub Issues**: https://github.com/yourusername/holocall/issues

---

## ✅ Assembly Checklist

Use this to track your progress:

### Prerequisites
- [ ] Unity Hub installed
- [ ] Unity 2022.3 LTS installed with Android/iOS modules
- [ ] Docker installed and running
- [ ] Android Studio installed (for Android builds)
- [ ] Xcode installed (for iOS builds on macOS)

### Backend
- [ ] Environment configured (.env file)
- [ ] Services started (`docker-compose up -d`)
- [ ] Health check passing (`curl http://localhost:8080/health`)

### Unity Project
- [ ] Project opened in Unity 2022.3 LTS
- [ ] Packages auto-installed (check Package Manager)
- [ ] WebSocket-Sharp DLL downloaded and placed
- [ ] MainScene.unity opens without errors
- [ ] Signaling server URL configured
- [ ] Test in Play mode works

### Desktop Build
- [ ] Built for your platform (Windows/macOS/Linux)
- [ ] Executable runs
- [ ] Can create/join room
- [ ] Backend connection works

### Android Build (if needed)
- [ ] Android SDK configured
- [ ] Switched to Android platform
- [ ] Player settings configured
- [ ] APK built successfully
- [ ] Installed on device
- [ ] App launches and requests permissions

### iOS Build (if needed)
- [ ] Xcode installed
- [ ] Switched to iOS platform
- [ ] Player settings configured
- [ ] Xcode project generated
- [ ] Signing configured in Xcode
- [ ] Built to device successfully
- [ ] App launches and requests permissions

---

**Assembly complete?** Proceed to [docs/DEMO.md](docs/DEMO.md) for testing scenarios!

🚀 **Ready to build the future of holographic communication!**
