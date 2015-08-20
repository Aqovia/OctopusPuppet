namespace OctopusPuppet
{
    public class ComponentVertex
    {
        public string OctopusProject { get; private set; }
        public ComponentAction Action { get; private set; }
        public int Group { get; set; }

        public ComponentVertex(string octopusProject, ComponentAction action)
        {
            OctopusProject = octopusProject;
            Action = action;
        }
    }
}
