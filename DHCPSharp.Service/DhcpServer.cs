using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DHCPSharp.Common;
using DHCPSharp.Common.Enums;
using DHCPSharp.Common.Extensions;
using DHCPSharp.Common.Loggers;
using DHCPSharp.Common.Serialization;
using DHCPSharp.DAL;
using DHCPSharp.DAL.Models;
using DHCPSharp.DAL.Repository;
using DHCPSharp.Properties;
using SQLite;

namespace DHCPSharp
{
    public class DhcpServer
    {
        // Constants
        private const int DHCP_PORT = 67;
        private const int DHCP_CLIENT_PORT = 68;

        private readonly ILogger Log;
        private UdpClient _listener;
        private CancellationTokenSource _cancellation;
        private readonly SemaphoreSlim _requestLock;
        
        public NetworkInterface DhcpInterface { get; private set; }
        public IPAddress DhcpInterfaceAddress { get; private set; }
        public IDhcpConfiguration Configuration { get; }
        public ILeaseManager LeaseManager { get; }

        public DhcpServer(ILogger logger, ILeaseManager leaseManager, IDhcpConfiguration configuration)
        {
            Log = logger;
            Configuration = configuration;
            LeaseManager = leaseManager;
                      
            _requestLock = new SemaphoreSlim(1);
        }

        public void Start()
        {
            Log.Info("DHCP Server is starting up...");
     
            DhcpInterface = GetNetworkInterface();
            DhcpInterfaceAddress = GetInterfaceAddress(DhcpInterface);
            StartListening(DhcpInterfaceAddress, DHCP_PORT);

            Log.Info($"DHCP Server is now listening for requests at '{DhcpInterfaceAddress}' on port '{DHCP_PORT}'");
        }
        public void Stop()
        {
            _cancellation.Cancel();
            _listener.Close();
            _listener = null;
        }
        private NetworkInterface GetNetworkInterface()
        {
            var result = NetworkInterface
                .GetAllNetworkInterfaces()
                .FirstOrDefault(x =>
                    x.NetworkInterfaceType.IsUsableNetworkInterface(x.OperationalStatus)
                    && x.GetIPProperties().UnicastAddresses.Any(y => y.Address.Equals(Configuration.BindIp)));

            if (result == null)
            {
                throw new Exception($"Unable to find a network interface for setting '{nameof(Configuration.BindIp)}' with a value of '{Configuration.BindIp}'");
            }

            return result;
        }

        private IPAddress GetInterfaceAddress(NetworkInterface nic)
        {
            foreach (var interfaceAddress in nic.GetIPProperties().UnicastAddresses)
            {
                if (interfaceAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return interfaceAddress.Address;
                }
            }

            return null;
        }

        private void StartListening(IPAddress address, int port)
        {
            _listener = new UdpClient(new IPEndPoint(address, port));
            _cancellation = new CancellationTokenSource();
            ReceiveLoop(_cancellation.Token);
        }

        private async void ReceiveLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var rsp = await _listener.ReceiveAsync()
                    .ConfigureAwait(false);

                    if (rsp.Buffer.Length > 0)
                    {
                        try
                        {
                            await ReceiveRequest(rsp.Buffer, rsp.RemoteEndPoint).ConfigureAwait(false);
                        }
                        catch
                        {
                            //TODO: Offer a way to handle that deserialization failed in some way unexpectedly, or notify the user
                        }
                    }
                }
                catch (ObjectDisposedException) { return; }
                catch
                {
                    //TODO: Possibly notify the user that Receive failed? This could be a SocketException.
                }
            }
        }

        private async Task ReceiveRequest(byte[] buffer, IPEndPoint remoteEndPoint)
        {
            DhcpData dhcpData = new DhcpData(remoteEndPoint, buffer);
            DhcpMessage dhcpMessage = ParseRequest(dhcpData, new DhcpPacketSerializer(), new DhcpMessageSerializer());
            await HandleRequest(dhcpMessage).ConfigureAwait(false);
        }

        private DhcpMessage ParseRequest(DhcpData dhcpData, IDhcpPacketSerializer pSerializer, IDhcpMessageSerializer mSerializer)
        {
            DhcpPacket packet = pSerializer.Deserialize(dhcpData.MessageBuffer);
            DhcpMessage message;

            try
            {
                message = mSerializer.ToMessage(packet);
            }
            catch (Exception ex)
            {
                Log.Error($"Error Parsing Dhcp Message {ex.Message}");
                throw;
            }

            return message;
        }
        
        private async Task HandleRequest(DhcpMessage message)
        {
            if (message.OperationCode == DhcpOperation.BootRequest)
            {
                switch (message.DhcpMessageType)
                {
                    case DhcpMessageType.Discover:
                        Log.Debug($"[DISCOVER] Message Received From '{message.HostName}'");
                        await this.DhcpDiscover(message).ConfigureAwait(false);
                        Log.Debug("[DISCOVER] Message Processed.");
                        break;
                    case DhcpMessageType.Request:
                        Log.Debug($"[REQUEST] Message Received From '{message.HostName}'");
                        await this.DhcpRequest(message).ConfigureAwait(false);
                        Log.Debug("[REQUEST] Message Processed.");
                        break;
                    case DhcpMessageType.Unknown:
                        Log.Warn("Unknown DHCP message type. Ignoring.");
                        break;
                    default:
                        Log.Debug($"Unhandled Dhcp Message ({message.DhcpMessageType}) Received, Ignoring.");
                        break;
                }
            }
            else
            {
                Log.Debug("Message came in, but it was not a BootRequest. Ignoring.");
            }
        }

        private async Task DhcpDiscover(DhcpMessage message)
        {
            var addressRequest = await LeaseManager.GetNextLease()
                .ConfigureAwait(false);

            await this.SendOffer(message, addressRequest)
                .ConfigureAwait(false);
        }

        private async Task DhcpRequest(DhcpMessage message)
        {
            await _requestLock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Client specified an address they would like
                if (message.Options.ContainsKey(DhcpOptionCode.RequestedIpAddress))
                {
                    await KeepAddressRequest(message).ConfigureAwait(false);
                }
                else
                {
                    var clientAddress = message.ClientIPAddress;

                    if (clientAddress.Equals(IPAddress.Parse("0.0.0.0")))
                    {
                        // A DHCP REQ should have an address
                        throw new Exception("A DHCP Request must have an address specified");
                    }

                    await LeaseManager.AddLease(clientAddress, message.ClientHardwareAddress, message.HostName)
                                .ConfigureAwait(false);

                    await this.SendAck(message, clientAddress).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
            _requestLock.Release();
        }

        private async Task KeepAddressRequest(DhcpMessage message)
        {
            var addressRequestData = message.Options[DhcpOptionCode.RequestedIpAddress];
            var addressRequest = new IPAddress(addressRequestData);
            Log.Debug($"[REQUEST] {message.ClientHardwareAddress} has requested to keep it's IP Address '{addressRequest}'");

            if (addressRequest.IsInSameSubnet(Configuration.StartIpAddress, Configuration.SubnetMask) == false)
            {
                Log.Debug($"[REQUEST] {message.ClientHardwareAddress} request for '{addressRequest}' has been DENIED due to subnet mismatch");
                await this.SendNak(message, addressRequest).ConfigureAwait(false);
                return;
            }

            var keepReservationResponse = await LeaseManager.KeepLeaseRequest(addressRequest, message.ClientHardwareAddress, message.HostName);
            if (keepReservationResponse)
            {
                await this.SendAck(message, addressRequest).ConfigureAwait(false);
                Log.Debug($"[REQUEST] {message.ClientHardwareAddress} has been approved!");
                return;
            }

            await this.SendNak(message, addressRequest).ConfigureAwait(false);
            Log.Debug($"[REQUEST] {message.ClientHardwareAddress} has been DENIED.");
        }

        private async Task SendOffer(DhcpMessage message, IPAddress offerAddress)
        {
            Log.Debug($"[OFFER] Creating the offer");

            message.OperationCode = DhcpOperation.BootReply;
            message.YourIPAddress = offerAddress;
            message.ServerIPAddress = DhcpInterfaceAddress;

            var optionBuilder = new DhcpOptionBuilder();
            optionBuilder.AddOption(DhcpOptionCode.DhcpMessageType, DhcpMessageType.Offer);
            optionBuilder.AddOption(DhcpOptionCode.DhcpAddress, DhcpInterfaceAddress);
            optionBuilder.AddOption(DhcpOptionCode.SubnetMask, Configuration.SubnetMask);
            optionBuilder.AddOption(DhcpOptionCode.Router, Configuration.Gateway);
            optionBuilder.AddOption(DhcpOptionCode.AddressTime, Configuration.LeaseTimeSeconds, true);

            var s = new DhcpMessageSerializer();
            var packet = s.ToPacket(message, optionBuilder.GetBytes());

            await this.SendReply(packet).ConfigureAwait(false);
                 

            Log.Debug($"[OFFER] IP Address '{offerAddress}' was sent over '{IPAddress.Broadcast}'");
        }

        private async Task SendAck(DhcpMessage message, IPAddress clientAddress)
        {
            Log.Debug($"[ACK] Creating the acknowledgement");

            message.OperationCode = DhcpOperation.BootReply;
            message.YourIPAddress = clientAddress;
            message.ServerIPAddress = DhcpInterfaceAddress;

            var optionBuilder = new DhcpOptionBuilder();
            optionBuilder.AddOption(DhcpOptionCode.DhcpMessageType, DhcpMessageType.Ack);
            optionBuilder.AddOption(DhcpOptionCode.DhcpAddress, DhcpInterfaceAddress);
            optionBuilder.AddOption(DhcpOptionCode.SubnetMask, Configuration.SubnetMask);
            optionBuilder.AddOption(DhcpOptionCode.Router, Configuration.Gateway);
            optionBuilder.AddOption(DhcpOptionCode.AddressTime, Configuration.LeaseTimeSeconds, true);

            var s = new DhcpMessageSerializer();
            var packet = s.ToPacket(message, optionBuilder.GetBytes());

            await this.SendReply(packet).ConfigureAwait(false);

            Log.Debug($"[ACK] IP Address '{clientAddress}' was sent over '{IPAddress.Broadcast}'");
        }

        private async Task SendNak(DhcpMessage message, IPAddress requestedAddress)
        {
            Log.Debug("[NAK] Creating the negative acknowledgement");

            message.OperationCode = DhcpOperation.BootReply;
            message.YourIPAddress = requestedAddress;
            message.ServerIPAddress = DhcpInterfaceAddress;

            var optionBuilder = new DhcpOptionBuilder();
            optionBuilder.AddOption(DhcpOptionCode.DhcpMessageType,DhcpMessageType.Nak);

            var s = new DhcpMessageSerializer();
            var packet = s.ToPacket(message, optionBuilder.GetBytes());

            await this.SendReply(packet).ConfigureAwait(false);

            Log.Debug($"[NAK] IP Address '{requestedAddress}' was sent over '{IPAddress.Broadcast}'");
        }

        private async Task SendReply(DhcpPacket packet)
        {
            try
            {
                var response = packet.GetBytes(new DhcpPacketSerializer());
                using (UdpClient client = new UdpClient())
                {
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, DHCP_CLIENT_PORT);
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);

                    await client.SendAsync(response, response.Length, endPoint)
                        .ConfigureAwait(false);               
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error sending Dhcp reply: {ex.Message}");
                throw;
            }
        }
    }
}
