using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Win32;
using Newtonsoft.Json;
using OctopusPuppet.Deployer;
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
            _branchDeploymentBranches = new List<Branch>();
            _selectedBranchDeploymentBranch = null;

            _branchDeploymentEnvironments = new List<Environment>();
            _selectedBranchDeploymentEnvironment = null;

            _redeploymentEnvironments = new List<Environment>();
            _selectedRedeploymentEnvironment = null;

            _environmentMirrorFromEnvironments = new List<Environment>();
            _selectedEnvironmentMirrorFromEnvironment = null;

            _environmentMirrorToEnvironments = new List<Environment>();
            _selectedEnvironmentMirrorToEnvironment = null;

            _layoutAlgorithmTypes = new List<string>(new []
            {
                "Tree",
                "Circular",
                "FR",
                "BoundedFR",
                "KK",
                "ISOM",
                "LinLog",
                "EfficientSugiyama",
                "Sugiyama",
                "CompoundFDP"
            });

            _selectedLayoutAlgorithmType = "EfficientSugiyama";

            OctopusApiKey = ConfigurationManager.AppSettings["OctopusApiKey"];
            OctopusUrl = ConfigurationManager.AppSettings["OctopusUrl"];
        }

        private bool _isLoadingData;
        public bool IsLoadingData
        {
            get { return _isLoadingData; }
            set
            {
                if (value == _isLoadingData) return;
                _isLoadingData = value;
                NotifyOfPropertyChange(() => IsLoadingData);
            }
        }

        private List<Branch> _branches;
        private List<Branch> Branches
        {
            get { return _branches; }
            set
            {
                if (value == _branches) return;
                _branches = value;
                BranchDeploymentBranches = value;
            }
        }

        private List<Environment> _environments;
        private List<Environment> Environments
        {
            get { return _environments; }
            set
            {
                if (value == _environments) return;
                _environments = value;

                BranchDeploymentEnvironments = value;
                RedeploymentEnvironments = value;
                EnvironmentMirrorFromEnvironments = value;
                EnvironmentMirrorToEnvironments = value;
            }
        }

        private EnvironmentDeploymentPlan _environmentDeploymentPlan;
        private EnvironmentDeploymentPlan EnvironmentDeploymentPlan
        {
            get { return _environmentDeploymentPlan; }
            set
            {
                if (value == _environmentDeploymentPlan) return;
                _environmentDeploymentPlan = value;
                NotifyOfPropertyChange(() => EnvironmentDeploymentPlan);
            }
        }

        private IBidirectionalGraph<ComponentDeploymentVertex, ComponentDeploymentEdge> _graph;
        public IBidirectionalGraph<ComponentDeploymentVertex, ComponentDeploymentEdge> Graph
        {
            get { return _graph; }
            private set
            {
                if (value == _graph) return;
                _graph = value;
                NotifyOfPropertyChange(() => Graph);
            }
        }

        private EnvironmentDeployment _environmentDeployment;
        public EnvironmentDeployment EnvironmentDeployment
        {
            get { return _environmentDeployment; }
            private set
            {
                if (value == _environmentDeployment) return;
                _environmentDeployment = value;
                NotifyOfPropertyChange(() => EnvironmentDeployment);
            }
        }

        private void GetBranchesAndEnvironments()
        {
            if (!string.IsNullOrEmpty(_octopusUrl) && !string.IsNullOrEmpty(_octopusApiKey))
            {
                IsLoadingData = true;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var deploymentPlanner = new OctopusDeploymentPlanner(_octopusUrl, _octopusApiKey);

                        Branches = deploymentPlanner.GetBranches();
                        Environments = deploymentPlanner.GetEnvironments();
                    }
                    catch
                    {
                        Branches = new List<Branch>();
                        Environments = new List<Environment>();                  
                    }
                }).ContinueWith(task => 
                {
                    IsLoadingData = false;
                });
            }
            else
            {
                Branches = new List<Branch>();
                Environments = new List<Environment>();
            }
        }

        private string _octopusApiKey;
        public string OctopusApiKey
        {
            get { return _octopusApiKey; }
            set
            {
                if (value == _octopusApiKey) return;
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
                if (value == _octopusUrl) return;
                _octopusUrl = value;

                GetBranchesAndEnvironments();

                NotifyOfPropertyChange(() => OctopusUrl);

                NotifyOfPropertyChange(() => CanBranchDeployment);
                NotifyOfPropertyChange(() => CanRedeployment);
                NotifyOfPropertyChange(() => CanEnvironmentMirror);
            }
        }

        private List<Branch> _branchDeploymentBranches;
        public List<Branch> BranchDeploymentBranches
        {
            get { return _branchDeploymentBranches; }
            private set
            {
                if (value == _branchDeploymentBranches) return;
                _branchDeploymentBranches = value;
                NotifyOfPropertyChange(() => BranchDeploymentBranches);
            }
        }

        private Branch _selectedBranchDeploymentBranch;
        public Branch SelectedBranchDeploymentBranch
        {
            get { return _selectedBranchDeploymentBranch; }
            set
            {
                if (value == _selectedBranchDeploymentBranch) return;
                _selectedBranchDeploymentBranch = value;
                NotifyOfPropertyChange(() => SelectedBranchDeploymentBranch);

                NotifyOfPropertyChange(() => CanBranchDeployment);
            }
        }

        private List<Environment> _branchDeploymentEnvironments;
        public List<Environment> BranchDeploymentEnvironments
        {
            get { return _branchDeploymentEnvironments; }
            private set
            {
                if (value == _branchDeploymentEnvironments) return;
                _branchDeploymentEnvironments = value;
                NotifyOfPropertyChange(() => BranchDeploymentEnvironments);
            }
        }

        private Environment _selectedBranchDeploymentEnvironment;
        public Environment SelectedBranchDeploymentEnvironment
        {
            get { return _selectedBranchDeploymentEnvironment; }
            set
            {
                if (value == _selectedBranchDeploymentEnvironment) return;
                _selectedBranchDeploymentEnvironment = value;
                NotifyOfPropertyChange(() => SelectedBranchDeploymentEnvironment);

                NotifyOfPropertyChange(() => CanBranchDeployment);
            }
        }

        private List<Environment> _redeploymentEnvironments;
        public List<Environment> RedeploymentEnvironments
        {
            get { return _redeploymentEnvironments; }
            private set
            {
                if (value == _redeploymentEnvironments) return;
                _redeploymentEnvironments = value;
                NotifyOfPropertyChange(() => RedeploymentEnvironments);
            }
        }

        private Environment _selectedRedeploymentEnvironment;
        public Environment SelectedRedeploymentEnvironment
        {
            get { return _selectedRedeploymentEnvironment; }
            set
            {
                if (value == _selectedRedeploymentEnvironment) return;
                _selectedRedeploymentEnvironment = value;
                NotifyOfPropertyChange(() => SelectedRedeploymentEnvironment);

                NotifyOfPropertyChange(() => CanRedeployment);
            }
        }

        private List<Environment> _environmentMirrorFromEnvironments;
        public List<Environment> EnvironmentMirrorFromEnvironments
        {
            get { return _environmentMirrorFromEnvironments; }
            private set
            {
                if (value == _environmentMirrorFromEnvironments) return;
                _environmentMirrorFromEnvironments = value;
                NotifyOfPropertyChange(() => EnvironmentMirrorFromEnvironments);
            }
        }

        private Environment _selectedEnvironmentMirrorFromEnvironment;
        public Environment SelectedEnvironmentMirrorFromEnvironment
        {
            get { return _selectedEnvironmentMirrorFromEnvironment; }
            set
            {
                if (value == _selectedEnvironmentMirrorFromEnvironment) return;
                _selectedEnvironmentMirrorFromEnvironment = value;
                NotifyOfPropertyChange(() => SelectedEnvironmentMirrorFromEnvironment);

                NotifyOfPropertyChange(() => CanEnvironmentMirror);
            }
        }

        private List<Environment> _environmentMirrorToEnvironments;
        public List<Environment> EnvironmentMirrorToEnvironments
        {
            get { return _environmentMirrorToEnvironments; }
            private set
            {
                if (value == _environmentMirrorToEnvironments) return;
                _environmentMirrorToEnvironments = value;
                NotifyOfPropertyChange(() => EnvironmentMirrorToEnvironments);
            }
        }

        private Environment _selectedEnvironmentMirrorToEnvironment;
        public Environment SelectedEnvironmentMirrorToEnvironment
        {
            get { return _selectedEnvironmentMirrorToEnvironment; }
            set
            {
                if (value == _selectedEnvironmentMirrorToEnvironment) return;
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
                    || _selectedBranchDeploymentEnvironment == null 
                    || _selectedBranchDeploymentBranch == null);
            }
        }

        public void BranchDeployment()
        {
            if (!CanBranchDeployment) return;

            IsLoadingData = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var deploymentPlanner = new OctopusDeploymentPlanner(_octopusUrl, _octopusApiKey);
                    var branchDeploymentPlans = deploymentPlanner.GetBranchDeploymentPlans(_selectedBranchDeploymentEnvironment.Id, _selectedBranchDeploymentBranch.Id);
                    EnvironmentDeploymentPlan = branchDeploymentPlans.EnvironmentDeploymentPlan;

                    var deploymentScheduler = new DeploymentScheduler();

                    var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(EnvironmentDeploymentPlan);
                    Graph = componentGraph.ToBidirectionalGraph();
                    EnvironmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);
                    SaveFileName = "branch " + _selectedBranchDeploymentBranch + " to " + _selectedBranchDeploymentEnvironment + ".json";
                    EnvironmentToDeployTo = _selectedBranchDeploymentEnvironment;
                }
                catch
                {
                    EnvironmentDeploymentPlan = new EnvironmentDeploymentPlan(new List<ComponentDeploymentPlan>());
                    Graph = null;
                    EnvironmentDeployment = new EnvironmentDeployment(new List<ProductDeployment>());
                    SaveFileName = string.Empty;
                    EnvironmentToDeployTo = null;
                }
            }).ContinueWith(task =>
            {
                IsLoadingData = false;
            });
        }

        public bool CanRedeployment
        {
            get
            {
                return !(string.IsNullOrEmpty(_octopusApiKey) 
                    || string.IsNullOrEmpty(_octopusApiKey) 
                    || _selectedRedeploymentEnvironment == null);
            }
        }

        public void Redeployment()
        {
            if (!CanRedeployment) return;

            IsLoadingData = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var deploymentPlanner = new OctopusDeploymentPlanner(_octopusUrl, _octopusApiKey);
                    var redeployDeploymentPlans = deploymentPlanner.GetRedeployDeploymentPlans(_selectedRedeploymentEnvironment.Id);
                    EnvironmentDeploymentPlan = redeployDeploymentPlans.EnvironmentDeploymentPlan;

                    var deploymentScheduler = new DeploymentScheduler();
                    var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(EnvironmentDeploymentPlan);
                    Graph = componentGraph.ToBidirectionalGraph();
                    EnvironmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);
                    SaveFileName = "redeploy " + _selectedRedeploymentEnvironment + ".json";
                    EnvironmentToDeployTo = _selectedRedeploymentEnvironment;
                }
                catch
                {
                    EnvironmentDeploymentPlan = new EnvironmentDeploymentPlan(new List<ComponentDeploymentPlan>());
                    Graph = null;
                    EnvironmentDeployment = new EnvironmentDeployment(new List<ProductDeployment>());
                    SaveFileName = string.Empty;
                    EnvironmentToDeployTo = null;
                }
            }).ContinueWith(task =>
            {
                IsLoadingData = false;
            });
        }

        public bool CanEnvironmentMirror
        {
            get
            {
                return !(string.IsNullOrEmpty(_octopusApiKey) 
                    || string.IsNullOrEmpty(_octopusApiKey) 
                    || _selectedEnvironmentMirrorFromEnvironment == null
                    || _selectedEnvironmentMirrorToEnvironment == null);
            }
        }

        public void EnvironmentMirror()
        {
            if (!CanEnvironmentMirror) return;

            IsLoadingData = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var deploymentPlanner = new OctopusDeploymentPlanner(_octopusUrl, _octopusApiKey);
                    var environmentMirrorDeploymentPlans = deploymentPlanner.GetEnvironmentMirrorDeploymentPlans(_selectedEnvironmentMirrorFromEnvironment.Id, _selectedEnvironmentMirrorToEnvironment.Id);

                    EnvironmentDeploymentPlan = environmentMirrorDeploymentPlans.EnvironmentDeploymentPlan;

                    var deploymentScheduler = new DeploymentScheduler();
                    var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(EnvironmentDeploymentPlan);
                    Graph = componentGraph.ToBidirectionalGraph();
                    EnvironmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);
                    SaveFileName = "mirror " + _selectedEnvironmentMirrorFromEnvironment + " to " + _selectedEnvironmentMirrorToEnvironment + ".json";
                    EnvironmentToDeployTo = _selectedEnvironmentMirrorToEnvironment;
                }
                catch
                {
                    EnvironmentDeploymentPlan = new EnvironmentDeploymentPlan(new List<ComponentDeploymentPlan>());
                    Graph = null;
                    EnvironmentDeployment = new EnvironmentDeployment(new List<ProductDeployment>());
                    SaveFileName = string.Empty;
                    EnvironmentToDeployTo = null;
                }
            }).ContinueWith(task =>
            {
                IsLoadingData = false;
            });
        }

        private List<string> _layoutAlgorithmTypes;
        public List<string> LayoutAlgorithmTypes
        {
            get { return _layoutAlgorithmTypes; }
            set
            {
                if (value == _layoutAlgorithmTypes) return;
                _layoutAlgorithmTypes = value;
                NotifyOfPropertyChange(() => LayoutAlgorithmTypes);
            }
        }

        private string _selectedLayoutAlgorithmType;
        public string SelectedLayoutAlgorithmType
        {
            get { return _selectedLayoutAlgorithmType; }
            set
            {
                if (value == _selectedLayoutAlgorithmType) return;
                _selectedLayoutAlgorithmType = value;
                NotifyOfPropertyChange(() => SelectedLayoutAlgorithmType);
            }
        }

        private string _saveFileName;
        public string SaveFileName
        {
            get { return _saveFileName; }
            set
            {
                if (value == _saveFileName) return;
                _saveFileName = value;
                NotifyOfPropertyChange(() => SaveFileName);
            }
        }

        public void SaveJson()
        {
            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = "json",
                Filter =  "Text files (*.json)|*.json|All files (*.*)|*.*",
                FileName = SaveFileName
            };
            if (saveFileDialog.ShowDialog() != true) return;
            var json = JsonConvert.SerializeObject(EnvironmentDeployment, Formatting.Indented);
            File.WriteAllText(saveFileDialog.FileName, json);
        }

        public void LoadJson()
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = "json",
                Filter = "Text files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;

            var json = File.ReadAllText(openFileDialog.FileName);
            var environmentDeployment = JsonConvert.DeserializeObject<EnvironmentDeployment>(json);
            EnvironmentDeployment = environmentDeployment;
        }

        private Environment _environmentToDeployTo;
        public Environment EnvironmentToDeployTo
        {
            get { return _environmentToDeployTo; }
            set
            {
                if (value == _environmentToDeployTo) return;
                _environmentToDeployTo = value;
                NotifyOfPropertyChange(() => EnvironmentToDeployTo);
            }
        }

        public void ExecuteEnvironmentDeployment()
        {
            IsLoadingData = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var componentVertexDeployer = new OctopusComponentVertexDeployer(_octopusUrl, _octopusApiKey, EnvironmentToDeployTo);
                    var cancellationTokenSource = new CancellationTokenSource();
                    var deploymentExecutor = new DeploymentExecutor(componentVertexDeployer, EnvironmentDeployment, cancellationTokenSource.Token);
                    deploymentExecutor.Execute().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch
                {

                }
            }).ContinueWith(task =>
            {
                IsLoadingData = false;
            });
        }
    }
}
