namespace Illarion.Client.Map
{
    public class Light 
    {
        public float Red {get;}
        public float Green {get;}
        public float Blue {get;}
        public float Size {get;}
        public float Brightness {get;}
        public bool Inverted {get;}

        public Light(int encodedLight)
        {
            Red = ((encodedLight / 100) % 10) / 9.0f;
            Green = ((encodedLight / 10) % 10) / 9.0f;
            Blue = (encodedLight % 10) / 9.0f;

            Brightness = ((encodedLight / 1000) % 10) / 9.0f;
            Size = (encodedLight / 10000) % 10;
            Inverted = (encodedLight / 100000) == 1;
        }
    }
}