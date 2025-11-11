# ⚡ HoloCall Quick Start (5 Minutes)

Get HoloCall running in 5 minutes for development.

## Prerequisites

- Node.js 18+
- Docker & Docker Compose
- Unity 2022.3 LTS (optional for testing client)

## Backend (2 minutes)

```bash
# 1. Clone repository
git clone https://github.com/yourusername/holocall.git
cd holocall

# 2. Start backend with Docker
cd infrastructure/docker
cp .env.example .env
docker-compose up -d

# 3. Verify it's running
curl http://localhost:8080/health
# Should return: {"status":"ok"}
```

**That's it!** Backend is running at `http://localhost:8080`

## Unity Client (3 minutes)

```bash
# 1. Open Unity Hub
# 2. Add Project: holocall/unity
# 3. Open with Unity 2022.3 LTS
# 4. Open Scene: Assets/Scenes/MainScene.unity
# 5. Click Play
```

## Test It

**Option 1: Two Unity Instances**
1. Build executable: File > Build And Run
2. Run both Unity Editor and built executable
3. User 1: Create Room
4. User 2: Join with Room ID

**Option 2: Unity + Mobile**
1. Build APK: File > Build Settings > Android > Build
2. Install on phone: `adb install holocall.apk`
3. Unity: Create Room
4. Mobile: Join Room

## Architecture Overview

```
┌─────────────────────────────────────────┐
│  Unity Clients (Desktop/Mobile/VR)     │
│  Cross-Platform WebRTC + AR Foundation  │
└──────────────┬──────────────────────────┘
               │ WebSocket + WebRTC
┌──────────────▼──────────────────────────┐
│  Backend (Node.js + TypeScript)         │
│  - Signaling Server (port 8080)         │
│  - TURN Server (port 3478, 5349)        │
│  - PostgreSQL (port 5432)               │
│  - Redis (port 6379)                    │
└─────────────────────────────────────────┘
```

## Platform Capabilities

| Platform | Capture Mode | Max Users | FPS Target |
|----------|--------------|-----------|------------|
| Desktop  | Volumetric   | 10        | 60         |
| Android  | Avatar       | 4         | 30-60      |
| iOS      | Avatar       | 6         | 60         |
| VR       | Avatar+Hands | 8         | 90-120     |

## Key Features Demo

1. **Desktop Volumetric**: Use RealSense/Kinect for 3D capture
2. **Mobile AR**: Place holograms with ARCore/ARKit
3. **Face Tracking**: 52 blendshapes real-time animation
4. **Spatial Audio**: 3D positional audio in AR
5. **Cross-Platform**: Desktop ↔ Mobile ↔ VR seamlessly

## Project Structure

```
holocall/
├── backend/           # Node.js signaling server
│   ├── src/
│   │   ├── services/  # Core business logic
│   │   ├── middleware/ # Auth, validation
│   │   └── index.ts   # Main entry
│   └── package.json
├── unity/             # Unity client (all platforms)
│   └── Assets/
│       └── Scripts/
│           ├── Core/       # Managers
│           ├── Networking/ # WebRTC
│           ├── Capture/    # Depth cameras
│           ├── AR/         # AR Foundation
│           └── Avatar/     # Face tracking
├── infrastructure/    # DevOps
│   └── docker/       # Docker Compose setup
└── docs/             # Documentation
```

## Configuration

### Backend Environment

Edit `infrastructure/docker/.env`:
```env
PUBLIC_IP=your.public.ip       # Your server's public IP
JWT_SECRET=your_secret_here    # Generate with: openssl rand -base64 32
TURN_SECRET=your_turn_secret   # For TURN authentication
```

### Unity Settings

Select `HoloCallManager` in Unity:
- Signaling Server URL: `ws://localhost:8080/ws`
- Platform: Auto-detect or force specific platform

## Common Commands

### Backend
```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f backend

# Stop services
docker-compose down

# Restart single service
docker-compose restart backend
```

### Unity
```bash
# Build Android APK
File > Build Settings > Android > Build

# Build iOS (macOS only)
File > Build Settings > iOS > Build

# Build Desktop
File > Build Settings > Standalone > Build
```

## Troubleshooting

### Backend not starting?
```bash
# Check if ports are in use
lsof -i :8080
lsof -i :5432
lsof -i :6379

# Check Docker logs
docker-compose logs backend
```

### Can't connect from Unity?
- Check signaling URL is correct
- Verify backend health: `curl http://localhost:8080/health`
- Check firewall allows port 8080

### WebRTC connection fails?
- Ensure TURN server is running: `docker ps | grep coturn`
- Test TURN: https://webrtc.github.io/samples/src/content/peerconnection/trickle-ice/

## Performance Targets

- Audio latency: **<100ms**
- Video latency: **<200ms**
- Avatar latency: **<250ms**
- Frame rate: **30-60 FPS** (mobile), **60 FPS** (desktop), **90-120 FPS** (VR)
- Bandwidth: **2-5 Mbps** per participant

## Next Steps

1. **Full Setup**: See [docs/SETUP.md](docs/SETUP.md)
2. **Run Demo**: Follow [docs/DEMO.md](docs/DEMO.md)
3. **Deploy**: Read [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md)
4. **Architecture**: Study [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## Support

- **Docs**: [docs/](docs/)
- **Issues**: [GitHub Issues](https://github.com/yourusername/holocall/issues)
- **Email**: support@holocall.com

---

**Ready to build the future of communication? Let's go! 🚀**
