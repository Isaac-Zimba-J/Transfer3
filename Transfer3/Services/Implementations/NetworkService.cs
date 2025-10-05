using System;
using System.Net.NetworkInformation;
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

    public Task<string> GetBroadcastAddressAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetLocalIpAddressAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetSubnetMaskAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsConnectedAsync()
    {
        throw new NotImplementedException();
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
}
