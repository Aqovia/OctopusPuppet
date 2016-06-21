using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OctopusPuppet.DeploymentPlanner
{
    public class ComponentFilter
    {
        [JsonProperty(Required = Required.AllowNull)]
        public bool Include { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<string> Expressions { get; set; }

        public bool Match(string project)
        {
            var matchExpression = Expressions
                .Select(expression => Regex.Match(project, expression))
                .Any(match => match.Success);

            return Include ? matchExpression : !matchExpression;
        }

        public ComponentFilter()
        {
            Expressions = new List<string>();
        }
    }
}