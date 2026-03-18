namespace DotnetAPI.Models
{
    // why partial? - because we want to be able to add properties to this class without modifying the auto-generated code that creates it based on the database schema. By using partial, we can create another file where we define additional properties or methods for the Posts class, and it will be combined with this auto-generated part at compile time. This way, if we need to regenerate the model from the database schema, we won't lose any custom code we've added in the separate partial class file.
    public partial class Post
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string PostTitle { get; set; } = "";
        public string PostContent { get; set; } = "";
        public DateTime PostCreated { get; set; }
        public DateTime PostUpdated { get; set; }
    }
}