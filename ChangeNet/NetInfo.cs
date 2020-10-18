using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChangeNet
{

    public class NetInfo
    {

        public string ID { get; set; }

        public string Name { get; set; }
        public string IP { get; set; }
        public string Getway { get; set; }
        public bool? DHCP { get; set; }
        public NetSpeed? Speed { get; set; }
        public OperationalStatus Status { get; set; }

    }
    public enum NetSpeed
    {
        [Description("هیچکدام")]
        None = 0,
        [Description("پرسرعت")]
        Fast = 1,
        [Description("نرمال")]
        Normal = 2
    }
}
