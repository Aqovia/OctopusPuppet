using System;
using System.Linq;
using Octopus.Client.Model;
using Octopus.Client.Repositories;
using OctopusPuppet.DeploymentPlanner;
using Environment = OctopusPuppet.DeploymentPlanner.Environment;

namespace OctopusPuppet.OctopusProvider
{
    internal static class ResourceExtensions
    {
        /// <summary>
        /// Find first release for project that matches version
        /// </summary>
        /// <param name="projects">The project repository</param>
        /// <param name="projectId">The project id to match on</param>
        /// <param name="version">Version for the project to match on</param>
        /// <returns>Matched release or null if there is no match</returns>        
        public static ReleaseResource GetRelease(this IProjectRepository projects, string projectId, SemVer version)
        {
            var project = projects.Get(projectId);
            var skip = 0;
            var shouldPage = false;
            do
            {
                var releasePages = projects.GetReleases(project, skip);

                var release = releasePages.Items.FirstOrDefault(x => new SemVer(x.Version) == version);

                if (release != null)
                {
                    return release;
                }

                skip += releasePages.ItemsPerPage;
                shouldPage = releasePages.TotalResults > skip;
            } while (shouldPage);

            throw new Exception(string.Format("Can't find release with project id {0} and version of {1}", projectId, version));
        }

        /// <summary>
        /// Find first environment by name
        /// </summary>
        /// <param name="environments">The environment repository</param>
        /// <param name="environment">The environment name</param>
        /// <returns>Environment object or null</returns>
        public static Environment GetEnvironment(this IEnvironmentRepository environments, string environment)
        {
            var result = environments
                .GetAll()
                .Select(x => new DeploymentPlanner.Environment()
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefault(x => x.Name == environment);

            if (result == null)
            {
                throw new Exception(string.Format("Can't find environment name of {0}", environment));
            }

            return result;
        }

        /// <summary>
        /// Find first project by name
        /// </summary>
        /// <param name="projects">The project repository</param>
        /// <param name="project"></param>
        /// <returns>Reference object or null</returns>
        public static ReferenceDataItem GetProjectByName(this IProjectRepository projects, string project)
        {
            var result = projects
                .GetAll()
                .FirstOrDefault(x => x.Name == project);

            if (result == null)
            {
                throw new Exception(string.Format("Can't find project with name of {0}", project));
            }

            return result;
        }
    }
}