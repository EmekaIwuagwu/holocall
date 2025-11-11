# 🔧 HoloCall Complete Assembly Guide

**Time Required:** 1-2 hours (first time), 15 minutes (subsequent builds)

This guide shows you **exactly** how to assemble and run HoloCall for Desktop and Mobile devices.

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Backend Setup (5 minutes)](#backend-setup)
3. [Unity Project Setup (30 minutes)](#unity-project-setup)
4. [Desktop Build (15 minutes)](#desktop-build)
5. [Android Build (20 minutes)](#android-build)
6. [iOS Build (20 minutes - macOS only)](#ios-build)
7. [Testing](#testing)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software

| Software | Version | Platform | Download |
|----------|---------|----------|----------|
| **Unity Hub** | Latest | All | https://unity.com/download |
| **Unity Editor** | 2022.3 LTS | All | Install via Unity Hub |
| **Node.js** | 18+ | All | https://nodejs.org |
| **Docker** | Latest | All | https://www.docker.com |
| **Android Studio** | Latest | Android builds | https://developer.android.com/studio |
| **Xcode** | Latest | iOS builds (macOS) | App Store |

### Optional (for Desktop volumetric)
- Intel RealSense D435/D455
- Azure Kinect DK

---

## Backend Setup

### Step 1: Start Backend Services (5 minutes)

```bash
# Navigate to docker directory
cd holocall/infrastructure/docker

# Copy environment file
cp .env.example .env

# IMPORTANT: Edit .env and set your PUBLIC_IP
nano .env  # or your preferred editor

# Start all services
docker-compose up -d

# Wait for services to start (30-60 seconds)
sleep 30

# Verify backend is running
curl http://localhost:8080/health
```

**Expected Output:**
```json
{"status":"ok","timestamp":"2024-...","uptime":12.34}
```

✅ **Backend is ready!** Keep it running for the entire session.

---

## Unity Project Setup

### Step 2: Install Unity 2022.3 LTS (10 minutes)

1. **Open Unity Hub**
2. Go to **Installs** tab
3. Click **Install Editor**
4. Select **2022.3.10f1** (or latest 2022.3.x LTS)
5. **Important:** Add these modules:

**Required Modules:**
- ✅ **Android Build Support**
  - ✅ Android SDK & NDK Tools
  - ✅ OpenJDK
- ✅ **iOS Build Support** (macOS only)
- ✅ **Windows Build Support** (if on Mac/Linux)
- ✅ **Mac Build Support** (if on Windows/Linux)
- ✅ **Linux Build Support** (if on Windows/Mac)

Click **Install** and wait (15-30 minutes for all modules).

### Step 3: Open HoloCall Project (2 minutes)

1. **In Unity Hub**, go to **Projects** tab
2. Click **Add** (or **Open**)
3. Navigate to `holocall/unity` folder
4. Click **Select Folder**
5. Unity will open and import assets (5-10 minutes first time)

**If prompted about Unity version:**
- Click "Continue" to upgrade if you have 2022.3.x
- Otherwise, install exact version from Unity Archive

### Step 4: Install Required Packages (10 minutes)

Unity should auto-install packages from `manifest.json`, but verify:

**Check Package Manager:**
1. **Window > Package Manager**
2. Change dropdown to "**In Project**"
3. Verify these packages are installed:

**Required Packages:**
- ✅ AR Foundation 5.1+
- ✅ ARCore XR Plugin 5.1+ (for Android)
- ✅ ARKit XR Plugin 5.1+ (for iOS)
- ✅ XR Plugin Management
- ✅ Universal Render Pipeline
- ✅ TextMeshPro

**If any are missing:**
- Change dropdown to "**Unity Registry**"
- Search for package
- Click **Install**

### Step 5: Download WebSocket Library (5 minutes)

**Option A: Pre-built DLL (Easiest)**

```bash
# Download WebSocket-Sharp v1.0.3-rc11
cd holocall/unity/Assets/Plugins

# Linux/Mac:
curl -L https://github.com/sta/websocket-sharp/releases/download/1.0.3-rc11/websocket-sharp.dll -o websocket-sharp.dll

# Windows (PowerShell):
Invoke-WebRequest -Uri "https://github.com/sta/websocket-sharp/releases/download/1.0.3-rc11/websocket-sharp.dll" -OutFile "websocket-sharp.dll"
```

**Option B: Manual Download**

1. Go to: https://github.com/sta/websocket-sharp/releases
2. Download `websocket-sharp.dll` from latest release
3. Copy to `holocall/unity/Assets/Plugins/websocket-sharp.dll`

**Verify in Unity:**
1. Check `Assets/Plugins/websocket-sharp.dll` appears in Project view
2. Select it
3. In Inspector, ensure all platforms are checked

### Step 6: Optional - Download Depth Camera SDKs (Desktop only)

**Intel RealSense (if you have RealSense camera):**

```bash
# Download SDK from:
https://github.com/IntelRealSense/librealsense/releases

# Windows: Run installer
# Linux: Install via apt (see docs/SETUP.md)
# macOS: Install via Homebrew
```

**Azure Kinect (if you have Kinect):**

```bash
# Download from:
https://learn.microsoft.com/azure/kinect-dk/sensor-sdk-download

# Install system-wide
```

**Without depth cameras:** Code falls back to webcam (2D video billboard mode).

### Step 7: Configure HoloCall Settings (2 minutes)

1. **Open Scene:** `Assets/Scenes/MainScene.unity`
2. **In Hierarchy**, select **HoloCallManager**
3. **In Inspector**, configure:

```
Signaling Server URL: ws://localhost:8080/ws

(Or if running on different machine:)
Signaling Server URL: ws://YOUR_IP:8080/ws
```

4. **Save Scene:** Ctrl+S (Cmd+S on Mac)

✅ **Unity project is ready!**

---

## Desktop Build

### Step 8: Build for Desktop (15 minutes)

#### Option A: Using Build Menu

1. **In Unity**, go to **HoloCall > Build > Desktop > Windows 64-bit**
   - Or **macOS** if on Mac
   - Or **Linux 64-bit** if on Linux

2. **Wait for build** (5-10 minutes)

3. **Build complete!** Location: `holocall/unity/Builds/Desktop/`

#### Option B: Using Unity Build Settings

1. **File > Build Settings**
2. **Ensure scene is added:**
   - `Assets/Scenes/MainScene.unity` should be checked
   - If not, click **Add Open Scenes**
3. **Select Platform:**
   - **Windows**: PC, Mac & Linux Standalone > Windows
   - **macOS**: PC, Mac & Linux Standalone > macOS
   - **Linux**: PC, Mac & Linux Standalone > Linux
4. Click **Switch Platform** (if needed)
5. Click **Build** and choose output folder
6. **Wait** (5-10 minutes)

#### Option C: Command Line Build

```bash
# From holocall directory
# Windows
unity-editor -quit -batchmode -projectPath ./unity -executeMethod HoloCallBuilder.BuildWindows64

# macOS
/Applications/Unity/Hub/Editor/2022.3.10f1/Unity.app/Contents/MacOS/Unity -quit -batchmode -projectPath ./unity -executeMethod HoloCallBuilder.BuildMacOS

# Linux
unity-editor -quit -batchmode -projectPath ./unity -executeMethod HoloCallBuilder.BuildLinux64
```

### Step 9: Test Desktop Build

```bash
# Navigate to build folder
cd unity/Builds/Desktop/Windows  # or macOS/Linux

# Run executable
# Windows:
HoloCall.exe

# macOS:
open HoloCall.app

# Linux:
./HoloCall.x86_64
```

**Expected:**
- Window opens
- Can enter email/display name
- Can create or join room
- (WebRTC connection may fail without full setup - that's OK for now)

---

## Android Build

### Step 10: Setup Android Environment (20 minutes - one time)

#### Install Android Studio

```bash
# Download from:
https://developer.android.com/studio

# Install it (follow installer)
```

#### Configure Unity for Android

1. **Open Unity**
2. **Edit > Preferences** (Unity > Settings on Mac)
3. **External Tools**
4. **Android section:**
   - Click **Download** next to Android SDK, NDK, JDK (if not set)
   - Or point to Android Studio installation:
     - SDK: `C:\Users\YourName\AppData\Local\Android\Sdk` (Windows)
     - SDK: `~/Library/Android/sdk` (macOS)
     - SDK: `~/Android/Sdk` (Linux)

5. **Verify paths are set** and click **OK**

#### Enable USB Debugging on Android Device

1. **On Android phone:**
2. **Settings > About phone**
3. **Tap "Build number" 7 times** (Developer options unlocked)
4. **Settings > Developer options**
5. **Enable "USB debugging"**
6. **Connect phone to computer via USB**

### Step 11: Build Android APK (15 minutes)

#### Option A: Using Build Menu (Recommended)

1. **In Unity:** **HoloCall > Build > Android > APK (Debug)**
2. **Wait for build** (10-15 minutes first time)
3. **APK location:** `holocall/unity/Builds/Android/HoloCall.apk`

#### Option B: Using Build Settings

1. **File > Build Settings**
2. **Select Platform: Android**
3. **Click "Switch Platform"** (wait 5 minutes)
4. **Click "Player Settings":**

**Configure these:**
```
Other Settings:
  Package Name: com.yourcompany.holocall
  Minimum API Level: Android 7.0 (API 24)
  Target API Level: API 33 or latest
  Scripting Backend: IL2CPP
  Target Architectures: ✅ ARM64 (REQUIRED for ARCore)
  Internet Access: Require

XR Plug-in Management (left sidebar):
  ✅ ARCore (Android tab)
```

5. **Close Player Settings**
6. **Click "Build"** and choose output location
7. **Wait** (10-15 minutes)

### Step 12: Install APK on Android Device

```bash
# Make sure phone is connected and USB debugging enabled
adb devices
# Should show your device

# Install APK
cd unity/Builds/Android
adb install HoloCall.apk

# Or drag-and-drop APK to phone
```

**If "adb not found":**
```bash
# Add to PATH:
# Windows: C:\Users\YourName\AppData\Local\Android\Sdk\platform-tools
# Mac/Linux: ~/Library/Android/sdk/platform-tools
```

### Step 13: Test Android Build

1. **Open HoloCall app** on phone
2. **Grant permissions:**
   - Camera (for AR)
   - Microphone (for audio)
3. **Enter credentials**
4. **Create or join room**
5. **Point at floor/table** - should see AR plane detection
6. **Tap to place hologram** (currently just placeholder)

---

## iOS Build

### Step 14: Setup iOS Environment (macOS only) (10 minutes)

#### Install Xcode

```bash
# From App Store or:
xcode-select --install
```

#### Configure Unity for iOS

1. **In Unity:** **File > Build Settings**
2. **Select Platform: iOS**
3. **Click "Switch Platform"** (wait 5 minutes)
4. **Click "Player Settings":**

**Configure these:**
```
Other Settings:
  Bundle Identifier: com.yourcompany.holocall
  Target minimum iOS Version: 13.0
  Architecture: ARM64
  Scripting Backend: IL2CPP
  Camera Usage Description: "Required for AR and face tracking"
  Microphone Usage Description: "Required for audio calls"

XR Plug-in Management (left sidebar):
  ✅ ARKit (iOS tab)
```

### Step 15: Build Xcode Project (15 minutes)

#### Option A: Using Build Menu

1. **HoloCall > Build > iOS > Xcode Project**
2. **Wait** (5-10 minutes)
3. **Xcode project created:** `holocall/unity/Builds/iOS/`

#### Option B: Using Build Settings

1. **File > Build Settings**
2. **Platform: iOS** (should be selected from Step 14)
3. **Click "Build"**
4. **Choose output:** `holocall/unity/Builds/iOS`
5. **Wait** (5-10 minutes)

### Step 16: Build in Xcode (20 minutes)

```bash
# Open Xcode project
cd unity/Builds/iOS
open Unity-iPhone.xcodeproj
```

**In Xcode:**

1. **Select "Unity-iPhone" project** (left sidebar)
2. **General tab:**
   - **Bundle Identifier:** com.yourcompany.holocall
   - **Team:** Select your Apple Developer team
   - **Automatically manage signing:** ✅ Check

3. **Signing & Capabilities tab:**
   - **Signing:** Should show green checkmark
   - **Click "+ Capability"** and add:
     - ✅ Camera (should be auto-added)
     - ✅ Microphone (should be auto-added)

4. **Connect iPhone/iPad** via USB
5. **Select your device** in Xcode toolbar (top)
6. **Click Run (▶️)** or **Product > Run**
7. **Wait** (5-10 minutes first time)

**On iOS device:**
- If prompted, **trust developer certificate:**
  - **Settings > General > Device Management**
  - **Trust your developer account**

8. **App launches** on device!

### Step 17: Test iOS Build

1. **Open HoloCall app** (auto-launched from Xcode)
2. **Grant permissions:**
   - Camera
   - Microphone
3. **Enter credentials**
4. **Create or join room**
5. **Point at floor/table** - should see AR plane detection
6. **Tap to place hologram**

---

## Testing

### Test Scenario 1: Desktop ↔ Desktop

**Requirements:** 2 computers with builds

1. **Computer 1:**
   - Run HoloCall.exe
   - Enter email: alice@test.com
   - Create Room
   - Note Room ID

2. **Computer 2:**
   - Run HoloCall.exe
   - Enter email: bob@test.com
   - Join Room (enter Room ID from Computer 1)

3. **Expected:**
   - Both should connect
   - (Holograms may not render without WebRTC integration)
   - Check Console logs for "WebSocket connected"

### Test Scenario 2: Desktop ↔ Mobile

**Requirements:** 1 desktop build + 1 mobile device

1. **Desktop:**
   - Create room as above

2. **Mobile:**
   - Open app
   - Join same room
   - Grant AR permissions
   - Point at floor
   - Tap to place hologram

3. **Expected:**
   - Connection established
   - AR planes detected on mobile
   - Hologram placed (currently placeholder)

### Test Scenario 3: Mobile ↔ Mobile

**Requirements:** 2 mobile devices

1. **Phone 1:** Create room
2. **Phone 2:** Join room
3. **Both:** Place holograms in AR

---

## Troubleshooting

### Unity Issues

**"Package not found: com.unity.xr.arfoundation"**
```
Solution:
1. Window > Package Manager
2. Change to "Unity Registry"
3. Search "AR Foundation"
4. Install
```

**"websocket-sharp.dll not found"**
```
Solution:
1. Download from GitHub (see Step 5)
2. Place in Assets/Plugins/
3. Restart Unity
```

**"Scene not found in build"**
```
Solution:
1. File > Build Settings
2. Click "Add Open Scenes"
3. Ensure MainScene.unity is checked
```

### Android Issues

**"Android SDK not found"**
```
Solution:
1. Install Android Studio
2. Unity > Edit > Preferences > External Tools
3. Set Android SDK path
```

**"adb: command not found"**
```
Solution:
Add to PATH:
export PATH=$PATH:~/Library/Android/sdk/platform-tools  # Mac/Linux
# or add to System Environment Variables (Windows)
```

**"App won't install: INSTALL_FAILED_UPDATE_INCOMPATIBLE"**
```
Solution:
Uninstall old version first:
adb uninstall com.yourcompany.holocall
Then install again
```

### iOS Issues

**"Code signing required"**
```
Solution:
1. Xcode > Preferences > Accounts
2. Add Apple ID
3. Select team in project settings
```

**"iOS deployment target not supported"**
```
Solution:
In Xcode:
1. Select Unity-iPhone target
2. General > Deployment Info
3. Set to iOS 13.0 or higher
```

### Backend Issues

**"Cannot connect to ws://localhost:8080"**
```
Solution:
1. Verify backend running: curl http://localhost:8080/health
2. If not, restart: docker-compose restart backend
3. Check logs: docker-compose logs backend
```

**"Connection refused"**
```
Solution:
1. Check firewall allows port 8080
2. If on different machine, use IP instead of localhost
3. Edit Unity: Signaling Server URL: ws://192.168.1.X:8080/ws
```

---

## Next Steps

### Current Status
✅ Backend running
✅ Unity project compiling
✅ Builds working for Desktop/Android/iOS
⚠️ WebRTC peer connections need integration
⚠️ Hologram rendering needs completion

### To Complete Full Integration:

1. **Integrate Unity WebRTC Package** (4-8 hours)
   - Install com.unity.webrtc
   - Connect to NetworkManager
   - Implement peer connection lifecycle

2. **Complete Hologram Rendering** (4-6 hours)
   - Point cloud renderer (Desktop)
   - Avatar mesh renderer (Mobile/VR)
   - Network synchronization

3. **Test with Real Devices** (1-2 days)
   - Test depth cameras
   - Test mobile face tracking
   - Performance optimization

**Estimated time to working demo:** 1-2 weeks of development

---

## Summary

You now have:
- ✅ Working backend infrastructure
- ✅ Unity project that compiles
- ✅ Ability to build for all platforms
- ✅ Foundation for WebRTC integration

**What works:**
- Backend services
- Authentication
- Room creation/joining
- AR plane detection (mobile)
- Build pipeline

**What needs work:**
- WebRTC peer connections
- Hologram rendering
- Avatar animation
- Full end-to-end testing

**Time invested:** ~2 hours setup
**Remaining work:** ~1-2 weeks integration

---

## Resources

- **Full Setup:** [docs/SETUP.md](SETUP.md)
- **Architecture:** [docs/ARCHITECTURE.md](ARCHITECTURE.md)
- **Backend API:** [docs/API.md](API.md)
- **Demo Scenarios:** [docs/DEMO.md](DEMO.md)

---

**Questions or issues?**
Open an issue on GitHub or check the troubleshooting guide!

🚀 **Happy building!**
