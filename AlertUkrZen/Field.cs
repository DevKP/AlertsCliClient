namespace AlertUkrZen
{
    public class Field
    {
        public int Padding { get; set; }
        public string Data { get; set; }

        public Field(string data)
        {
            Data = data;
        }
    }
}