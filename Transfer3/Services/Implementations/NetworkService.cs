using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Transfer3.Services.Contracts;

namespace Transfer3.Services.Implementations;

public class NetworkService : INetworkService
{
    public event EventHandler<bool>? ConnectivityChanged;


    public NetworkService()
    {
        // monitor network changes
        NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
    }

    public async Task<string> GetBroadcastAddressAsync()
    {
        var localIp = await GetLocalIpAddressAsync();
        var subnetMask = await GetSubnetMaskAsync();

        var ipBytes = IPAddress.Parse(localIp).GetAddressBytes();
        var maskBytes = IPAddress.Parse(subnetMask).GetAddressBytes();

        // Calculate broadcast address: IP | (~SubnetMask)
        var broadcastBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
        }

        return new IPAddress(broadcastBytes).ToString();

    }

    public async Task<string> GetLocalIpAddressAsync()
    {
        try
        {
            // get the local ip address by connecting to a public dns server
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            await socket.ConnectAsync("8.8.8.8", 65530);

            var endPoint = socket.LocalEndPoint as System.Net.IPEndPoint;
            return endPoint?.Address.ToString() ?? "127.0.0.1";
        }
        catch (System.Exception)
        {
            return GetIpFromNetworkInterfaces();
        }
    }

    public async Task<string> GetSubnetMaskAsync()
    {
        var localIp = await GetLocalIpAddressAsync();

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            var ipProps = ni.GetIPProperties();
            foreach (var addr in ipProps.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                    addr.Address.ToString() == localIp)
                {
                    return addr.IPv4Mask.ToString();
                }
            }
        }

        return "255.255.255.0"; // Default subnet mask
    }

    public async Task<bool> IsConnectedAsync()
    {
        await Task.CompletedTask;

        // Check if any network interface is up and operational
        return NetworkInterface.GetAllNetworkInterfaces()
            .Any(ni => ni.OperationalStatus == OperationalStatus.Up &&
                      ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                      ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel);
    }

    private async void OnNetworkAddressChanged(object? sender, EventArgs e)
    {
        // Handle network address changes if needed
        var isConnected = await IsConnectedAsync();
        ConnectivityChanged?.Invoke(this, isConnected);

    }

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        ConnectivityChanged?.Invoke(this, e.IsAvailable);
    }

    private string GetIpFromNetworkInterfaces()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up ||
                ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var ipProps = ni.GetIPProperties();
            foreach (var addr in ipProps.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return addr.Address.ToString();
                }
            }
        }
        return "127.0.0.1";
    }
}
