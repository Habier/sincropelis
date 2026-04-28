namespace SincroPelis.Playback
{
    public sealed class TrackOption
    {
        public TrackOption(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }
        public int Id { get; }

        public override string ToString() => Name;
    }
}
