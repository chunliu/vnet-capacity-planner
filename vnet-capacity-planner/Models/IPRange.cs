using System.Net;

namespace vnet_capacity_planner.Models
{
    public class IPRange
    {
        public string StartIP { get; set; }
        private int _cidr = 0;
        private IPNetwork _ipNetwork = null;

        public string AddressSpace 
        { 
            get
            {
                return string.IsNullOrEmpty(StartIP) ? string.Empty : StartIP + "/" + Cidr.ToString();
            }
        }

        public string AddressCount
        {
            get
            {
                if (string.IsNullOrEmpty(StartIP))
                    return "0";

                if (_ipNetwork == null)
                    _ipNetwork = IPNetwork.Parse(AddressSpace);

                return _ipNetwork.Total.ToString();
            }
        }

        public int Cidr
        {
            get
            {
                return _cidr;
            }
            set
            {
                if (_cidr != value)
                {
                    _cidr = value;
                    if (!string.IsNullOrEmpty(StartIP))
                        _ipNetwork = IPNetwork.Parse(AddressSpace);
                }
            }
        }

        public string AddressRange
        {
            get
            {
                if (string.IsNullOrEmpty(StartIP))
                    return "";

                if (_ipNetwork == null)
                    _ipNetwork = IPNetwork.Parse(AddressSpace);

                return $"{_ipNetwork.Network} - {_ipNetwork.Broadcast}";
            }
        }
    }
}
