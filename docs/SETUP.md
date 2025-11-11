# 🔧 HoloCall Complete Setup Guide

This comprehensive guide walks you through setting up HoloCall for development and production.

## Table of Contents

- [System Requirements](#system-requirements)
- [Backend Setup](#backend-setup)
- [Unity Setup](#unity-setup)
- [Platform-Specific Setup](#platform-specific-setup)
- [Docker Setup](#docker-setup)
- [Production Deployment](#production-deployment)
- [Troubleshooting](#troubleshooting)

---

## System Requirements

### Development Machine

**Minimum:**
- OS: Windows 10, macOS 11, or Ubuntu 20.04
- CPU: Intel i5 8th gen / AMD Ryzen 5 2600
- RAM: 16 GB
- Storage: 20 GB free space
- GPU: Integrated graphics (for Unity Editor)
- Internet: 10 Mbps download/upload

**Recommended:**
- OS: Windows 11, macOS 13, or Ubuntu 22.04
- CPU: Intel i7 10th gen / AMD Ryzen 7 3700X
- RAM: 32 GB
- Storage: 50 GB SSD
- GPU: NVIDIA GTX 1660 / AMD RX 5600
- Internet: 50+ Mbps

### Backend Server

**Minimum (Development):**
- 2 vCPU
- 4 GB RAM
- 20 GB storage
- 100 Mbps network

**Recommended (Production):**
- 4+ vCPU
- 16 GB RAM
- 100 GB SSD
- 1 Gbps network
- Static public IP

---

## Backend Setup

### 1. Install Node.js

#### Windows
```bash
# Download from https://nodejs.org
# Or use winget
winget install OpenJS.NodeJS.LTS
```

#### macOS
```bash
# Using Homebrew
brew install node@18
```

#### Ubuntu
```bash
# Using NodeSource
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs
```

**Verify Installation:**
```bash
node --version  # Should be v18.x.x or higher
npm --version   # Should be 9.x.x or higher
```

### 2. Install PostgreSQL

#### Using Docker (Recommended)
```bash
docker run -d \
  --name holocall-postgres \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=holocall \
  -p 5432:5432 \
  postgres:15-alpine
```

#### Native Installation

**Ubuntu:**
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
```

**macOS:**
```bash
brew install postgresql@15
brew services start postgresql@15
```

**Windows:**
Download from https://www.postgresql.org/download/windows/

**Create Database:**
```bash
# Access PostgreSQL
psql -U postgres

# Create database and user
CREATE DATABASE holocall;
CREATE USER holocall WITH ENCRYPTED PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE holocall TO holocall;
\q
```

### 3. Install Redis

#### Using Docker (Recommended)
```bash
docker run -d \
  --name holocall-redis \
  -p 6379:6379 \
  redis:7-alpine
```

#### Native Installation

**Ubuntu:**
```bash
sudo apt update
sudo apt install redis-server
sudo systemctl start redis-server
```

**macOS:**
```bash
brew install redis
brew services start redis
```

**Windows:**
Download from https://github.com/microsoftarchive/redis/releases

### 4. Configure Backend

```bash
# Clone repository
git clone https://github.com/yourusername/holocall.git
cd holocall/backend

# Install dependencies
npm install

# Copy environment file
cp .env.example .env

# Edit configuration
nano .env
```

**Edit `.env`:**
```env
# Server
NODE_ENV=development
PORT=8080
HOST=0.0.0.0

# Database
DATABASE_URL=postgresql://holocall:your_password@localhost:5432/holocall

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# JWT (generate secure secrets)
JWT_SECRET=your-jwt-secret-min-32-chars
JWT_REFRESH_SECRET=your-refresh-secret-min-32-chars

# TURN Server
TURN_SECRET=your-turn-secret
TURN_SERVER_URL=turn:your-public-ip:3478
STUN_SERVER_URL=stun:stun.l.google.com:19302

# Mediasoup
MEDIASOUP_ANNOUNCED_IP=127.0.0.1  # Use public IP for production
```

**Generate Secure Secrets:**
```bash
# Generate JWT secret
openssl rand -base64 32

# Generate TURN secret
openssl rand -hex 32
```

### 5. Initialize Database

```bash
# Run migrations (creates tables)
npm run migrate

# Or manually run SQL
psql -U holocall -d holocall -f ../infrastructure/docker/init-db.sql
```

### 6. Start Backend

```bash
# Development mode (with hot reload)
npm run dev

# Production mode
npm run build
npm start
```

**Verify Backend:**
```bash
curl http://localhost:8080/health
# Should return: {"status":"ok","timestamp":"...","uptime":...}
```

---

## Unity Setup

### 1. Install Unity

#### Install Unity Hub

**Windows/macOS:**
Download from https://unity.com/download

**Ubuntu:**
```bash
# Download Unity Hub AppImage
wget https://public-cdn.cloud.unity3d.com/hub/prod/UnityHub.AppImage
chmod +x UnityHub.AppImage
./UnityHub.AppImage
```

#### Install Unity Editor 2022.3 LTS

1. Open Unity Hub
2. Go to "Installs" tab
3. Click "Install Editor"
4. Select "2022.3.x LTS"
5. Choose modules:
   - **Android Build Support**
     - Android SDK & NDK Tools
     - OpenJDK
   - **iOS Build Support** (macOS only)
   - **WebGL Build Support**
   - **Windows Build Support** (if on macOS/Linux)
   - **Linux Build Support** (if on Windows/macOS)

### 2. Open HoloCall Project

1. In Unity Hub, click "Add" (or "Open")
2. Navigate to `holocall/unity`
3. Select folder and click "Open"
4. Wait for Unity to import assets (may take 5-10 minutes first time)

### 3. Install Required Packages

Unity Package Manager should auto-import dependencies. If not:

1. Open Unity Editor
2. Go to **Window > Package Manager**
3. Install these packages:

**Required Packages:**
- AR Foundation 5.1+
- ARCore XR Plugin 5.1+ (Android)
- ARKit XR Plugin 5.1+ (iOS)
- XR Plugin Management
- Universal Render Pipeline (URP)
- TextMeshPro

**Optional Packages:**
- Oculus XR Plugin (for Quest)
- OpenXR Plugin (for cross-platform VR)
- Cinemachine (camera control)
- ProBuilder (level design)

### 4. Configure Project Settings

#### Player Settings

Go to **Edit > Project Settings > Player**

**All Platforms:**
- Company Name: `YourCompany`
- Product Name: `HoloCall`
- Version: `1.0.0`
- Scripting Backend: `IL2CPP`

**Android:**
- Package Name: `com.yourcompany.holocall`
- Minimum API Level: `Android 7.0 (API 24)`
- Target API Level: `Android 13 (API 33)` (or latest)
- Scripting Backend: `IL2CPP`
- Target Architectures: `ARM64` (required for ARCore)
- Internet Access: `Require`
- Write Permission: `External (SDCard)`

**iOS:**
- Bundle Identifier: `com.yourcompany.holocall`
- Target minimum iOS Version: `13.0`
- Architecture: `ARM64`
- Camera Usage Description: `Required for AR and face tracking`
- Microphone Usage Description: `Required for audio calls`

**Standalone (Desktop):**
- Architecture: `x86_64`
- Fullscreen Mode: `Windowed`
- Run In Background: `True`

#### XR Settings

Go to **Edit > Project Settings > XR Plug-in Management**

**Android:**
- [ ] ARCore
- Configure ARCore:
  - Depth: `Optional`
  - Face Tracking: `Optional`

**iOS:**
- [ ] ARKit
- Configure ARKit:
  - Face Tracking: `Required`
  - Depth API: `Automatic`

**Standalone (VR):**
- [ ] Oculus (for Quest development)
- [ ] OpenXR (for cross-platform VR)

#### Graphics Settings

Go to **Edit > Project Settings > Graphics**

- Scriptable Render Pipeline: `UniversalRenderPipelineAsset`
- Rendering Path: `Forward`
- HDR: `True`
- MSAA: `4x` (adjust based on target platform)

#### Quality Settings

Go to **Edit > Project Settings > Quality**

Create quality levels:
- **High** (Desktop): 60 FPS, high shadows, post-processing
- **Medium** (Mobile high-end): 60 FPS, medium shadows, basic post-processing
- **Low** (Mobile low-end): 30 FPS, no shadows, no post-processing
- **VR**: 90+ FPS, optimized shadows, minimal post-processing

### 5. Configure HoloCall Settings

1. Open Scene: `Assets/Scenes/MainScene.unity`
2. Select `HoloCallManager` GameObject in Hierarchy
3. In Inspector, configure:

```
Signaling Server URL: ws://localhost:8080/ws
(Or your server's IP: ws://192.168.1.100:8080/ws)

Platform: Auto (or force Desktop/Android/iOS/VR for testing)

Max Participants: 10 (Desktop), 4 (Android), 6 (iOS), 8 (VR)
```

### 6. Import WebRTC Package

HoloCall requires WebRTC for peer connections. Choose one:

**Option A: Unity WebRTC Package (Official)**
```bash
# Add via Package Manager
# Window > Package Manager
# + > Add package from git URL
https://github.com/Unity-Technologies/com.unity.webrtc.git
```

**Option B: WebSocket-Sharp (for signaling only)**
```bash
# Already included in project
# Assets/Plugins/WebSocket-Sharp.dll
```

**Option C: Custom Implementation**
- Use native WebRTC libraries per platform
- Wrap with C# bindings
- See `Assets/Plugins/` for platform-specific plugins

### 7. Test in Unity Editor

1. Click **Play** button
2. Enter test credentials:
   - Email: `test@holocall.com`
   - Display Name: `Test User`
3. Click "Authenticate"
4. Click "Create Room"
5. Note Room ID
6. Verify console logs show WebSocket connection

---

## Platform-Specific Setup

### Android Setup

#### 1. Install Android SDK

If not installed with Unity:

```bash
# Download Android Studio
https://developer.android.com/studio

# Or install command-line tools
https://developer.android.com/studio/index.html#command-tools
```

**Set Environment Variables:**

**Windows (PowerShell):**
```powershell
[System.Environment]::SetEnvironmentVariable("ANDROID_HOME", "C:\Users\YourUser\AppData\Local\Android\Sdk", "User")
```

**macOS/Linux:**
```bash
export ANDROID_HOME=$HOME/Library/Android/sdk  # macOS
export ANDROID_HOME=$HOME/Android/Sdk  # Linux
export PATH=$PATH:$ANDROID_HOME/tools:$ANDROID_HOME/platform-tools
```

#### 2. Configure Unity for Android

1. **File > Build Settings**
2. Platform: **Android**
3. Click **Switch Platform**
4. **Player Settings**:
   - Other Settings:
     - [ ] Auto Graphics API (uncheck)
     - Graphics APIs: `OpenGLES3`, `Vulkan`
     - Multithreaded Rendering: `True`
   - Publishing Settings:
     - Create new Keystore (for release builds)

#### 3. Build APK

```bash
# Debug build
File > Build Settings > Build

# Or use command line
unity-editor -quit -batchmode -projectPath ./unity -buildTarget Android -executeMethod Builder.BuildAndroid
```

#### 4. Install on Device

```bash
# Enable USB debugging on Android device
# Settings > About phone > Tap "Build number" 7 times
# Settings > Developer options > USB debugging

# Connect device via USB
adb devices

# Install APK
adb install holocall.apk

# View logs
adb logcat -s Unity
```

### iOS Setup (macOS only)

#### 1. Install Xcode

```bash
# From App Store or
xcode-select --install
```

#### 2. Configure Signing

1. **File > Build Settings > iOS > Player Settings**
2. **Other Settings > Identification**:
   - Signing Team ID: (select your Apple Developer team)
   - Automatically Sign: `True` (recommended)
   - Or manually configure provisioning profiles

#### 3. Build Xcode Project

1. **File > Build Settings > iOS**
2. Click **Build**
3. Choose output folder (e.g., `builds/ios`)
4. Wait for build to complete

#### 4. Open in Xcode

```bash
# Open generated Xcode project
open builds/ios/Unity-iPhone.xcodeproj
```

#### 5. Deploy to Device

1. Connect iPhone/iPad via USB
2. In Xcode, select your device in toolbar
3. Click **Run** (Play button)
4. If prompted, trust developer certificate on device:
   - Settings > General > Device Management

### Desktop Setup

#### RealSense Camera (Windows/Linux)

**Install SDK:**

**Windows:**
```bash
# Download Intel RealSense SDK 2.0
https://github.com/IntelRealSense/librealsense/releases

# Install executable
```

**Ubuntu:**
```bash
# Add repository
sudo apt-key adv --keyserver keyserver.ubuntu.com --recv-key F6E65AC044F831AC80A06380C8B3A55A6F3EFCDE
sudo add-apt-repository "deb https://librealsense.intel.com/Debian/apt-repo $(lsb_release -cs) main"

# Install
sudo apt update
sudo apt install librealsense2-devel librealsense2-utils

# Test camera
realsense-viewer
```

**Integrate with Unity:**
```bash
# Download RealSense SDK for Unity
https://github.com/IntelRealSense/librealsense/tree/master/wrappers/unity

# Copy to Assets/Plugins/
```

#### Azure Kinect (Windows/Linux)

**Install SDK:**

**Windows:**
```bash
# Download Azure Kinect SDK
https://learn.microsoft.com/en-us/azure/kinect-dk/sensor-sdk-download

# Install MSI package
```

**Ubuntu:**
```bash
# Add Microsoft repository
curl -sSL https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
sudo apt-add-repository https://packages.microsoft.com/ubuntu/18.04/prod

# Install
sudo apt update
sudo apt install k4a-tools libk4a1.4-dev

# Test camera
k4aviewer
```

---

## Docker Setup

### 1. Install Docker

#### Windows
```bash
# Download Docker Desktop
https://www.docker.com/products/docker-desktop

# Or use winget
winget install Docker.DockerDesktop
```

#### macOS
```bash
# Download Docker Desktop
https://www.docker.com/products/docker-desktop

# Or use Homebrew
brew install --cask docker
```

#### Ubuntu
```bash
# Install Docker Engine
sudo apt update
sudo apt install apt-transport-https ca-certificates curl software-properties-common
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
sudo apt update
sudo apt install docker-ce docker-ce-cli containerd.io

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Add user to docker group
sudo usermod -aG docker $USER
newgrp docker
```

**Verify Installation:**
```bash
docker --version
docker-compose --version
```

### 2. Configure Docker Environment

```bash
cd holocall/infrastructure/docker

# Copy environment file
cp .env.example .env

# Edit with your configuration
nano .env
```

**Important Variables:**
```env
# Replace with strong passwords
DB_PASSWORD=your_strong_password_here
REDIS_PASSWORD=your_strong_password_here
JWT_SECRET=your_jwt_secret_min_32_chars
TURN_SECRET=your_turn_secret

# Replace with your server's public IP
PUBLIC_IP=your.public.ip.address
```

### 3. Start Services

```bash
# Start all services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f backend
```

### 4. Verify Services

```bash
# Test backend
curl http://localhost:8080/health

# Test PostgreSQL
docker exec -it holocall-postgres psql -U holocall -d holocall -c "SELECT version();"

# Test Redis
docker exec -it holocall-redis redis-cli ping

# Test TURN server (requires turnutils)
turnutils_uclient -v -u test -w test your.public.ip
```

### 5. Stop Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes (WARNING: deletes data)
docker-compose down -v
```

---

## Production Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for comprehensive production deployment guide.

**Quick Checklist:**

- [ ] Obtain domain name
- [ ] Set up SSL certificates (Let's Encrypt)
- [ ] Configure firewall rules
- [ ] Set up monitoring (Prometheus + Grafana)
- [ ] Configure backups (database, Redis)
- [ ] Set up CDN (for static assets)
- [ ] Configure auto-scaling (if using cloud)
- [ ] Set up CI/CD pipeline
- [ ] Configure error tracking (Sentry)
- [ ] Set up log aggregation (ELK stack)

---

## Troubleshooting

### Backend Won't Start

**Error: "Cannot connect to database"**
```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# Check connection
psql -U holocall -h localhost -d holocall

# Check logs
docker logs holocall-postgres
```

**Error: "Port 8080 already in use"**
```bash
# Find process using port
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows

# Kill process or change PORT in .env
```

### Unity Build Fails

**Error: "Unable to list target platforms"**
- Install required Unity modules in Unity Hub

**Error: "Android SDK not found"**
- Set ANDROID_HOME environment variable
- Edit > Preferences > External Tools > Android SDK

**Error: "iOS build requires macOS"**
- iOS builds only work on macOS with Xcode

### WebRTC Connection Fails

**Check STUN/TURN servers:**
```bash
# Test TURN server
https://webrtc.github.io/samples/src/content/peerconnection/trickle-ice/

# Enter your TURN credentials and test
```

**Check firewall rules:**
```bash
# Ensure these ports are open
80, 443 (HTTP/HTTPS)
8080 (Backend)
3478, 5349 (TURN/TURNS)
49152-65535 (TURN relay)
10000-10100 (Mediasoup RTC)
```

### Face Tracking Not Working

**Android (ARCore):**
- Check device is ARCore compatible
- Ensure ARCore app is installed and updated
- Check camera permissions in app settings

**iOS (ARKit):**
- Check device has TrueDepth camera (iPhone X+)
- Ensure iOS version is 13+
- Check camera permissions

---

## Next Steps

After successful setup:

1. **Run Demo**: Follow [DEMO.md](DEMO.md) for testing
2. **Read API Docs**: Understand the API in [API.md](API.md)
3. **Deploy to Production**: See [DEPLOYMENT.md](DEPLOYMENT.md)
4. **Contribute**: Check [CONTRIBUTING.md](../CONTRIBUTING.md)

---

**Need Help?**
- Check [FAQ](FAQ.md)
- Open an issue on GitHub
- Join our Discord community
- Email: support@holocall.com
