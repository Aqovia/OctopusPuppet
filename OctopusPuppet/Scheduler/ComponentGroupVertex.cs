using System;
using System.Collections.Generic;
using System.Linq;

namespace OctopusPuppet.Scheduler
{
    public class ComponentGroupVertex
    {
        public IEnumerable<ComponentVertex> Vertices { get; set; }
        public IEnumerable<ComponentEdge> Edges { get; set; }
        public int ExecutionOrder { get; set; }

        public TimeSpan? DeploymentDuration
        {
            get { 
                return Vertices
                .Select(x => x.DeploymentDuration)
                .Max(); 
            }
        }

        public ComponentGroupVertex(IEnumerable<ComponentVertex> vertices, IEnumerable<ComponentEdge> edges)
        {
            Vertices = vertices;
            Edges = edges;
            ExecutionOrder = -1;
        }
    }
}