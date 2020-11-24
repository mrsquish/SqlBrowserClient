using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public record SqlInstance (string ServerName, string InstanceName, bool IsClustered, string Version, int TcpPort, string NamedPipe);
}
