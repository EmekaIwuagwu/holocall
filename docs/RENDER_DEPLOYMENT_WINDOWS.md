# 🌐 HoloCall - Deploy Backend to Render + Test on Windows

**Complete guide to deploy HoloCall backend on Render.com and test with Unity client on Windows**

**Time Required:** 1-2 hours total
- Backend deployment: 30 minutes
- Unity setup: 45 minutes
- Testing: 15 minutes

---

## ⚠️ Important: Unity is REQUIRED

**Question: Can I use Visual Studio instead of Unity?**

**Answer: NO** - Here's why:

- **Unity** = Game engine + build environment (REQUIRED)
  - Compiles C# code for Desktop/Android/iOS
  - Includes rendering engine, AR Foundation, scene management
  - **You MUST install Unity** to build HoloCall client

- **Visual Studio** = Code editor only (OPTIONAL)
  - Used for writing/editing C# scripts
  - Cannot build Unity projects
  - Unity can use VS Code, Visual Studio, or Rider as editor

**Summary:** You need **Unity 2022.3 LTS** to build the client. Visual Studio is just for editing code.

---

## 📋 Table of Contents

1. [Deploy Backend to Render](#part-1-deploy-backend-to-render)
2. [Setup Unity on Windows](#part-2-setup-unity-on-windows)
3. [Connect Unity Client to Render Backend](#part-3-connect-to-render-backend)
4. [Build and Test on Windows](#part-4-build-and-test)
5. [Troubleshooting](#troubleshooting)

---

## Part 1: Deploy Backend to Render

### Prerequisites

- GitHub account
- Render.com account (free tier works)
- HoloCall repository

### Step 1.1: Prepare Repository for Render (5 minutes)

Render needs specific configuration files. Let me create them:

**Create `render.yaml` in repository root:**

```yaml
# render.yaml - Render Blueprint for HoloCall
services:
  # Backend Node.js service
  - type: web
    name: holocall-backend
    env: node
    region: oregon
    plan: free
    buildCommand: cd backend && npm install && npm run build
    startCommand: cd backend && npm start
    envVars:
      - key: NODE_ENV
        value: production
      - key: PORT
        value: 8080
      - key: JWT_SECRET
        generateValue: true
      - key: JWT_REFRESH_SECRET
        generateValue: true
      - key: TURN_SECRET
        generateValue: true
      - key: DATABASE_URL
        fromDatabase:
          name: holocall-postgres
          property: connectionString
      - key: REDIS_URL
        fromService:
          type: redis
          name: holocall-redis
          property: connectionString

  # PostgreSQL database
  - type: pserv
    name: holocall-postgres
    env: docker
    plan: free
    region: oregon

  # Redis cache
  - type: redis
    name: holocall-redis
    plan: free
    region: oregon
    maxmemoryPolicy: allkeys-lru
```

**However, for simplicity, let's do manual deployment first:**

### Step 1.2: Create Web Service on Render (10 minutes)

1. **Go to:** https://render.com
2. **Sign up/Login** with GitHub
3. **Click "New +"** → **"Web Service"**
4. **Connect Repository:**
   - Click "Connect account" (if not connected)
   - Find `holocall` repository
   - Click "Connect"

5. **Configure Service:**

   ```
   Name: holocall-backend
   Region: Oregon (or closest to you)
   Branch: main (or your branch)
   Root Directory: backend
   Runtime: Node
   Build Command: npm install && npm run build
   Start Command: npm start
   Plan: Free
   ```

6. **Add Environment Variables:**

   Click "Advanced" → "Add Environment Variable":

   ```
   NODE_ENV = production
   PORT = 8080

   JWT_SECRET = [Click "Generate" or paste: your-secret-32-chars-min]
   JWT_REFRESH_SECRET = [Click "Generate" or paste: your-secret-32-chars-min]
   TURN_SECRET = [Click "Generate" or paste: your-secret-32-chars-min]

   # We'll add database later
   ```

   **Generate secrets** on your computer:
   ```bash
   # Windows PowerShell
   -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})
   # Run 3 times for 3 different secrets
   ```

7. **Click "Create Web Service"**

8. **Wait for deploy** (5-10 minutes)

### Step 1.3: Add PostgreSQL Database (10 minutes)

1. **In Render Dashboard**, click "New +" → "PostgreSQL"

2. **Configure Database:**
   ```
   Name: holocall-postgres
   Database: holocall
   User: holocall
   Region: Oregon (same as backend)
   Plan: Free
   ```

3. **Click "Create Database"**

4. **Copy Connection String:**
   - Once created, go to database page
   - Copy "External Database URL" (looks like: `postgres://user:pass@host/db`)

5. **Add to Backend Service:**
   - Go back to `holocall-backend` service
   - Click "Environment" tab
   - Add:
     ```
     DATABASE_URL = [paste connection string]
     ```
   - Click "Save Changes"
   - Backend will auto-redeploy

### Step 1.4: Test Backend Deployment (2 minutes)

1. **Get your backend URL:**
   - On backend service page: `https://holocall-backend-XXXX.onrender.com`

2. **Test health endpoint:**
   ```bash
   # Windows PowerShell
   Invoke-WebRequest -Uri "https://holocall-backend-XXXX.onrender.com/health"

   # Or use browser:
   # Visit: https://holocall-backend-XXXX.onrender.com/health
   ```

   **Expected Response:**
   ```json
   {"status":"ok","timestamp":"2024-...","uptime":123.45}
   ```

✅ **Backend is deployed!** Note your URL: `https://holocall-backend-XXXX.onrender.com`

---

## Part 2: Setup Unity on Windows

### Prerequisites

- Windows 10/11 (64-bit)
- 20 GB free disk space
- Administrator access

### Step 2.1: Install Unity Hub (10 minutes)

1. **Download Unity Hub:**
   - Visit: https://unity.com/download
   - Click "Download Unity Hub"
   - Run installer: `UnityHubSetup.exe`

2. **Install Unity Hub:**
   - Click "I agree" to terms
   - Choose install location (default is fine)
   - Click "Install"

3. **Launch Unity Hub**

4. **Create Unity Account** (if you don't have one):
   - Click "Sign In"
   - Click "Create account"
   - Follow steps

5. **Activate License:**
   - Click "Preferences" (gear icon)
   - Click "Licenses"
   - Click "Add"
   - Choose "Get a free personal license"
   - Follow steps

### Step 2.2: Install Unity Editor 2022.3 LTS (20 minutes)

1. **In Unity Hub**, click **"Installs"** (left sidebar)

2. **Click "Install Editor"**

3. **Choose Version:**
   - **Select: "2022.3.10f1"** (or latest 2022.3.x LTS)
   - If not visible, click "Archive" → "download archive" → find 2022.3 LTS

4. **Add Modules** (IMPORTANT):

   Check these boxes:

   **Platforms:**
   - ✅ **Android Build Support**
     - ✅ Android SDK & NDK Tools
     - ✅ OpenJDK
   - ✅ **Windows Build Support (IL2CPP)**

   **Documentation:**
   - ✅ Documentation (optional but helpful)

   **Development Tools:**
   - ✅ Microsoft Visual Studio Community 2022 (if you want code editor)

5. **Click "Continue"**

6. **Accept licenses** and click "Install"

7. **Wait for installation** (15-30 minutes depending on internet)

### Step 2.3: Clone HoloCall Repository (5 minutes)

**Option A: Using Git (if installed)**

```bash
# Open PowerShell or Git Bash
cd C:\Users\YourName\Documents

# Clone repository
git clone https://github.com/yourusername/holocall.git
cd holocall
```

**Option B: Download ZIP**

1. Go to GitHub repository
2. Click "Code" → "Download ZIP"
3. Extract to: `C:\Users\YourName\Documents\holocall`

### Step 2.4: Open Project in Unity (10 minutes)

1. **In Unity Hub**, click **"Projects"** (left sidebar)

2. **Click "Add"** (top right)

3. **Navigate to:** `C:\Users\YourName\Documents\holocall\unity`

4. **Click "Select Folder"**

5. **Unity will open and import assets** (5-10 minutes first time)

   You'll see:
   - "Importing package..."
   - "Compiling scripts..."
   - Progress bar

6. **Wait until Unity Editor opens fully**

### Step 2.5: Download WebSocket Library (5 minutes)

Unity needs WebSocket-Sharp DLL for signaling:

**Option A: Using PowerShell**

```powershell
# Open PowerShell as Administrator
cd C:\Users\YourName\Documents\holocall\unity\Assets\Plugins

# Download WebSocket-Sharp DLL
Invoke-WebRequest -Uri "https://github.com/sta/websocket-sharp/releases/download/1.0.3-rc11/websocket-sharp.dll" -OutFile "websocket-sharp.dll"
```

**Option B: Manual Download**

1. **Visit:** https://github.com/sta/websocket-sharp/releases
2. **Download:** `websocket-sharp.dll` (from latest release, currently 1.0.3-rc11)
3. **Save to:** `C:\Users\YourName\Documents\holocall\unity\Assets\Plugins\websocket-sharp.dll`

**Verify in Unity:**

1. In Unity, check **Project** tab (bottom)
2. Navigate to: **Assets/Plugins**
3. You should see: `websocket-sharp.dll`
4. If not, refresh: Right-click in Project → "Refresh"

✅ **Unity is ready!**

---

## Part 3: Connect to Render Backend

### Step 3.1: Update Signaling Server URL (2 minutes)

1. **In Unity**, open scene:
   - **Project tab** → **Assets/Scenes**
   - **Double-click:** `MainScene.unity`

2. **In Hierarchy** (left panel), select: **HoloCallManager**

3. **In Inspector** (right panel), find:
   ```
   Signaling Server URL: ws://localhost:8080/ws
   ```

4. **Change to your Render URL:**
   ```
   wss://holocall-backend-XXXX.onrender.com/ws
   ```

   **Important:** Use `wss://` (secure WebSocket), not `ws://`

5. **Save Scene:** `Ctrl + S`

### Step 3.2: Update Backend for WebSocket (Important)

Your Render backend needs to support WebSocket. Verify `backend/src/index.ts` has:

```typescript
// This should already be in your code
const server = createServer(app);
const wss = new WebSocketServer({ server, path: '/ws' });
```

If using Render, WebSocket should work automatically on the HTTP port.

---

## Part 4: Build and Test

### Step 4.1: Test in Unity Editor First (5 minutes)

1. **In Unity Editor**, click **Play** button (top center)

2. **In Game view**, you should see HoloCall UI

3. **Enter test credentials:**
   - Email: `test@holocall.com`
   - Display Name: `Test User`

4. **Click "Authenticate"**

5. **Check Console** (bottom panel):
   - Should see: "WebSocket connected"
   - If errors, check backend URL

6. **Click "Create Room"**
   - Should get Room ID
   - Console should show: "Room created: XXXXXXXX"

✅ **If this works, backend integration is successful!**

### Step 4.2: Build Windows Desktop App (15 minutes)

#### Option A: Using Build Menu

1. **In Unity**, go to menu: **HoloCall > Build > Desktop > Windows 64-bit**

2. **Wait for build** (5-10 minutes)

3. **Build completes:**
   - Location: `holocall\unity\Builds\Desktop\Windows\HoloCall.exe`
   - Unity may open folder automatically

#### Option B: Using Build Settings

1. **File > Build Settings**

2. **Verify:**
   - ✅ Scene: `Assets/Scenes/MainScene.unity` is in list
   - Platform: **PC, Mac & Linux Standalone** is selected
   - Target Platform: **Windows**
   - Architecture: **x86_64**

3. **Click "Build"**

4. **Choose folder:** Browse to `Builds\Desktop\Windows`

5. **Click "Select Folder"**

6. **Wait** (5-10 minutes)

### Step 4.3: Test Desktop Build (5 minutes)

1. **Navigate to build folder:**
   ```
   C:\Users\YourName\Documents\holocall\unity\Builds\Desktop\Windows
   ```

2. **Double-click:** `HoloCall.exe`

3. **Test same as Step 4.1:**
   - Enter credentials
   - Authenticate
   - Create room

4. **Test with 2 instances:**
   - Run `HoloCall.exe` twice (separate windows)
   - Instance 1: Create room
   - Instance 2: Join room (enter Room ID from Instance 1)
   - Both should connect!

---

## Testing Checklist

### ✅ Backend Tests

Test these URLs in browser (replace `XXXX` with your Render ID):

```
✅ Health: https://holocall-backend-XXXX.onrender.com/health
✅ CORS: Should allow requests from any origin
```

### ✅ Unity Editor Tests

```
✅ Play mode works
✅ Can authenticate (enter email/name)
✅ Console shows "WebSocket connected"
✅ Can create room (gets Room ID)
✅ No errors in Console
```

### ✅ Desktop Build Tests

```
✅ Executable runs
✅ Can authenticate
✅ Can create room
✅ Two instances can connect
✅ Room ID works for joining
```

---

## File Structure After Setup

```
C:\Users\YourName\Documents\holocall\
├── backend/                      (not needed locally, on Render)
├── unity/
│   ├── Assets/
│   │   ├── Plugins/
│   │   │   └── websocket-sharp.dll    ← Downloaded manually
│   │   ├── Scenes/
│   │   │   └── MainScene.unity         ← Configured with Render URL
│   │   └── Scripts/                    ← Your C# code
│   ├── Builds/
│   │   └── Desktop/
│   │       └── Windows/
│   │           └── HoloCall.exe        ← Built executable
│   └── Packages/
│       └── manifest.json               ← Package dependencies
└── docs/                               ← Documentation
```

---

## Troubleshooting

### Backend Issues

**Problem: "Health endpoint returns error"**

```
Solution:
1. Check Render dashboard for deploy status
2. View logs: Render dashboard → Service → Logs tab
3. Verify environment variables are set
4. Check build command succeeded
```

**Problem: "WebSocket connection refused"**

```
Solution:
1. Ensure using wss:// (not ws://) for Render
2. Check Render logs for WebSocket errors
3. Verify path is /ws in both backend and Unity
4. Test with: https://www.websocket.org/echo.html
   URL: wss://your-backend.onrender.com/ws
```

### Unity Issues

**Problem: "Unity won't open project"**

```
Solution:
1. Verify Unity version is 2022.3.x
2. Check project path has no special characters
3. Delete Library folder and reopen (Unity will rebuild)
4. Check Unity Hub → Preferences → Installs shows 2022.3.x
```

**Problem: "Cannot find websocket-sharp.dll"**

```
Solution:
1. Re-download DLL from GitHub releases
2. Verify placed in: Assets/Plugins/websocket-sharp.dll
3. In Unity, right-click Assets → Refresh
4. Check DLL Inspector settings: All platforms checked
```

**Problem: "Build fails with errors"**

```
Solution:
1. Check Console for specific errors
2. Verify all scripts compile (no red errors)
3. Try: Edit → Preferences → External Tools → Regenerate project files
4. Clean build: HoloCall → Clean Builds, then rebuild
```

**Problem: "WebSocket not connecting in build"**

```
Solution:
1. Verify Render URL is correct in MainScene
2. Check Windows Firewall isn't blocking
3. Test URL in browser first
4. Check Render backend is running (not sleeping on free tier)
```

### Render-Specific Issues

**Problem: "Free tier backend sleeps"**

```
Note: Render free tier sleeps after 15 minutes of inactivity
- First request after sleep takes 30-60 seconds to wake up
- Solution: Upgrade to paid tier, or accept the delay

Workaround: Keep backend alive
- Use a service like UptimeRobot to ping every 5 minutes
- Ping: https://your-backend.onrender.com/health
```

**Problem: "Database connection fails"**

```
Solution:
1. Verify DATABASE_URL is set in environment variables
2. Check database is in same region as backend
3. View database logs in Render dashboard
4. Test connection string format: postgres://user:pass@host:5432/db
```

---

## Next Steps

### You Now Have:
✅ Backend deployed on Render
✅ Unity project configured on Windows
✅ Desktop build that connects to cloud backend
✅ Ability to test 1:1 calls

### To Complete Full Functionality:

**Short-term (1-2 weeks):**
1. Integrate Unity WebRTC package for peer connections
2. Implement hologram rendering (point cloud/avatar)
3. Test with 2+ users

**Medium-term (3-4 weeks):**
4. Build Android APK and test on phone
5. Add face tracking for avatars
6. Optimize performance

**Long-term (2-3 months):**
7. Deploy to production
8. Add advanced features (recording, screen share)
9. App Store submissions

---

## Quick Command Reference

### PowerShell Commands (Windows)

```powershell
# Generate secret
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})

# Test backend
Invoke-WebRequest -Uri "https://your-backend.onrender.com/health"

# Download WebSocket DLL
cd C:\Users\YourName\Documents\holocall\unity\Assets\Plugins
Invoke-WebRequest -Uri "https://github.com/sta/websocket-sharp/releases/download/1.0.3-rc11/websocket-sharp.dll" -OutFile "websocket-sharp.dll"

# Clone repository
cd C:\Users\YourName\Documents
git clone https://github.com/yourusername/holocall.git
```

### Unity Build Menu

```
HoloCall/
├── Build/
│   └── Desktop/
│       └── Windows 64-bit     ← Build .exe
├── Open Build Folder          ← Open Builds/ folder
├── Clean Builds              ← Delete old builds
└── Validate Build Settings   ← Check configuration
```

---

## Summary

**Total Time:** ~2 hours

**What You Did:**
1. ✅ Deployed backend to Render (free tier)
2. ✅ Installed Unity 2022.3 LTS on Windows
3. ✅ Configured Unity project
4. ✅ Built Windows desktop app
5. ✅ Connected to cloud backend
6. ✅ Tested 1:1 call locally

**Cost:** $0 (using free tiers)

**You Can Now:**
- Create/join rooms via cloud backend
- Test with multiple desktop instances
- Share your Render URL with others for testing

**You Cannot Yet:**
- See actual holograms (needs WebRTC integration)
- Use on mobile (needs Android/iOS build + testing)
- Have full production features

---

## Resources

- **Render Docs:** https://render.com/docs
- **Unity Docs:** https://docs.unity3d.com
- **HoloCall Docs:** `holocall/docs/`
- **WebSocket-Sharp:** https://github.com/sta/websocket-sharp

---

**Ready to test? Follow the steps above and you'll have a working deployment!** 🚀

Any issues? Check the Troubleshooting section or open a GitHub issue.
