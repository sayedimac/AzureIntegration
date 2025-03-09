namespace AzureIntegration.Services
{
    public class ResourceGroup
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
    }


    public class AzureResource
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
    }
}
