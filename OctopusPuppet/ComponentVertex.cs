namespace OctopusPuppet
{
    public class ComponentVertex
    {
        public string OctopusProject { get; private set; }
        public ComponentAction Action { get; private set; }
        public int ComponentGroup { get; set; }
        public int ProductGroup { get; set; }
        public int ExecutionOrder { get; set; }

        public ComponentVertex(string octopusProject, ComponentAction action)
        {
            OctopusProject = octopusProject;
            Action = action;
            ComponentGroup = -1;
            ProductGroup = -1;
            ExecutionOrder = -1;
        }
    }
}
