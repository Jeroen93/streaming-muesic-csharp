namespace Streaming_Muesic_WPF.minim
{
    class RectangularWindow : WindowFunction
    {
        public RectangularWindow()
        {
        }

        protected override float Value(int length, int index)
        {
            return 1f;
        }

        public override string ToString()
        {
            return "Rectangular Window";
        }
    }
}
