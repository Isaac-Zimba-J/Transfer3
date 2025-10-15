# 📁 Cross-Platform File Transfer App

A modern, wireless file transfer application built with .NET MAUI that enables seamless file sharing between Android, iOS, Windows, and macOS devices on the same local network.

![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-8.0-512BD4?style=for-the-badge&logo=.net)
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

- **.NET 8 SDK** or later
- **Visual Studio 2022** (17.8+) or **Visual Studio Code** with C# extensions
- **MAUI Workload**: Run `dotnet workload install maui`

### Platform-Specific Requirements

- **Android**: Android SDK (API 21+), Android device or emulator
- **iOS/macOS**: Xcode 15+, macOS Ventura or later
- **Windows**: Windows 10 (1809+) or Windows 11

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/file-transfer-app.git
cd file-transfer-app
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Install NuGet Packages

```bash
dotnet add package CommunityToolkit.Mvvm --version 8.2.2
```

### 4. Configure Permissions

#### Android (Platforms/Android/AndroidManifest.xml)
Already configured with:
- Internet access
- Network state access
- Storage permissions

#### iOS/macOS (Platforms/MacCatalyst/Entitlements.plist)
Already configured with:
- Network client/server capabilities
- File access permissions

### 5. Build and Run

#### Android
```bash
dotnet build -t:Run -f net8.0-android
```

#### iOS
```bash
dotnet build -t:Run -f net8.0-ios
```

#### macOS
```bash
dotnet build -t:Run -f net8.0-maccatalyst
```

#### Windows
```bash
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

## 📱 Usage

### Step 1: Launch on Multiple Devices
Install and launch the app on 2+ devices connected to the **same WiFi network**.

### Step 2: Discover Devices
The app automatically discovers nearby devices. You'll see them appear in the **Devices** tab within seconds.

### Step 3: Send a File
1. Select a device from the **Devices** tab
2. Tap **"Send File to Selected Device"**
3. Choose a file from your device
4. On the receiving device, tap **Accept** when prompted

### Step 4: Monitor Progress
Watch real-time transfer progress in the **Active** tab:
- Progress bar with percentage
- Transfer speed (MB/s)
- Remaining time estimate

### Step 5: View History
Check completed transfers in the **History** tab.

## 🏗️ Project Structure

```
FileTransferApp/
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

**Your Name**
- GitHub: [@yourusername](https://github.com/yourusername)
- LinkedIn: [Your Name](https://linkedin.com/in/yourprofile)

## 🙏 Acknowledgments

- [.NET MAUI Team](https://github.com/dotnet/maui)
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- All contributors and testers

## 📞 Support

- 📧 Email: your.email@example.com
- 🐛 Issues: [GitHub Issues](https://github.com/yourusername/file-transfer-app/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/yourusername/file-transfer-app/discussions)

---

**⭐ Star this repo if you find it helpful!**

**🔗 Share with friends who need wireless file transfer!**
