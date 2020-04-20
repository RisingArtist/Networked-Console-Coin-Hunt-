using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
     //What info should the player care about across the network?
    class ClientInfo
    {
        public int posX;
        public int posY;
        public int score;
        public bool justGot;
        public bool disconnect;

        public ClientInfo(int x, int y, int m_score)
        {
            posX = x;
            posY = y;
            score = m_score;
            justGot = false;
        }
    }
}
