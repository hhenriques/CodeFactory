namespace CodeFactory {

    public class TypedArgument {

        public string Type { get; set; }
        public string Name { get; set; }

        public TypedArgument(string type, string name) {
            Type = type;
            Name = name;
        }

    }
}
