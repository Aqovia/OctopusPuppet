using System.Collections.Generic;

namespace OctopusPuppet
{
    public class ComponentGroupVertex
    {
        public IEnumerable<ComponentVertex> Vertices { get; set; }
        public IEnumerable<ComponentEdge> Edges { get; set; }
        public int ExecutionOrder { get; set; }

        public ComponentGroupVertex(IEnumerable<ComponentVertex> vertices, IEnumerable<ComponentEdge> edges)
        {
            Vertices = vertices;
            Edges = edges;
            ExecutionOrder = -1;
        }
    }
}