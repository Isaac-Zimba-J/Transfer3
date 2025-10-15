# 📁 Cross-Platform File Transfer3

A modern, wireless file transfer application built with .NET MAUI that enables seamless file sharing between Android, iOS, Windows, and macOS devices on the same local network.

![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-9.0-512BD4?style=for-the-badge&logo=.net)
![C#](https://img.shields.io/badge/C%23-11.0-239120?style=for-the-badge&logo=c-sharp)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

## 🎯 Features

- 🔍 **Automatic Device Discovery** - No manual IP configuration needed
- 📤 **Fast File Transfers** - Direct peer-to-peer transfers using TCP
- 📊 **Real-time Progress** - See transfer speed and progress
- 📱 **Cross-Platform** - Works on Android, iOS, Windows, and macOS
- 🔒 **Local Network Only** - Secure transfers within your WiFi
- 📜 **Transfer History** - Track all your file transfers
- 🎨 **Modern UI** - Clean, intuitive Material Design interface

## 📸 Screenshots

_Coming soon - Screenshots of the app in action!_

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                     FILE TRANSFER APP ARCHITECTURE                   │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                         PRESENTATION LAYER                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│  │  MainPage    │  │   Devices    │  │   Active     │              │
│  │   (XAML)     │  │     Tab      │  │  Transfers   │              │
│  └──────────────┘  └──────────────┘  └──────────────┘              │
└─────────────────────────────────────────────────────────────────────┘
                              ▲ Data Binding
                              │
┌─────────────────────────────────────────────────────────────────────┐
│                       APPLICATION LAYER (MVVM)                       │
│  ┌──────────────────────────────────────────────────────┐           │
│  │              MainViewModel (ObservableObject)         │           │
│  │  • Commands: Start/Stop Discovery, Send File         │           │
│  │  • Properties: Devices, Transfers, Status             │           │
│  │  • Events: Progress, Completion, Discovery            │           │
│  └──────────────────────────────────────────────────────┘           │
└─────────────────────────────────────────────────────────────────────┘
                              ▲ Dependency Injection
                              │
┌─────────────────────────────────────────────────────────────────────┐
│                          SERVICE LAYER                               │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐    │
│  │    Discovery    │  │  File Transfer  │  │    Network      │    │
│  │     Service     │  │     Service     │  │    Service      │    │
│  │                 │  │                 │  │                 │    │
│  │  • Broadcast    │  │  • Send File    │  │  • Get IP       │    │
│  │  • Listen       │  │  • Receive      │  │  • Check Conn   │    │
│  │  • Heartbeat    │  │  • Progress     │  │  • Broadcast    │    │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
                              ▲
                              │
┌─────────────────────────────────────────────────────────────────────┐
│                          DOMAIN LAYER                                │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐    │
│  │   DeviceInfo    │  │ FileTransferInfo│  │     Enums       │    │
│  │                 │  │                 │  │                 │    │
│  │  • Name         │  │  • FileName     │  │  • DeviceType   │    │
│  │  • IP Address   │  │  • Progress     │  │  • Status       │    │
│  │  • DeviceType   │  │  • Speed        │  │  • MessageType  │    │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
```

## 🔄 How It Works - Visual Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    DEVICE DISCOVERY & FILE TRANSFER FLOW             │
└─────────────────────────────────────────────────────────────────────┘

PHASE 1: DEVICE DISCOVERY (UDP Port 48888)
═══════════════════════════════════════════════════════════════════════

Device A (MacBook)                           Device B (Android Phone)
192.168.1.100                                192.168.1.105
     │                                             │
     │ [1] App Starts                              │ [1] App Starts
     │ ───────────────────────────────────────────┼──────────────────>
     │     UDP Broadcast: "I'm MacBook"            │
     │     { name, ip, type, port }                │
     │                                             │
     │                                             │ [2] Receives Broadcast
     │                                             │     Adds "MacBook" to list
     │                                             │
     │ <───────────────────────────────────────────┼────────────────
     │     UDP Broadcast: "I'm Android"            │ [3] Broadcasts presence
     │                                             │
     │ [4] Receives Broadcast                      │
     │     Adds "Android" to list                  │
     │                                             │
     │         HEARTBEAT (Every 5 seconds)         │
     │ ◄──────────────────────────────────────────►│
     │           "Still here" broadcasts           │
     │                                             │


PHASE 2: FILE TRANSFER (TCP Port 48889)
═══════════════════════════════════════════════════════════════════════

Device A (Sender)                            Device B (Receiver)
     │                                             │
     │ [1] User selects "photo.jpg"                │
     │     Taps "Send to Android"                  │
     │                                             │
     │ [2] TCP Connect ───────────────────────────►│
     │     (3-way handshake)                       │
     │                                             │
     │ [3] Send Metadata ─────────────────────────►│
     │     {                                       │
     │       "fileName": "photo.jpg",              │
     │       "fileSize": 5242880,                  │
     │       "id": "abc123"                        │
     │     }                                       │
     │                                             │
     │                                             │ [4] Alert User
     │                                             │     "Accept photo.jpg?"
     │                                             │     [Accept] [Reject]
     │                                             │
     │ ◄──────────────────────────────────────────┤ [5] User Accepts
     │     Response: "ACCEPT"                      │
     │                                             │
     │ [6] Transfer File Data                      │
     │ ────────────────────────────────────────────►│
     │     Chunk 1 (8 KB) ────────────────────────►│ [Write to disk]
     │     Chunk 2 (8 KB) ────────────────────────►│ [Write to disk]
     │     Chunk 3 (8 KB) ────────────────────────►│ [Write to disk]
     │     ...                                     │ ...
     │     Chunk N (remaining) ───────────────────►│ [Write to disk]
     │                                             │
     │     Progress: 25% ▓▓▓░░░░░░░░░░░           │ Progress: 25%
     │     Speed: 2.5 MB/s                         │ Speed: 2.5 MB/s
     │                                             │
     │     Progress: 50% ▓▓▓▓▓▓░░░░░░░           │ Progress: 50%
     │     Speed: 2.8 MB/s                         │ Speed: 2.8 MB/s
     │                                             │
     │     Progress: 100% ▓▓▓▓▓▓▓▓▓▓▓▓          │ Progress: 100%
     │                                             │
     │ [7] Transfer Complete                       │ [8] File Saved!
     │     Status: ✓ Completed                     │     Status: ✓ Completed
     │     Move to History                         │     Move to History
     │                                             │
     │ [9] Close Connection ───────────────────────│


NETWORK TOPOLOGY
═══════════════════════════════════════════════════════════════════════

                        ┌─────────────────┐
                        │   WiFi Router   │
                        │  192.168.1.1    │
                        └────────┬────────┘
                                 │
                 ┌───────────────┼───────────────┐
                 │               │               │
        ┌────────▼────────┐ ┌───▼────────┐ ┌───▼────────┐
        │   MacBook       │ │  Android    │ │   iPhone   │
        │ 192.168.1.100   │ │192.168.1.105│ │192.168.1.110│
        │                 │ │             │ │            │
        │ [File Transfer] │ │[Discovery]  │ │[Discovery] │
        │      App        │ │    App      │ │    App     │
        └─────────────────┘ └─────────────┘ └────────────┘
             Sender            Receiver         Observer
                 │                 │                │
                 └─────────┬───────┴────────────────┘
                           │
                    UDP Broadcasts
               (Everyone hears everyone)


DATA FLOW DETAILS
═══════════════════════════════════════════════════════════════════════

UDP Discovery Packet:
┌──────────────────────────────────────────────────────────────┐
│ {                                                             │
│   "Id": "550e8400-e29b-41d4-a716-446655440000",             │
│   "Name": "John's MacBook Pro",                              │
│   "IpAddress": "192.168.1.100",                              │
│   "Port": 48889,                                             │
│   "DeviceType": "macOS",                                     │
│   "IsOnline": true,                                          │
│   "LastSeen": "2024-10-15T10:30:00Z"                        │
│ }                                                             │
└──────────────────────────────────────────────────────────────┘

TCP File Transfer Packet Structure:
┌──────────────────────────────────────────────────────────────┐
│ [4 bytes: Metadata Length] [Metadata JSON] [File Data...]    │
│                                                               │
│ Example:                                                      │
│ [0x00, 0x00, 0x01, 0x2C] → 300 bytes of metadata            │
│ [{...JSON...}] → File transfer info                          │
│ [0xFF, 0xD8, 0xFF, 0xE0...] → Actual file bytes             │
└──────────────────────────────────────────────────────────────┘
```

## 📋 Prerequisites

- **.NET 9 SDK** or later
- **Visual Studio Code** with C# Dev Kit extension
- **MAUI Workload**: Run `dotnet workload install maui`
- **Dotnet Meteor Extension** by Nikita Romanov - [Install from VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=JaneySprings.dotnet-meteor)

### Platform-Specific Requirements

- **Android**: Android SDK (API 21+), Android device or emulator
- **iOS/macOS**: Xcode 15+, macOS Ventura or later (Mac only)
- **Windows**: Windows 10 (1809+) or Windows 11

## 🚀 Getting Started

### 1. Install Dotnet Meteor Extension

1. Open **VS Code**
2. Go to Extensions (`Ctrl+Shift+X` or `Cmd+Shift+X`)
3. Search for **"Dotnet Meteor"** by Nikita Romanov
4. Click **Install**

![Dotnet Meteor](https://img.shields.io/badge/VS%20Code-Dotnet%20Meteor-007ACC?style=flat-square&logo=visual-studio-code)

### 2. Clone the Repository

```bash
git clone https://github.com/Isaac-Zimba-J/Transfer3.git
cd Transfer3
```

### 3. Open in VS Code

```bash
code .
```

### 4. Install Dependencies

Open VS Code Terminal (` Ctrl+`` or  `Cmd+``) and run:

```bash
dotnet restore
```

### 5. Install NuGet Packages

```bash
dotnet add package CommunityToolkit.Mvvm --version 8.2.2
```

### 6. Configure Permissions

#### Android (Platforms/Android/AndroidManifest.xml)

Already configured with:

- Internet access
- Network state access
- Storage permissions

#### iOS/macOS (Platforms/MacCatalyst/Entitlements.plist)

Already configured with:

- Network client/server capabilities
- File access permissions

### 7. Build and Run with Dotnet Meteor

#### Method 1: Using Command Palette

1. Press `F1` or `Ctrl+Shift+P` (Windows/Linux) / `Cmd+Shift+P` (Mac)
2. Type **"Meteor: Run Project"**
3. Select your target platform (Android, iOS, Windows, macOS)
4. Select your device/emulator
5. Press Enter to build and run

#### Method 2: Using Status Bar

1. Look at the bottom status bar in VS Code
2. Click on the **platform selector** (shows current platform)
3. Choose your target platform
4. Click the **device selector** to choose device/emulator
5. Click the **▶ Run** button in the status bar

#### Method 3: Using Debug Panel

1. Open the **Run and Debug** panel (`Ctrl+Shift+D` or `Cmd+Shift+D`)
2. Select configuration from dropdown (e.g., "Android", "iOS", "Windows")
3. Click **▶ Start Debugging** or press `F5`

### 8. Platform-Specific Instructions

#### 📱 Android

1. Enable **Developer Options** on your Android device:
   - Go to Settings → About Phone
   - Tap "Build Number" 7 times
2. Enable **USB Debugging**:
   - Settings → System → Developer Options → USB Debugging
3. Connect device via USB or use Android emulator
4. In VS Code, Dotnet Meteor will automatically detect your device
5. Select device and click Run

#### 🍎 iOS/macOS (Mac only)

1. Ensure Xcode is installed with command line tools:
   ```bash
   xcode-select --install
   ```
2. For iOS device: Connect iPhone/iPad via USB
3. Trust the computer on your iOS device when prompted
4. In VS Code, select iOS/macOS from platform selector
5. Select your device/simulator and click Run

#### 🪟 Windows

1. Enable Developer Mode:
   - Settings → Update & Security → For Developers
   - Toggle "Developer Mode" ON
2. In VS Code, select Windows from platform selector
3. Click Run (no device selection needed)

## 📱 How to Use the App

### Step 1: Launch on Multiple Devices

1. Build and deploy the app to 2+ devices using Dotnet Meteor
2. Ensure all devices are connected to the **same WiFi network**
3. Launch the app on each device

### Step 2: Discover Devices

- The app **automatically starts discovery** when launched
- Wait 2-5 seconds for devices to appear in the **Devices** tab
- You'll see device names, IP addresses, and device types (📱 Android, 💻 Mac, etc.)
- If devices don't appear, tap the **Refresh** button

### Step 3: Send a File

1. Go to the **Devices** tab
2. **Tap on a device** to select it (it will be highlighted)
3. Tap **"Send File to Selected Device"** button at the bottom
4. Choose a file from your device using the file picker
5. Wait for the receiving device to respond

### Step 4: Accept Incoming File (Receiver)

1. A popup will appear: **"Receive [filename] from [device name]?"**
2. Tap **Accept** to receive the file
3. Choose a folder to save the file (or use default Downloads)
4. Transfer starts automatically

### Step 5: Monitor Progress

Watch real-time transfer in the **Active** tab:

- **Progress bar** with percentage (0-100%)
- **Transfer speed** (KB/s, MB/s)
- **File name** and size
- **Cancel button** to stop transfer if needed

### Step 6: View History

Check completed transfers in the **History** tab:

- ✅ Completed transfers (green)
- ❌ Failed transfers (red)
- 🚫 Cancelled transfers (orange)
- Timestamp and device info

## 🎯 Quick Start Video Workflow

```
1. Open VS Code → Open Project Folder
2. Press F5 (Start Debugging)
3. Select Platform (Android/iOS/Windows/macOS)
4. Select Device/Emulator
5. App builds and launches automatically
6. Repeat on second device
7. Devices discover each other
8. Select device → Send File → Accept on receiver
9. Watch transfer progress
10. Done! ✅
```

## 🏗️ Project Structure

```
Transfer3/
├── Domain/
│   ├── Entities/
│   │   ├── DeviceInfo.cs          # Device representation
│   │   └── FileTransferInfo.cs    # Transfer metadata
│   └── Enums/
│       └── Enums.cs                # DeviceType, TransferStatus, etc.
│
├── Services/
│   ├── Contracts/                  # Service interfaces
│   │   ├── IDeviceDiscoveryService.cs
│   │   ├── IFileTransferService.cs
│   │   ├── INetworkService.cs
│   │   ├── IFilePickerService.cs
│   │   └── IDeviceInfoService.cs
│   │
│   └── Implementations/            # Concrete implementations
│       ├── DeviceDiscoveryService.cs
│       ├── FileTransferService.cs
│       ├── NetworkService.cs
│       ├── FilePickerService.cs
│       └── DeviceInfoService.cs
│
├── ViewModels/
│   └── MainViewModel.cs            # MVVM ViewModel with commands
│
├── Views/
│   ├── MainPage.xaml               # Main UI
│   └── MainPage.xaml.cs            # Code-behind
│
├── Helpers/
│   ├── Converters.cs               # Value converters for XAML
│   └── PermissionHelper.cs         # Permission management
│
├── Platforms/                      # Platform-specific code
│   ├── Android/
│   │   └── AndroidManifest.xml
│   ├── iOS/
│   ├── MacCatalyst/
│   │   └── Entitlements.plist
│   └── Windows/
│
├── App.xaml                        # App resources
├── App.xaml.cs                     # App startup
└── MauiProgram.cs                  # DI configuration
```

## 🔧 Configuration

### Ports

- **Discovery Port (UDP)**: 48888
- **Transfer Port (TCP)**: 48889

Change in service implementations if these ports are blocked:

```csharp
// DeviceDiscoveryService.cs
private const int DiscoveryPort = 48888;

// FileTransferService.cs
private const int TransferPort = 48889;
```

### Transfer Settings

```csharp
// FileTransferService.cs
private const int BufferSize = 8192;  // 8KB chunks (increase for faster transfer)

// DeviceDiscoveryService.cs
private const int HeartbeatIntervalSeconds = 5;     // Discovery frequency
private const int DeviceTimeoutSeconds = 15;        // Device offline timeout
```

## 🧪 Testing

### Unit Testing Services

```bash
dotnet test
```

### Manual Testing Checklist

- [ ] App launches on all target platforms
- [ ] Devices discover each other within 5 seconds
- [ ] File picker opens correctly
- [ ] Transfer shows progress bar
- [ ] Transfer completes successfully
- [ ] History shows completed transfers
- [ ] App handles network disconnect gracefully

## 🐛 Troubleshooting

### Devices Not Discovering

**Problem**: Devices don't appear in the list

**Solutions**:

1. Ensure both devices on same WiFi (not cellular/different networks)
2. Check firewall settings:
   - **Windows**: Allow app through Windows Firewall
   - **macOS**: System Preferences → Security → Firewall → Allow app
3. Verify UDP port 48888 not blocked by router
4. Try manual refresh button
5. Restart app on both devices

### Transfer Fails

**Problem**: Transfer starts but fails midway

**Solutions**:

1. Check available storage on receiving device
2. Verify file permissions (both read and write)
3. Ensure stable WiFi connection
4. Try smaller file first
5. Check TCP port 48889 not blocked

### Permission Denied

**Problem**: App crashes or can't access files

**Solutions**:

1. **Android**: Go to Settings → Apps → File Transfer → Permissions → Enable Storage
2. **iOS**: Settings → Privacy → Files and Folders → Enable
3. Uninstall and reinstall app to trigger permission prompts

### Slow Transfer Speed

**Problem**: Transfer slower than expected

**Solutions**:

1. Move closer to WiFi router
2. Close other bandwidth-heavy apps
3. Increase buffer size in `FileTransferService.cs`:
   ```csharp
   private const int BufferSize = 65536; // 64KB instead of 8KB
   ```

## 🔐 Security Considerations

⚠️ **Current Implementation**:

- No encryption (plain text transfer)
- No authentication
- Local network only

**For Production Use**, implement:

- TLS/SSL encryption
- Device pairing with QR codes
- User authentication
- File integrity verification (checksums)
- Rate limiting

## 📈 Performance

### Benchmarks

- **Discovery Time**: 1-5 seconds
- **Transfer Speed**: 10-50 MB/s (WiFi dependent)
- **Memory Usage**: ~50-100 MB
- **Battery Impact**: Low (discovery), Medium (active transfer)

### Optimization Tips

1. **Faster Discovery**: Reduce heartbeat interval to 2 seconds
2. **Faster Transfer**: Increase buffer size to 64KB
3. **Better Battery**: Increase heartbeat to 10 seconds, pause discovery when backgrounded

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Isaac Zimba**

- GitHub: [@Isaac-Zimba-J](https://github.com/Isaac-Zimba-J)
- LinkedIn: [Isaac Zimba](https://www.linkedin.com/in/isaac-zimba-842061239/)
- Email: zimbaisaacj2002@gmail.com

## 🙏 Acknowledgments

This project was made possible thanks to these amazing tools and libraries:

### Core Technologies

- **[.NET MAUI Team](https://github.com/dotnet/maui)** - Microsoft's cross-platform UI framework
- **[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)** - MVVM helpers and source generators

### Development Tools

- **[Dotnet Meteor](https://marketplace.visualstudio.com/items?itemName=JaneySprings.dotnet-meteor)** by **Nikita Romanov** - Essential VS Code extension that makes MAUI development seamless with device management, debugging, and deployment features
- **[Visual Studio Code](https://code.visualstudio.com/)** - Lightweight, powerful code editor
- **[C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)** - Microsoft's C# development tools

### .NET Libraries Used

- **System.Net.Sockets** - TCP/UDP networking
- **System.Net.NetworkInformation** - Network interface management
- **System.Text.Json** - JSON serialization
- **Microsoft.Extensions.DependencyInjection** - Dependency injection framework
- **Microsoft.Extensions.Logging** - Logging infrastructure

### Special Thanks

- **Nikita Romanov** ([@JaneySprings](https://github.com/JaneySprings)) - For creating and maintaining Dotnet Meteor, which dramatically improves the .NET MAUI development experience in VS Code
- All contributors and testers who helped improve this project
- The .NET community for continuous support and feedback

## 📞 Support

- 📧 Email: zimbaisaacj2002@gmail.com
- 🐛 Issues: [GitHub Issues](https://github.com/Isaac-Zimba-J/Transfer3/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/Isaac-Zimba-J/Transfer3/discussions)

---

**⭐ Star this repo if you find it helpful!**

**🔗 Share with friends who need wireless file transfer!**

**🚀 Built with VS Code + Dotnet Meteor**
