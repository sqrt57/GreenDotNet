namespace Green
{
    public readonly struct Identifier
    {
        private readonly string Name;

        public Identifier(string name)
        {
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj is Identifier other)
                return Equals(other);
            return false;
        }

        public bool Equals(Identifier other)
        {
            return Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
