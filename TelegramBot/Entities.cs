using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
    class Channel
    {
        public string Name;
        public long ID ;

        public override string ToString()
        {
            return Name;
        }
    }

    class Group
    {
        public string Name;
        public long ID;

        public override string ToString()
        {
            return Name;
        }
    }
}
