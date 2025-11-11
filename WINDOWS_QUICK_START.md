# 🪟 Run HoloCall on Windows - Quick Guide

**Scenario:** Backend deployed on Render ✅ → Now run Unity client on Windows

**Time:** 1 hour total

---

## ✅ Prerequisites

Before starting, you need:
- ✅ Backend deployed on Render (get the URL: `https://holocall-backend-XXXX.onrender.com`)
- ✅ Windows 10/11 (64-bit)
- ✅ 20 GB free disk space
- ✅ Good internet connection (for Unity download)
- ✅ HoloCall repository on your computer

---

## 📦 Part 1: Install Unity (40 minutes)

### Step 1: Download Unity Hub (5 minutes)

**Open PowerShell or Command Prompt and run:**

```powershell
# Or just download from browser
start https://unity.com/download
```

1. Click **"Download Unity Hub for Windows"**
2. Run the installer: `UnityHubSetup.exe`
3. Click through installation (all defaults are fine)
4. **Launch Unity Hub** when done

### Step 2: Create Unity Account (5 minutes)

1. In Unity Hub, click **"Sign In"** (top right)
2. Click **"Create account"**
3. Fill in email/password
4. Verify email
5. Sign in to Unity Hub

### Step 3: Activate Free License (2 minutes)

1. Click **Settings** ⚙️ (bottom left)
2. Click **"Licenses"** tab
3. Click **"Add"** → **"Get a free personal license"**
4. Click **"Agree and get personal edition license"**
5. Done! ✅

### Step 4: Install Unity Editor 2022.3 LTS (25 minutes)

1. Click **"Installs"** tab (left sidebar)
2. Click **"Install Editor"** (top right)
3. Find **"2022.3.10f1"** or latest **2022.3.x LTS**
4. Click **"Install"**

**IMPORTANT - Select these modules:**

```
✅ Microsoft Visual Studio Community 2022 (optional - for code editing)
✅ Android Build Support (if you want mobile later)
   ✅ Android SDK & NDK Tools
   ✅ OpenJDK
✅ Windows Build Support (IL2CPP)
```

5. Click **"Continue"**
6. Accept license agreements
7. Click **"Install"**
8. ☕ **Wait 20-30 minutes** (large download)

---

## 📁 Part 2: Setup Project (15 minutes)

### Step 5: Get HoloCall Repository

**Option A: If you have Git installed**

```powershell
# Open PowerShell
cd C:\Users\$env:USERNAME\Documents

# Clone repository
git clone https://github.com/EmekaIwuagwu/holocall.git
cd holocall
```

**Option B: Download ZIP**

1. Go to: https://github.com/EmekaIwuagwu/holocall
2. Click **"Code"** → **"Download ZIP"**
3. Extract to: `C:\Users\YourName\Documents\holocall`

### Step 6: Open Project in Unity (10 minutes)

1. **In Unity Hub**, click **"Projects"** tab
2. Click **"Add"** (or **"Open"** dropdown → **"Add project from disk"**)
3. Browse to: `C:\Users\YourName\Documents\holocall\unity`
4. Click **"Add Project"** (or **"Select Folder"**)
5. Unity will open and import packages (5-10 minutes)
   - You'll see "Importing..." and progress bars
   - **Wait until Unity Editor fully loads**

### Step 7: Download WebSocket Library (5 minutes)

Unity needs WebSocket-Sharp DLL. Open **PowerShell as Administrator**:

```powershell
# Navigate to Plugins folder
cd C:\Users\$env:USERNAME\Documents\holocall\unity\Assets\Plugins

# Download WebSocket-Sharp DLL
Invoke-WebRequest -Uri "https://github.com/sta/websocket-sharp/releases/download/1.0.3-rc11/websocket-sharp.dll" -OutFile "websocket-sharp.dll"
```

**Verify in Unity:**
1. In Unity, look at **Project** tab (bottom)
2. Navigate: **Assets → Plugins**
3. You should see: `websocket-sharp.dll`
4. If not visible: **Right-click in Project → Refresh**

---

## 🔗 Part 3: Connect to Render Backend (5 minutes)

### Step 8: Configure Backend URL

1. **In Unity**, open scene:
   - **Project tab** → **Assets** → **Scenes**
   - **Double-click**: `MainScene.unity`

2. **In Hierarchy** (left panel), click: **HoloCallManager**

3. **In Inspector** (right panel), find:
   ```
   Signaling Server URL: ws://localhost:8080/ws
   ```

4. **Change to your Render URL:**
   ```
   wss://holocall-backend-XXXX.onrender.com/ws
   ```

   **IMPORTANT:**
   - Use `wss://` (secure WebSocket), not `ws://`
   - Replace `XXXX` with your Render app ID
   - Keep `/ws` at the end

5. **Save Scene**: Press `Ctrl + S`

---

## ▶️ Part 4: Test in Unity Editor (5 minutes)

### Step 9: Play Mode Test

1. **In Unity**, click the **Play ▶️** button (top center)

2. **Game view** appears with HoloCall interface

3. **Test authentication:**
   - Email: `test@example.com`
   - Display Name: `Test User`
   - Click **"Authenticate"**

4. **Check Console** (bottom panel):
   - Should see: `"[NetworkManager] WebSocket connected"`
   - If errors, check backend URL

5. **Test room creation:**
   - Click **"Create Room"**
   - You should get a Room ID (e.g., `ABC12345`)
   - Console shows: `"Room created: ABC12345"`

6. **Stop Play Mode**: Click **Play ▶️** button again

✅ **If this works, your connection to Render backend is successful!**

---

## 🔨 Part 5: Build Desktop Application (15 minutes)

### Step 10: Build Windows EXE

**Method 1: Using Build Menu (Easiest)**

1. **In Unity**, top menu: **HoloCall → Build → Desktop → Windows 64-bit**
2. Unity will build (5-10 minutes)
3. Build location: `holocall\unity\Builds\Desktop\Windows\HoloCall.exe`

**Method 2: Using Build Settings**

1. **File → Build Settings**
2. **Verify scene is added:**
   - Check if `Scenes/MainScene` is listed
   - If not: Click **"Add Open Scenes"**
3. **Platform**: Should be **"PC, Mac & Linux Standalone"**
4. **Target Platform**: **Windows**
5. **Architecture**: **x86_64**
6. Click **"Build"**
7. Choose folder: `Builds\Desktop\Windows`
8. Click **"Select Folder"**
9. Wait (5-10 minutes)

---

## 🎮 Part 6: Test Desktop App (10 minutes)

### Step 11: Run the Application

1. **Navigate to:**
   ```
   C:\Users\YourName\Documents\holocall\unity\Builds\Desktop\Windows
   ```

2. **Double-click:** `HoloCall.exe`

3. **Test same as Unity Editor:**
   - Enter email and name
   - Click "Authenticate"
   - Create room
   - Get Room ID

### Step 12: Test with Two Instances (2-Player Test)

1. **Run `HoloCall.exe` TWICE** (open two windows)

**Window 1 (Player 1):**
```
Email: player1@test.com
Name: Player One
Click: "Authenticate"
Click: "Create Room"
Note the Room ID: ABC12345
```

**Window 2 (Player 2):**
```
Email: player2@test.com
Name: Player Two
Click: "Authenticate"
Type Room ID: ABC12345
Click: "Join Room"
```

**Expected Result:**
- ✅ Both windows connect
- ✅ Console logs show both users in room
- ⚠️ Holograms won't render yet (needs WebRTC integration)

---

## 🎯 Success Checklist

After following this guide, you should have:

### ✅ Setup Complete
- [x] Unity 2022.3 LTS installed
- [x] HoloCall project opens without errors
- [x] WebSocket-Sharp DLL in Plugins folder
- [x] Backend URL configured to Render

### ✅ Testing Complete
- [x] Play mode works in Unity Editor
- [x] Can authenticate
- [x] Console shows "WebSocket connected"
- [x] Can create room and get Room ID
- [x] Desktop .exe builds successfully
- [x] Desktop app runs

### ✅ Multi-User Test
- [x] Two instances can run simultaneously
- [x] Player 1 creates room
- [x] Player 2 joins room
- [x] Both see connection in console

---

## 🐛 Troubleshooting

### Problem: "Unity won't open project"

**Solution:**
```powershell
# Check Unity version
# In Unity Hub → Installs
# Must be 2022.3.x (not 2021 or 2023)

# If wrong version, install 2022.3.10f1
```

### Problem: "Cannot find websocket-sharp.dll"

**Solution:**
```powershell
# Re-download manually
cd C:\Users\$env:USERNAME\Documents\holocall\unity\Assets\Plugins

# Download again
Invoke-WebRequest -Uri "https://github.com/sta/websocket-sharp/releases/download/1.0.3-rc11/websocket-sharp.dll" -OutFile "websocket-sharp.dll"

# In Unity: Right-click Project → Refresh
```

### Problem: "WebSocket connection failed"

**Check these:**

1. **Backend URL is correct:**
   ```
   ✅ wss://holocall-backend-XXXX.onrender.com/ws
   ❌ ws://holocall-backend-XXXX.onrender.com/ws  (wrong - not secure)
   ❌ https://holocall-backend-XXXX.onrender.com/ws (wrong - not WebSocket)
   ```

2. **Backend is running:**
   ```powershell
   # Test in browser or PowerShell
   Invoke-WebRequest -Uri "https://holocall-backend-XXXX.onrender.com/health"

   # Should return: {"status":"ok"}
   ```

3. **Render free tier:**
   - Free tier sleeps after 15 min inactivity
   - First request takes 30-60 seconds to wake up
   - Try again after waiting 1 minute

### Problem: "Build fails with errors"

**Solution:**
```
1. Check Console for red errors
2. Fix any script errors first
3. Try: Edit → Preferences → External Tools → Regenerate project files
4. If persists: HoloCall → Clean Builds, then rebuild
```

### Problem: "Application won't launch"

**Solution:**
```
1. Check Windows Defender didn't block it
2. Run as Administrator
3. Check antivirus settings
4. Rebuild with: HoloCall → Build → Desktop → Windows 64-bit
```

---

## 📊 What Works Now vs. What's Next

### ✅ What Works NOW (After This Guide)

- Backend hosted on Render
- Unity project compiles
- Desktop app builds and runs
- Authentication works
- Room creation/joining works
- Multi-user connections work
- WebSocket signaling works

### ⚠️ What Doesn't Work Yet (Needs 1-2 weeks)

- Hologram rendering (you won't see other users yet)
- Point cloud display
- Avatar display
- Face tracking rendering
- Full 3D visualization

### 🔄 To Complete Full Functionality:

**Next Steps (Estimated 1-2 weeks of development):**

1. **Install Unity WebRTC Package** (4-8 hours)
   - Window → Package Manager
   - Add: `com.unity.webrtc`
   - Integrate with NetworkManager

2. **Implement Hologram Rendering** (1-2 weeks)
   - Point cloud renderer (Desktop)
   - Avatar renderer (Mobile)
   - Network synchronization

3. **Test with Real Hardware**
   - Depth cameras (RealSense/Kinect)
   - Mobile phones
   - Optimization

---

## 🎯 Quick Command Reference

### PowerShell Commands

```powershell
# Navigate to project
cd C:\Users\$env:USERNAME\Documents\holocall

# Download WebSocket DLL
cd unity\Assets\Plugins
Invoke-WebRequest -Uri "https://github.com/sta/websocket-sharp/releases/download/1.0.3-rc11/websocket-sharp.dll" -OutFile "websocket-sharp.dll"

# Test backend
Invoke-WebRequest -Uri "https://your-backend.onrender.com/health"

# Open project folder
explorer C:\Users\$env:USERNAME\Documents\holocall

# Open build folder
explorer unity\Builds\Desktop\Windows
```

### Unity Shortcuts

```
Ctrl + S          = Save Scene
Ctrl + P          = Play/Stop
Ctrl + Shift + B  = Build Settings
F               = Focus on selected object
```

---

## 📖 Related Guides

- **Full Deployment Guide**: `docs/RENDER_DEPLOYMENT_WINDOWS.md`
- **Complete Setup**: `docs/SETUP.md`
- **Assembly Guide**: `docs/ASSEMBLY_GUIDE.md`
- **Demo Scenarios**: `docs/DEMO.md`

---

## ✅ You're Done!

**Congratulations!** 🎉 You now have:

1. ✅ Backend running on Render (cloud)
2. ✅ Unity installed on Windows
3. ✅ HoloCall project configured
4. ✅ Desktop application built
5. ✅ Tested and working connections

**What you can do:**
- Test with multiple desktop instances
- Share your Render URL with others to test remotely
- Continue development to add hologram rendering

**Total time invested:** ~1-2 hours

**Cost:** $0 (all free tiers)

---

## 🚀 Next Steps

### Immediate:
1. Test with a friend (share your HoloCall.exe + Render URL)
2. Try creating/joining rooms
3. Verify connections work

### This Week:
4. Install Unity WebRTC package
5. Start implementing rendering
6. Test with webcam (no depth camera needed for initial tests)

### This Month:
7. Complete hologram rendering
8. Test with depth cameras
9. Build mobile versions

---

**Questions?** Check the troubleshooting section above or the full guides in `docs/` folder!

**Ready to see holograms?** Next step: Integrate WebRTC rendering! 🎭
