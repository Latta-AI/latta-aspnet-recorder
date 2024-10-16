using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LattaASPNet
{
    public class LattaLog
    {
        public string message;
        public string level;
        public long timestamp;

        public LattaLog(string message, string level)
        {
            this.message = message;
            this.level = level;
            this.timestamp = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();
        }
    }
}
