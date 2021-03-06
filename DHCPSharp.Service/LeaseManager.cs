﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DHCPSharp.Common;
using DHCPSharp.DAL.Repository;
using DHCPSharp.Common.Extensions;
using DHCPSharp.DAL;
using DHCPSharp.DAL.Models;
using SQLite;

namespace DHCPSharp
{
    public interface ILeaseManager
    {
        Task<IPAddress> GetNextLease();
        Task<IPAddress> GetLastLease();
        Task AddLease(IPAddress ipAddress, PhysicalAddress physicalAddress, string hostName);
        Task<bool> KeepLeaseRequest(IPAddress ipAddress, PhysicalAddress physicalAddress, string hostName);
        Task RemoveLease(IPAddress ipAddress);
        Task CleanExpiredLeases();
    }
    public class LeaseManager : ILeaseManager
    {
        readonly ILeaseRepo _leaseRepo;
        readonly IDhcpConfiguration _configuration;
 
        public LeaseManager(IDhcpConfiguration configuration, ILeaseRepo repo)
        {
            _leaseRepo = repo;
            _configuration = configuration;
        }

        public async Task<IPAddress> GetNextLease()
        {
            var leaseCount = await _leaseRepo.AsQueryable().CountAsync().ConfigureAwait(false);

            if (leaseCount > 0)
            {
                var leaseLease = await GetLastLease().ConfigureAwait(false);
                var nextIpAddress = leaseLease.ToNextIpAddress();
                return nextIpAddress;
            }
            return _configuration.StartIpAddress;
        }

        public async Task<IPAddress> GetLastLease()
        {
            var lastLease = await _leaseRepo.AsQueryable().OrderByDescending(x => x.InsertedAt).FirstAsync().ConfigureAwait(false);
            return IPAddress.Parse(lastLease.IpAddress);
        }

        public async Task AddLease(IPAddress ipAddress, PhysicalAddress physicalAddress, string hostName)
        {
            var lease = new Lease
            {
                HostName = hostName,
                PhysicalAddress = physicalAddress.ToString(),
                IpAddress = ipAddress.ToString(),
                Expiration = DateTime.UtcNow.AddSeconds(_configuration.LeaseTimeSeconds)
            };

            await _leaseRepo.Insert(lease);
        }

        public async Task<bool> KeepLeaseRequest(IPAddress ipAddress, PhysicalAddress physicalAddress, string hostName)
        {
            var lease = await _leaseRepo.GetByIpAddress(ipAddress).ConfigureAwait(false);

            if (lease == null)
            {
                lease = await _leaseRepo.GetByPhysicalAddress(physicalAddress).ConfigureAwait(false);

                if(lease == null)
                {
                    await AddLease(ipAddress, physicalAddress, hostName).ConfigureAwait(false);
                    return true;
                }
                await UpdateLease(lease, ipAddress, physicalAddress, hostName).ConfigureAwait(false);
                return true;
            }

            if (lease.PhysicalAddress.Equals(physicalAddress.ToString()))
            {
                await UpdateLease(lease, ipAddress, physicalAddress, hostName).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private async Task UpdateLease(Lease lease, IPAddress ipAddress, PhysicalAddress physicalAddress, string hostName)
        {
            lease.IpAddress = ipAddress.ToString();
            lease.PhysicalAddress = physicalAddress.ToString();
            lease.HostName = hostName;
            lease.Expiration = DateTime.UtcNow.AddSeconds(_configuration.LeaseTimeSeconds);
            lease.UpdatedAt = DateTime.UtcNow;
            await _leaseRepo.Update(lease).ConfigureAwait(false);
        }

        public async Task RemoveLease(IPAddress ipAddress)
        {
            var lease = await _leaseRepo.GetByIpAddress(ipAddress);
            await _leaseRepo.Delete(lease).ConfigureAwait(false);
        }

        public async Task CleanExpiredLeases()
        {
            var expiredLeases = await _leaseRepo
                                        .AsQueryable()
                                        .Where(x => x.Expiration <= DateTime.UtcNow)
                                        .ToListAsync();

            foreach(var lease in expiredLeases)
            {
                await _leaseRepo.Delete(lease).ConfigureAwait(false);
            }
        }
    }
}
