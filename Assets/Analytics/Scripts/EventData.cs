using System;

namespace Analytics
{
    [Serializable]
    public struct EventData
    {
        public string type;
        public string data;

        public EventData(string type, string data)
        {
            this.type = type;
            this.data = data;
        }
    }
}