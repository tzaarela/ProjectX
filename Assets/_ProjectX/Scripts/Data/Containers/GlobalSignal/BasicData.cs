namespace _Content.Scripts.Data.Containers.GlobalSignal
{
    public class BasicData : GlobalSignalBaseData
    {
        public int intValue;
        public float floatValue;
        public string stringValue;

        public BasicData()
        {

        }

        public BasicData(int intValue)
        {
            this.intValue = intValue;
        }

        public BasicData(float floatValue)
        {
            this.floatValue = floatValue;
        }

        public BasicData(string stringValue)
        {
            this.stringValue = stringValue;
        }
    }
}