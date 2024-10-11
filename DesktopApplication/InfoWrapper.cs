using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApplication
{

    public class InfoWrapper
    {
        public static InfoWrapper? Instance;
        public string? Token { get; set; }
        public static InfoWrapper GetInstance()
        {
            if (Instance == null)
            {
                Instance = new InfoWrapper();
            }
            return Instance;
        }
    }
}
