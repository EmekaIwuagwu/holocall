# 🎬 HoloCall Demo Script

This guide provides step-by-step instructions for testing HoloCall across different platforms.

## Prerequisites

- Backend services running (see [Installation Guide](../README.md#installation))
- Unity Editor installed (for testing)
- At least 2 devices for multi-user testing
- Good network connection (WiFi recommended)

## Demo Scenarios

### Scenario 1: Desktop ↔ Desktop Volumetric Call

**Required Hardware:**
- 2 Desktop computers
- Intel RealSense D435 or Azure Kinect (at least one)
- Microphones and speakers/headphones

**Steps:**

1. **Start Backend Services**
   ```bash
   cd infrastructure/docker
   docker-compose up -d

   # Verify services are running
   docker-compose ps
   curl http://localhost:8080/health
   ```

2. **Open Unity Project (Computer 1)**
   ```bash
   # Open Unity Hub
   # Add project: holocall/unity
   # Open with Unity 2022.3 LTS
   ```

3. **Configure HoloCall Manager**
   - Open Scene: `Assets/Scenes/DesktopScene.unity`
   - Select `HoloCallManager` GameObject
   - Set `Signaling Server URL`: `ws://localhost:8080/ws` (or your server IP)
   - Set `Platform`: `Desktop`

4. **Authenticate User 1**
   - Click Play in Unity
   - Enter email: `alice@test.com`
   - Enter display name: `Alice`
   - Click "Authenticate"

5. **Create Room**
   - Click "Create Room"
   - Note the Room ID (e.g., `ABC12345`)

6. **Setup Computer 2**
   - Repeat steps 2-4 with different credentials (e.g., `bob@test.com`, `Bob`)

7. **Join Room from Computer 2**
   - Enter Room ID from step 5
   - Click "Join Room"

8. **Test Volumetric Capture**
   - Both users should see each other as point clouds
   - Move around and verify real-time updates
   - Test audio by speaking

**Expected Results:**
- Latency: 150-200ms for volumetric data
- Frame rate: 30 FPS capture, 60 FPS rendering
- Point cloud: 10K-30K points
- Audio: Clear with <100ms latency

**Troubleshooting:**
- No depth camera detected? Check USB connection and drivers
- Point cloud not visible? Check firewall rules for WebRTC ports
- High latency? Reduce point cloud density in settings

---

### Scenario 2: Mobile AR ↔ Mobile AR Avatar Call

**Required Hardware:**
- 2 ARCore/ARKit compatible phones
- Good lighting for face tracking

**Steps:**

1. **Build Android APK**
   ```bash
   # In Unity
   # File > Build Settings
   # Platform: Android
   # Switch Platform
   # Player Settings:
   #   - Company Name: YourCompany
   #   - Product Name: HoloCall
   #   - Package Name: com.yourcompany.holocall
   #   - Minimum API Level: 24 (Android 7.0)
   # Build and Run
   ```

2. **Install on Both Phones**
   ```bash
   # Via ADB
   adb install holocall.apk

   # Or build directly to connected device
   ```

3. **Launch App on Phone 1**
   - Open HoloCall app
   - Allow camera and microphone permissions
   - Authenticate with `alice@test.com`

4. **Create Room**
   - Tap "Create Room"
   - Note Room ID displayed

5. **Launch App on Phone 2**
   - Authenticate with `bob@test.com`
   - Enter Room ID from Phone 1
   - Tap "Join"

6. **Place Holograms in AR**
   - Point camera at a flat surface (table/floor)
   - Tap to place Alice's avatar on Phone 2
   - Tap to place Bob's avatar on Phone 1

7. **Test Face Tracking**
   - Make facial expressions
   - Verify avatar mimics your face in real-time
   - Test lip sync by speaking

**Expected Results:**
- Face tracking: 52 blendshapes at 30 FPS
- Avatar rendering: 60 FPS on high-end, 30 FPS on mid-tier
- Placement accuracy: ±5cm
- Battery consumption: 45-60 minutes per session

**Troubleshooting:**
- Plane detection not working? Ensure good lighting and textured surfaces
- Face tracking jittery? Check front camera cleanliness
- Avatar not syncing? Verify WebSocket connection in logs

---

### Scenario 3: Desktop ↔ Mobile Cross-Platform Call

**Required Hardware:**
- 1 Desktop with depth camera
- 1 Mobile phone (Android/iOS)

**Steps:**

1. **Start Desktop Client**
   - Follow Scenario 1 steps 1-5
   - Create room as Desktop user

2. **Start Mobile Client**
   - Follow Scenario 2 steps 2-5
   - Join room created by Desktop user

3. **Observe Cross-Platform Communication**
   - **On Desktop**: See mobile user as animated avatar
   - **On Mobile**: See desktop user as volumetric point cloud hologram
   - Test bidirectional audio

4. **Test AR Placement**
   - On mobile, place desktop user's hologram on table
   - Scale hologram with pinch gesture
   - Rotate with two-finger rotation

**Expected Results:**
- Desktop sends volumetric (point cloud)
- Mobile sends avatar (blendshapes + head pose)
- Both can hear each other clearly
- Spatial audio works on mobile (avatar positioned in AR space)

---

### Scenario 4: Group Call (4 Participants)

**Required Hardware:**
- 2 Desktops + 2 Mobiles (or any combination)

**Steps:**

1. **User 1 (Desktop)**: Create room

2. **Users 2-4**: Join room with Room ID

3. **Observe Multi-User Layout**
   - Desktop: Grid layout with 3 other participants
   - Mobile: Carousel with dots to switch between holograms

4. **Test Scenarios**:
   - All participants talk simultaneously (test audio mixing)
   - Move holograms on mobile to different positions
   - Desktop user shares screen (if implemented)
   - One user leaves and rejoins

**Expected Results:**
- All 4 users visible and audible
- Frame rate: 30-60 FPS depending on device
- Bandwidth: 2-4 Mbps per user
- CPU usage: <50% on mid-tier devices

---

## Performance Testing

### Latency Measurement

**Audio Latency:**
```bash
# User 1: Clap hands
# User 2: Measure time to hear clap
# Expected: 70-100ms
```

**Video/Hologram Latency:**
```bash
# User 1: Wave hand
# User 2: Measure time to see wave
# Expected: 150-250ms
```

### Network Conditions Testing

**Simulate Poor Network:**
```bash
# On Linux
sudo tc qdisc add dev eth0 root netem delay 100ms loss 5%

# Test call quality
# Observe adaptive bitrate and quality degradation

# Remove simulation
sudo tc qdisc del dev eth0 root
```

**Expected Behavior:**
- Automatic quality reduction under poor conditions
- No call drops unless >10% packet loss
- Graceful recovery when network improves

### Battery Testing (Mobile)

```bash
# Before starting call
adb shell dumpsys battery

# Run call for 30 minutes
# After call
adb shell dumpsys battery

# Calculate battery drain
# Expected: <2% per minute (45-60 min total)
```

---

## Debugging Tools

### Backend Logs

```bash
# View signaling server logs
docker-compose logs -f backend

# View TURN server logs
docker-compose logs -f coturn

# View all logs
docker-compose logs -f
```

### Unity Logs

```bash
# Desktop (Windows)
C:\Users\YourUser\AppData\LocalLow\CompanyName\HoloCall\Player.log

# macOS
~/Library/Logs/CompanyName/HoloCall/Player.log

# Android
adb logcat -s Unity
```

### Network Debugging

```bash
# Check WebRTC stats in Unity console
# Look for:
# - Packets sent/received
# - Bytes sent/received
# - RTT (round-trip time)
# - Packet loss
# - Jitter

# Chrome WebRTC internals (for web testing)
chrome://webrtc-internals
```

---

## Demo Checklist

### Pre-Demo Setup (30 minutes before)

- [ ] Backend services running and healthy
- [ ] Test room creation and joining
- [ ] Audio/video working on all devices
- [ ] Network speed test (min 5 Mbps upload/download)
- [ ] Devices charged (>80% battery)
- [ ] Backup devices ready
- [ ] Screen recording setup (if presenting)

### During Demo

- [ ] Start with simple 1:1 call
- [ ] Demonstrate cross-platform compatibility
- [ ] Show AR placement features
- [ ] Test face tracking/expressions
- [ ] Demonstrate spatial audio
- [ ] Show group call (if time permits)
- [ ] Handle Q&A

### Post-Demo

- [ ] Collect feedback
- [ ] Review logs for errors
- [ ] Document any issues
- [ ] Update FAQ based on questions

---

## Common Issues and Solutions

### Issue: "Cannot connect to signaling server"

**Solutions:**
1. Check backend is running: `curl http://localhost:8080/health`
2. Verify WebSocket URL in Unity is correct
3. Check firewall rules allow port 8080
4. Ensure JWT token is valid (check expiry)

### Issue: "Room not found"

**Solutions:**
1. Verify Room ID is correct (case-sensitive)
2. Check if room expired (rooms expire after 24h empty)
3. Ensure backend didn't restart (clears in-memory rooms)

### Issue: "ICE connection failed"

**Solutions:**
1. Check TURN server is running
2. Verify TURN credentials are correct
3. Test TURN server: https://webrtc.github.io/samples/src/content/peerconnection/trickle-ice/
4. Ensure ports 3478, 5349, 49152-65535 are open

### Issue: "Face tracking not working"

**Solutions:**
1. Ensure front camera is being used
2. Check good lighting on face
3. Verify ARCore/ARKit is enabled in build settings
4. Test with AR Foundation face tracking sample

### Issue: "High latency / lag"

**Solutions:**
1. Check network speed: min 5 Mbps
2. Reduce point cloud density on desktop
3. Lower video resolution
4. Enable adaptive bitrate
5. Check CPU usage (<80%)

---

## Video Demo Script

**Duration: 5 minutes**

**Script:**

```
[0:00-0:30] Introduction
"Welcome to HoloCall, the cross-platform holographic telepresence system."

[0:30-1:30] Desktop Demo
"Here I'm using the desktop client with a RealSense depth camera.
You can see me as a real-time 3D point cloud with RGB colors."

[1:30-2:30] Mobile AR Demo
"Now on my phone, I can join the same call. Using AR, I place the hologram
on my desk. Notice how the avatar tracks my facial expressions in real-time."

[2:30-3:30] Cross-Platform
"The magic happens when desktop and mobile connect. The desktop user sees
my avatar, while I see their volumetric hologram. Both with spatial audio."

[3:30-4:30] Group Call
"We can have up to 10 people in a room. Each platform adapts automatically -
desktop sends point clouds, mobile sends avatars, VR sends full-body tracking."

[4:30-5:00] Closing
"HoloCall - bringing the future of communication to every device.
Try it today at holocall.com"
```

---

## Live Demo Tips

### Setup

1. **Arrive early**: Test everything 30 minutes before
2. **Backup plan**: Have pre-recorded video ready
3. **Internet**: Use wired connection or 5G hotspot
4. **Lighting**: Ensure good lighting for mobile face tracking
5. **Audio**: Test microphones and speakers

### During Presentation

1. **Start simple**: Begin with 1:1 call
2. **Narrate**: Explain what you're doing
3. **Show stats**: Display latency/FPS metrics
4. **Handle errors gracefully**: Have troubleshooting steps memorized
5. **Engage audience**: Take volunteers to try it

### What to Emphasize

- **Cross-platform**: Works on ANY device
- **Real-time**: Low latency performance
- **Production-ready**: Docker deployment, scalable
- **Open source**: MIT license, extensible
- **Future-proof**: Roadmap for AI features

---

## Success Metrics

After demo, evaluate:

- [ ] Call established in <10 seconds
- [ ] Audio latency <100ms
- [ ] Video/hologram latency <250ms
- [ ] No disconnections during demo
- [ ] Face tracking smooth and responsive
- [ ] AR placement accurate
- [ ] Audience engagement high
- [ ] Technical questions answered satisfactorily

---

## Next Steps

After successful demo:

1. **Share recording**: Upload to YouTube/social media
2. **Collect feedback**: Create survey
3. **Document issues**: Log any bugs found
4. **Update roadmap**: Prioritize features based on feedback
5. **Plan next demo**: Schedule follow-up with improvements

---

For more information, see:
- [Setup Guide](../README.md#installation)
- [Architecture Documentation](ARCHITECTURE.md)
- [API Reference](API.md)
- [Troubleshooting](TROUBLESHOOTING.md)
