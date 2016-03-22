using System.Collections.Generic;
using System.Configuration;
using Caliburn.Micro;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Scheduler;
using QuickGraph;

namespace OctopusPuppet.Gui.ViewModels
{
    public class DeploymentPlannerViewModel : PropertyChangedBase
    {
        public DeploymentPlannerViewModel()
        {
            _branchDeploymentBranches = new List<string>();
            _selectedBranchDeploymentBranch = string.Empty;

            _branchDeploymentEnvironments = new List<string>();
            _selectedBranchDeploymentEnvironment = string.Empty;

            _redeploymentEnvironments = new List<string>();
            _selectedRedeploymentEnvironment = string.Empty;

            _environmentMirrorFromEnvironments = new List<string>();
            _selectedEnvironmentMirrorFromEnvironment = string.Empty;

            _environmentMirrorToEnvironments = new List<string>();
            _selectedEnvironmentMirrorToEnvironment = string.Empty;

            OctopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            OctopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
        }

        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                NotifyOfPropertyChange(() => Loading);
            }
        }

        private List<string> _branches;
        private List<string> Branches
        {
            get { return _branches; }
            set
            {
                _branches = value;
                BranchDeploymentBranches = value;
            }
        }

        private List<string> _environments;
        private List<string> Environments
        {
            get { return _environments; }
            set
            {
                _environments = value;

                BranchDeploymentEnvironments = value;
                RedeploymentEnvironments = value;
                EnvironmentMirrorFromEnvironments = value;
                EnvironmentMirrorToEnvironments = value;
            }
        }

        private List<DeploymentPlan> _deploymentPlans;
        private List<DeploymentPlan> DeploymentPlans
        {
            get { return _deploymentPlans; }
            set
            {
                _deploymentPlans = value;
                NotifyOfPropertyChange(() => DeploymentPlans);
            }
        }

        private IBidirectionalGraph<ComponentVertex, ComponentEdge> _graph;
        public IBidirectionalGraph<ComponentVertex, ComponentEdge> Graph
        {
            get { return _graph; }
            private set
            {
                _graph = value;
                NotifyOfPropertyChange(() => Graph);
            }
        }

        private List<List<ComponentGroupVertex>> _products;
        public List<List<ComponentGroupVertex>> Products
        {
            get { return _products; }
            private set
            {
                _products = value;
                NotifyOfPropertyChange(() => Products);
            }
        }

        private void GetBranchesAndEnvironments()
        {
            Loading = true;
            if (!string.IsNullOrEmpty(_octopusUrl) && !string.IsNullOrEmpty(_octopusApiKey))
            {
                try
                {
                    var deploymentPlanner = new OctopusDeploymentPlanner(_octopusUrl, _octopusApiKey);

                    Branches = deploymentPlanner.GetBranches();
                    Environments = deploymentPlanner.GetEnvironments();
                }
                catch
                {
                    Branches = new List<string>();
                    Environments = new List<string>();                  
                }
            }
            else
            {
                Branches = new List<string>();
                Environments = new List<string>();
            }
            Loading = false;
        }

        private string _octopusApiKey;
        public string OctopusApiKey
        {
            get { return _octopusApiKey; }
            set
            {
                _octopusApiKey = value;

                GetBranchesAndEnvironments();

                NotifyOfPropertyChange(() => OctopusApiKey);

                NotifyOfPropertyChange(() => CanBranchDeployment);
                NotifyOfPropertyChange(() => CanRedeployment);
                NotifyOfPropertyChange(() => CanEnvironmentMirror);
            }
        }

        private string _octopusUrl;
        public string OctopusUrl
        {
            get { return _octopusUrl; }
            set
            {
                _octopusUrl = value;

                GetBranchesAndEnvironments();

                NotifyOfPropertyChange(() => OctopusUrl);

                NotifyOfPropertyChange(() => CanBranchDeployment);
                NotifyOfPropertyChange(() => CanRedeployment);
                NotifyOfPropertyChange(() => CanEnvironmentMirror);
            }
        }

        private List<string> _branchDeploymentBranches;
        public List<string> BranchDeploymentBranches
        {
            get { return _branchDeploymentBranches; }
            private set
            {
                _branchDeploymentBranches = value;
                NotifyOfPropertyChange(() => BranchDeploymentBranches);
            }
        }

        private string _selectedBranchDeploymentBranch;
        public string SelectedBranchDeploymentBranch
        {
            get { return _selectedBranchDeploymentBranch; }
            set
            {
                _selectedBranchDeploymentBranch = value;
                NotifyOfPropertyChange(() => SelectedBranchDeploymentBranch);

                NotifyOfPropertyChange(() => CanBranchDeployment);
            }
        }

        private List<string> _branchDeploymentEnvironments;
        public List<string> BranchDeploymentEnvironments
        {
            get { return _branchDeploymentEnvironments; }
            private set
            {
                _branchDeploymentEnvironments = value;
                NotifyOfPropertyChange(() => BranchDeploymentEnvironments);
            }
        }

        private string _selectedBranchDeploymentEnvironment;
        public string SelectedBranchDeploymentEnvironment
        {
            get { return _selectedBranchDeploymentEnvironment; }
            set
            {
                _selectedBranchDeploymentEnvironment = value;
                NotifyOfPropertyChange(() => SelectedBranchDeploymentEnvironment);

                NotifyOfPropertyChange(() => CanBranchDeployment);
            }
        }

        private List<string> _redeploymentEnvironments;
        public List<string> RedeploymentEnvironments
        {
            get { return _redeploymentEnvironments; }
            private set
            {
                _redeploymentEnvironments = value;
                NotifyOfPropertyChange(() => RedeploymentEnvironments);
            }
        }

        private string _selectedRedeploymentEnvironment;
        public string SelectedRedeploymentEnvironment
        {
            get { return _selectedRedeploymentEnvironment; }
            set
            {
                _selectedRedeploymentEnvironment = value;
                NotifyOfPropertyChange(() => SelectedRedeploymentEnvironment);

                NotifyOfPropertyChange(() => CanRedeployment);
            }
        }

        private List<string> _environmentMirrorFromEnvironments;
        public List<string> EnvironmentMirrorFromEnvironments
        {
            get { return _environmentMirrorFromEnvironments; }
            private set
            {
                _environmentMirrorFromEnvironments = value;
                NotifyOfPropertyChange(() => EnvironmentMirrorFromEnvironments);
            }
        }

        private string _selectedEnvironmentMirrorFromEnvironment;
        public string SelectedEnvironmentMirrorFromEnvironment
        {
            get { return _selectedEnvironmentMirrorFromEnvironment; }
            set
            {
                _selectedEnvironmentMirrorFromEnvironment = value;
                NotifyOfPropertyChange(() => SelectedEnvironmentMirrorFromEnvironment);

                NotifyOfPropertyChange(() => CanEnvironmentMirror);
            }
        }

        private List<string> _environmentMirrorToEnvironments;
        public List<string> EnvironmentMirrorToEnvironments
        {
            get { return _environmentMirrorToEnvironments; }
            private set
            {
                _environmentMirrorToEnvironments = value;
                NotifyOfPropertyChange(() => EnvironmentMirrorToEnvironments);
            }
        }

        private string _selectedEnvironmentMirrorToEnvironment;
        public string SelectedEnvironmentMirrorToEnvironment
        {
            get { return _selectedEnvironmentMirrorToEnvironment; }
            set
            {
                _selectedEnvironmentMirrorToEnvironment = value;
                NotifyOfPropertyChange(() => SelectedEnvironmentMirrorToEnvironment);

                NotifyOfPropertyChange(() => CanEnvironmentMirror);
            }
        }

        public bool CanBranchDeployment
        {
            get
            {
                return !(string.IsNullOrEmpty(_octopusApiKey) 
                    || string.IsNullOrEmpty(_octopusApiKey) 
                    || string.IsNullOrEmpty(_selectedBranchDeploymentEnvironment) 
                    || string.IsNullOrEmpty(_selectedBranchDeploymentBranch));
            }
        }

        public void BranchDeployment()
        {
            if (!CanBranchDeployment) return;
            Loading = true;
            try
            {
                var deploymentPlanner = new OctopusDeploymentPlanner(_octopusUrl, _octopusApiKey);
                var branchDeploymentPlans = deploymentPlanner.GetBranchDeploymentPlans(_selectedBranchDeploymentEnvironment, _selectedBranchDeploymentBranch);
                DeploymentPlans = branchDeploymentPlans.DeploymentPlans;

                var deploymentScheduler = new DeploymentScheduler();

                var componentGraph = deploymentScheduler.GetDeploymentComponentGraph(DeploymentPlans);
                Graph = componentGraph.ToBidirectionalGraph();
                Products = deploymentScheduler.GetDeploymentSchedule(componentGraph);
            }
            catch
            {
                DeploymentPlans = new List<DeploymentPlan>();
            }
            Loading = false;
        }

        public bool CanRedeployment
        {
            get
            {
                return !(string.IsNullOrEmpty(_octopusApiKey) 
                    || string.IsNullOrEmpty(_octopusApiKey) 
                    || string.IsNullOrEmpty(_selectedRedeploymentEnvironment));
            }
        }

        public void Redeployment()
        {
            if (!CanRedeployment) return;
            Loading = true;
            try
            {
                var deploymentPlanner = new OctopusDeploymentPlanner(_octopusUrl, _octopusApiKey);
                var redeployDeploymentPlans = deploymentPlanner.GetRedeployDeploymentPlans(_selectedRedeploymentEnvironment);
                DeploymentPlans = redeployDeploymentPlans.DeploymentPlans;

                var deploymentScheduler = new DeploymentScheduler();
                var componentGraph = deploymentScheduler.GetDeploymentComponentGraph(DeploymentPlans);
                Graph = componentGraph.ToBidirectionalGraph();
                Products = deploymentScheduler.GetDeploymentSchedule(componentGraph);
            }
            catch
            {
                DeploymentPlans = new List<DeploymentPlan>();
            }
            Loading = false;
        }

        public bool CanEnvironmentMirror
        {
            get
            {
                return !(string.IsNullOrEmpty(_octopusApiKey) 
                    || string.IsNullOrEmpty(_octopusApiKey) 
                    || string.IsNullOrEmpty(_selectedEnvironmentMirrorFromEnvironment) 
                    || string.IsNullOrEmpty(_selectedEnvironmentMirrorToEnvironment));
            }
        }

        public void EnvironmentMirror()
        {
            if (!CanEnvironmentMirror) return;
            Loading = true;
            try
            {
                var deploymentPlanner = new OctopusDeploymentPlanner(_octopusUrl, _octopusApiKey);
                var environmentMirrorDeploymentPlans = deploymentPlanner.GetEnvironmentMirrorDeploymentPlans(_selectedEnvironmentMirrorFromEnvironment, _selectedEnvironmentMirrorToEnvironment);

                DeploymentPlans = environmentMirrorDeploymentPlans.DeploymentPlans;

                var deploymentScheduler = new DeploymentScheduler();
                var componentGraph = deploymentScheduler.GetDeploymentComponentGraph(DeploymentPlans);
                Graph = componentGraph.ToBidirectionalGraph();
                Products = deploymentScheduler.GetDeploymentSchedule(componentGraph);
            }
            catch
            {
                DeploymentPlans = new List<DeploymentPlan>();
            }
            Loading = false;
        }
    }
}
