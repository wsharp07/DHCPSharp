# DHCPSharp
Simple DHCP Server written in C#

Based on RFC2131: https://www.ietf.org/rfc/rfc2131.txt

### What works

* IP Addresses are persisted in a SQLite data store
* IP leases which expired are cleaned up

**Supported DHCP Message Types**

* DHCP Discovery
* DHCP Offer
* DHCP Request
* DHCP ACK
* DHCP NAK

## What doesn't work

* IP Addresses are not reused in the event the lease expires

**Not Supported DHCP Message Types**

* DHCP Release
* DHCP Decline
* DHCP Inform

## Configuration

You must set the configuration prior to running the server. Configuration is set
in the `App.config` XML file.

`StartIpAddress`
IP address to start assigning at

`EndIpAddress`
IP address to stop assigning at

`GatewayIpAddress`
IP address of your network gateway (router)

`SubnetIpAddress`
Subnet mask you are assigning addresses within

`LeaseTimeoutSeconds`
How long, in seconds, the client should hold it's lease.
**note: 0 indicates an infinite lease**

`BindIpAddress`
The IP address of the network adapter you're binding to.
**note: if not set it is assumed loopback**

