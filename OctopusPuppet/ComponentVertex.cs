namespace OctopusPuppet
{
    public class ComponentVertex
    {
        public string OctopusProject { get; private set; }
        public ComponentAction Action { get; private set; }
        public int StepGroup { get; set; }
        public int ProductGroup { get; set; }
        public int Level { get; set; }

        public ComponentVertex(string octopusProject, ComponentAction action)
        {
            OctopusProject = octopusProject;
            Action = action;
        }
    }
}
