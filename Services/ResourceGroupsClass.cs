namespace AzureIntegration.Services
{
    public class ResourceGroup
    {
        public string Name { get; set; }
        public string Location { get; set; }
    }

    public class ResourceGroups
    {
        public List<ResourceGroup> Value { get; set; }
    }
}
