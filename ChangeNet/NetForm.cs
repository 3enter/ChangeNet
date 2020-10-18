using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;

namespace ChangeNet
{
    public partial class NetForm : Telerik.WinControls.UI.RadForm
    {
        private const string Fast = "192.168.1.254";
        private const string Normal = "192.168.1.1";
        public List<NetInfo> NetList { get; set; }
        public NetForm()
        {
            InitializeComponent();
        }
        public static bool SetIpAddress(string ipAddress, string subnetMask, string ID, string gateway = null, string dns1 = null, string dns2 = null)
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] && (string)mo["SettingID"] == ID)
                {
                    try
                    {
                        ManagementBaseObject newIP = mo.GetMethodParameters("EnableStatic");

                        newIP["IPAddress"] = new string[] { ipAddress };
                        newIP["SubnetMask"] = new string[] { subnetMask };

                        ManagementBaseObject setIP = mo.InvokeMethod("EnableStatic", newIP, null);

                        if (gateway != null)
                        {
                            ManagementBaseObject newGateway = mo.GetMethodParameters("SetGateways");

                            newGateway["DefaultIPGateway"] = new string[] { gateway };
                            newGateway["GatewayCostMetric"] = new int[] { 1 };

                            ManagementBaseObject setGateway = mo.InvokeMethod("SetGateways", newGateway, null);
                        }


                        if (dns1 != null || dns2 != null)
                        {
                            ManagementBaseObject newDns = mo.GetMethodParameters("SetDNSServerSearchOrder");
                            var dns = new List<string>();

                            if (dns1 != null)
                            {
                                dns.Add(dns1);
                            }

                            if (dns2 != null)
                            {
                                dns.Add(dns2);
                            }

                            newDns["DNSServerSearchOrder"] = dns.ToArray();

                            ManagementBaseObject setDNS = mo.InvokeMethod("SetDNSServerSearchOrder", newDns, null);
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public static bool SetDHCP(string ID)
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] && (string)mo["SettingID"] == ID)
                {
                    try
                    {
                        ManagementBaseObject newDNS = mo.GetMethodParameters("SetDNSServerSearchOrder");

                        newDNS["DNSServerSearchOrder"] = null;
                        ManagementBaseObject enableDHCP = mo.InvokeMethod("EnableDHCP", null, null);
                        ManagementBaseObject setDNS = mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public void setGateway(string gateway, string ID)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"] && (string)objMO["SettingID"] == ID)
                {
                    ManagementBaseObject setGateway;
                    ManagementBaseObject newGateway =
                      objMO.GetMethodParameters("SetGateways");

                    newGateway["DefaultIPGateway"] = new string[] { gateway };
                    newGateway["GatewayCostMetric"] = new int[] { 1 };

                    setGateway = objMO.InvokeMethod("SetGateways", newGateway, null);
                }
            }
        }
        public void Referesh()
        {
            var vv = NetworkInterface.GetAllNetworkInterfaces().Where(s => !s.Description.Contains("Virtual") && s.NetworkInterfaceType != NetworkInterfaceType.Loopback).Select(s => new NetInfo
            {
                ID = s.Id,
                Name = s.Name,
                IP = s?.GetIPProperties()?.UnicastAddresses?.Where(v => v.SuffixOrigin == SuffixOrigin.OriginDhcp || v.SuffixOrigin == SuffixOrigin.Manual)?.FirstOrDefault()?.Address?.ToString(),
                Getway = s?.GetIPProperties().GatewayAddresses?.FirstOrDefault()?.Address?.ToString(),
                DHCP = s?.GetIPProperties()?.GetIPv4Properties()?.IsDhcpEnabled,
            }).ToList();
            NetList.ForEach(x => { var c = vv.Where(s => s.ID == x.ID)?.FirstOrDefault(); x.Getway=c.Getway; x.IP = c.IP; x.DHCP = c.DHCP; x.Name = c.Name; x.Speed = c.Getway == Fast ? NetSpeed.Fast : c.Getway == Normal ? NetSpeed.Normal : NetSpeed.None; });
            radGridView1.DataSource = null;
            radGridView1.DataSource = NetList;
            GridInit();

        }
        public void GridInit()
        {
            radGridView1.Columns["ID"].IsVisible = false;
            radGridView1.Columns["Status"].IsVisible = false;
            radGridView1.Columns["Name"].ReadOnly = true;
            
            radGridView1.AutoSizeColumnsMode = Telerik.WinControls.UI.GridViewAutoSizeColumnsMode.Fill;
        }
        private void NetForm_Load(object sender, EventArgs e)
        {
            NetList = NetworkInterface.GetAllNetworkInterfaces().Where(s => !s.Description.Contains("Virtual") && s.NetworkInterfaceType != NetworkInterfaceType.Loopback).Select(s => new NetInfo
            {
                ID = s.Id,
                Name = s.Name,
                IP = s?.GetIPProperties()?.UnicastAddresses?.Where(v => v.SuffixOrigin == SuffixOrigin.OriginDhcp || v.SuffixOrigin == SuffixOrigin.Manual)?.FirstOrDefault()?.Address?.ToString(),
                Getway = s?.GetIPProperties().GatewayAddresses?.FirstOrDefault()?.Address?.ToString(),
                DHCP = s?.GetIPProperties()?.GetIPv4Properties()?.IsDhcpEnabled,
                Status = s.OperationalStatus,
            }).ToList();
            NetList.ForEach(x => x.Speed = x.Getway == Fast ? NetSpeed.Fast : x.Getway == Normal ? NetSpeed.Normal : NetSpeed.None);
            radGridView1.DataSource = NetList;
            GridInit();

        }

        private void radGridView1_CellEndEdit(object sender, Telerik.WinControls.UI.GridViewCellEventArgs e)
        {
            var Data = (NetInfo)e.Row.DataBoundItem;
            if (e.Column.Name == nameof(NetInfo.Getway))
            {
                setGateway(Data.Getway?.ToString(), Data.ID);
            }
            if (e.Column.Name == nameof(NetInfo.IP))
            {
                SetIpAddress(Data.IP?.ToString(), "255.255.255.0", Data.ID, Data?.Getway?.ToString(), "1.1.1.1", "1.0.0.1");
            }
            if (e.Column.Name == nameof(NetInfo.DHCP))
            {
                if (Data.DHCP == true && Data.DHCP != null)
                    SetDHCP(Data.ID);
                else
                    SetIpAddress(Data.IP?.ToString(), "255.255.255.0", Data.ID, Data?.Getway?.ToString(), "1.1.1.1", "1.0.0.1");
            }
            if (e.Column.Name == nameof(NetInfo.Speed))
            {
                if (Data.Speed == NetSpeed.Fast)
                    setGateway(Fast, Data.ID);
                if (Data.Speed == NetSpeed.Normal)
                    setGateway(Normal, Data.ID);
            }
            Referesh();
        }


        private void radGridView1_RowFormatting(object sender, Telerik.WinControls.UI.RowFormattingEventArgs e)
        {
            var data = (NetInfo)e.RowElement.Data.DataBoundItem;
            if (data.Status != OperationalStatus.Up)
                e.RowElement.Enabled = false;
            else
                e.RowElement.Enabled = true;
        }
    }

}
