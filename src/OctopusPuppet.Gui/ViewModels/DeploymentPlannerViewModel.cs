using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Win32;
using Newtonsoft.Json;
using OctopusPuppet.Deployer;
using OctopusPuppet.DeploymentPlanner;
using OctopusPuppet.Gui.Model;
using OctopusPuppet.OctopusProvider;
using OctopusPuppet.Scheduler;
using QuickGraph;
using Environment = OctopusPuppet.DeploymentPlanner.Environment;

namespace OctopusPuppet.Gui.ViewModels
{
    public class DeploymentPlannerViewModel : PropertyChangedBase, IProgress<ComponentVertexDeploymentProgress>
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

            _layoutAlgorithmTypes = new List<string>(new[]
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

            ApiSettings = new OctopusApiSettings
            (
                ConfigurationManager.AppSettings["OctopusUrl"],
                ConfigurationManager.AppSettings["OctopusApiKey"]
            );

            LoadDefaultComponentFilterJson();
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
                NotifyOfPropertyChange(() => CanDeploy);
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
                EnvironmentDeploymentResult = _environmentDeployment;
                NotifyOfPropertyChange(() => EnvironmentDeployment);
                NotifyOfPropertyChange(() => CanDeploy);
                NotifyOfPropertyChange(() => CanCancelDeploy);
            }
        }

        public bool CanDeploy
        {
            get { return EnvironmentDeployment != null && !IsLoadingData; }
        }

        public bool CanCancelDeploy
        {
            get { return EnvironmentDeployment != null && CancellationTokenSource != null; }
        }

        public async Task GetBranchesAndEnvironments()
        {

            if (!_apiSettings.IsEmpty)
            {
                IsLoadingData = true;
                try
                {
                    await Task.Factory.StartNew(() =>
                    {
                        var deploymentPlanner = new OctopusDeploymentPlanner(_apiSettings.Url, _apiSettings.ApiKey);

                        Branches = deploymentPlanner.GetBranches();
                        Environments = deploymentPlanner.GetEnvironments();
                    });
                }
                catch (Exception x)
                {
                    MessageBox.Show(x.Message, x.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                IsLoadingData = false;
            }
            else
            {
                Branches = new List<Branch>();
                Environments = new List<Environment>();
            }
        }

        private OctopusApiSettings _apiSettings;
        public OctopusApiSettings ApiSettings
        {
            get { return _apiSettings; }
            set
            {
                if (value == _apiSettings)
                    return;

                _apiSettings = value;

                GetBranchesAndEnvironments();

                NotifyOfPropertyChange(() => ApiSettings);

                NotifyOfPropertyChange(() => CanBranchDeployment);
                NotifyOfPropertyChange(() => CanRedeployment);
                NotifyOfPropertyChange(() => CanEnvironmentMirror);
                NotifyOfPropertyChange(() => CanGetBranchesAndEnvironments);
            }
        }

        public bool CanGetBranchesAndEnvironments => !_apiSettings.IsEmpty;

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

        private bool _doNotUseDifferentialDeploymentForBranchDeployment = false;
        public bool DoNotUseDifferentialDeploymentForBranchDeployment
        {
            get { return _doNotUseDifferentialDeploymentForBranchDeployment; }
            set
            {
                if (value == _doNotUseDifferentialDeploymentForBranchDeployment) return;
                _doNotUseDifferentialDeploymentForBranchDeployment = value;
                NotifyOfPropertyChange(() => DoNotUseDifferentialDeploymentForBranchDeployment);
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

        private bool _doNotUseDifferentialDeploymentForMirror = false;
        public bool DoNotUseDifferentialDeploymentForMirror
        {
            get { return _doNotUseDifferentialDeploymentForMirror; }
            set
            {
                if (value == _doNotUseDifferentialDeploymentForMirror) return;
                _doNotUseDifferentialDeploymentForMirror = value;
                NotifyOfPropertyChange(() => DoNotUseDifferentialDeploymentForMirror);
            }
        }

        private bool _skipMasterBranchForMirrorDeployment = false;
        public bool SkipMasterBranchForMirrorDeployment
        {
            get { return _skipMasterBranchForMirrorDeployment; }
            set
            {
                if (value == _skipMasterBranchForMirrorDeployment) return;
                _skipMasterBranchForMirrorDeployment = value;
                NotifyOfPropertyChange(() => SkipMasterBranchForMirrorDeployment);
            }
        }

        private bool _skipMasterBranchForBranchDeployment = false;
        public bool SkipMasterBranchForBranchDeployment
        {
            get { return _skipMasterBranchForBranchDeployment; }
            set
            {
                if (value == _skipMasterBranchForBranchDeployment) return;
                _skipMasterBranchForBranchDeployment = value;
                NotifyOfPropertyChange(() => SkipMasterBranchForBranchDeployment);
            }
        }

        private bool _componentFilterInclude = false;
        public bool ComponentFilterInclude
        {
            get { return _componentFilterInclude; }
            set
            {
                if (value == _componentFilterInclude) return;
                _componentFilterInclude = value;
                NotifyOfPropertyChange(() => ComponentFilterInclude);
            }
        }

        private ObservableCollection<StringWrapper> _componentFilterExpressions = new ObservableCollection<StringWrapper>();
        public ObservableCollection<StringWrapper> ComponentFilterExpressions
        {
            get { return _componentFilterExpressions; }
            set
            {
                if (value == _componentFilterExpressions) return;
                _componentFilterExpressions = value;
                NotifyOfPropertyChange(() => ComponentFilterExpressions);
            }
        }

        private string _componentFilterSaveFileName;
        public string ComponentFilterSaveFileName
        {
            get { return _componentFilterSaveFileName; }
            set
            {
                if (value == _componentFilterSaveFileName) return;
                _componentFilterSaveFileName = value;
                NotifyOfPropertyChange(() => ComponentFilterSaveFileName);
            }
        }

        public void SaveComponentFilterJson()
        {
            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = "json",
                Filter = "Text files (*.json)|*.json|All files (*.*)|*.*",
                FileName = ComponentFilterSaveFileName
            };
            if (saveFileDialog.ShowDialog() != true) return;

            var componentFilter = new ComponentFilter()
            {
                Include = ComponentFilterInclude,
                Expressions = ComponentFilterExpressions.Select(x => x.Text).ToList()
            };

            var json = JsonConvert.SerializeObject(componentFilter, Formatting.Indented);
            File.WriteAllText(saveFileDialog.FileName, json);
        }

        private void LoadDefaultComponentFilterJson()
        {
            if (File.Exists("filter.json"))
            {
                LoadComponentFilterJsonFromFile("filter.json");
            }
        }

        public void LoadComponentFilterJson()
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = "json",
                Filter = "Text files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() != true) return;

            LoadComponentFilterJsonFromFile(openFileDialog.FileName);
        }

        public void LoadComponentFilterJsonFromFile(string jsonFilePath)
        {
            var json = File.ReadAllText(jsonFilePath);
            var componentFilter = JsonConvert.DeserializeObject<ComponentFilter>(json);
            var expressions = componentFilter.Expressions
                .Select(x => new StringWrapper { Text = x })
                .ToList();
            ComponentFilterExpressions = new ObservableCollection<StringWrapper>(expressions);
            ComponentFilterInclude = componentFilter.Include;
        }

        public bool CanBranchDeployment
        {
            get
            {
                return !_apiSettings.IsEmpty &&
                       !(_selectedBranchDeploymentEnvironment == null
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
                    var deploymentPlanner = new OctopusDeploymentPlanner(_apiSettings.Url, _apiSettings.ApiKey);
                    var componentFilter = new ComponentFilter
                    {
                        Expressions = ComponentFilterExpressions.Select(x => x.Text).ToList(),
                        Include = ComponentFilterInclude
                    };

                    var branchDeploymentPlans = deploymentPlanner.GetBranchDeploymentPlans(_selectedBranchDeploymentEnvironment.Name, _selectedBranchDeploymentBranch.Name, _doNotUseDifferentialDeploymentForBranchDeployment, _skipMasterBranchForBranchDeployment, componentFilter);
                    EnvironmentDeploymentPlan = branchDeploymentPlans.EnvironmentDeploymentPlan;

                    var deploymentScheduler = new DeploymentScheduler();

                    var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(EnvironmentDeploymentPlan);
                    Graph = componentGraph.ToBidirectionalGraph();
                    EnvironmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);
                    EnvironmentDeploymentSaveFileName = "branch " + _selectedBranchDeploymentBranch.Name + " to " + _selectedBranchDeploymentEnvironment.Name + ".json";
                    EnvironmentToDeployTo = _selectedBranchDeploymentEnvironment;
                }
                catch
                {
                    EnvironmentDeploymentPlan = new EnvironmentDeploymentPlan(new List<ComponentDeploymentPlan>());
                    Graph = null;
                    EnvironmentDeployment = new EnvironmentDeployment(new List<ProductDeployment>());
                    EnvironmentDeploymentSaveFileName = string.Empty;
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
                return !_apiSettings.IsEmpty &&
                       _selectedRedeploymentEnvironment != null;
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
                    var deploymentPlanner = new OctopusDeploymentPlanner(_apiSettings.Url, _apiSettings.ApiKey);
                    var componentFilter = new ComponentFilter
                    {
                        Expressions = ComponentFilterExpressions.Select(x => x.Text).ToList(),
                        Include = ComponentFilterInclude
                    };
                    var redeployDeploymentPlans = deploymentPlanner.GetRedeployDeploymentPlans(_selectedRedeploymentEnvironment.Name, componentFilter);
                    EnvironmentDeploymentPlan = redeployDeploymentPlans.EnvironmentDeploymentPlan;

                    var deploymentScheduler = new DeploymentScheduler();
                    var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(EnvironmentDeploymentPlan);
                    Graph = componentGraph.ToBidirectionalGraph();
                    EnvironmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);
                    EnvironmentDeploymentSaveFileName = "redeploy " + _selectedRedeploymentEnvironment.Name + ".json";
                    EnvironmentToDeployTo = _selectedRedeploymentEnvironment;
                }
                catch
                {
                    EnvironmentDeploymentPlan = new EnvironmentDeploymentPlan(new List<ComponentDeploymentPlan>());
                    Graph = null;
                    EnvironmentDeployment = new EnvironmentDeployment(new List<ProductDeployment>());
                    EnvironmentDeploymentSaveFileName = string.Empty;
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
                return !_apiSettings.IsEmpty &&
                       !(_selectedEnvironmentMirrorFromEnvironment == null
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
                    var deploymentPlanner = new OctopusDeploymentPlanner(_apiSettings.Url, _apiSettings.ApiKey);
                    var componentFilter = new ComponentFilter
                    {
                        Expressions = ComponentFilterExpressions.Select(x => x.Text).ToList(),
                        Include = ComponentFilterInclude
                    };
                    var environmentMirrorDeploymentPlans = deploymentPlanner.GetEnvironmentMirrorDeploymentPlans(_selectedEnvironmentMirrorFromEnvironment.Name, _selectedEnvironmentMirrorToEnvironment.Name, _doNotUseDifferentialDeploymentForMirror, _skipMasterBranchForMirrorDeployment, componentFilter);

                    EnvironmentDeploymentPlan = environmentMirrorDeploymentPlans.EnvironmentDeploymentPlan;

                    var deploymentScheduler = new DeploymentScheduler();
                    var componentGraph = deploymentScheduler.GetComponentDeploymentGraph(EnvironmentDeploymentPlan);
                    Graph = componentGraph.ToBidirectionalGraph();
                    EnvironmentDeployment = deploymentScheduler.GetEnvironmentDeployment(componentGraph);
                    EnvironmentDeploymentSaveFileName = "mirror " + _selectedEnvironmentMirrorFromEnvironment.Name + " to " + _selectedEnvironmentMirrorToEnvironment.Name + ".json";
                    EnvironmentToDeployTo = _selectedEnvironmentMirrorToEnvironment;
                }
                catch
                {
                    EnvironmentDeploymentPlan = new EnvironmentDeploymentPlan(new List<ComponentDeploymentPlan>());
                    Graph = null;
                    EnvironmentDeployment = new EnvironmentDeployment(new List<ProductDeployment>());
                    EnvironmentDeploymentSaveFileName = string.Empty;
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

        private string _environmentDeploymentSaveFileName;
        public string EnvironmentDeploymentSaveFileName
        {
            get { return _environmentDeploymentSaveFileName; }
            set
            {
                if (value == _environmentDeploymentSaveFileName) return;
                _environmentDeploymentSaveFileName = value;
                NotifyOfPropertyChange(() => EnvironmentDeploymentSaveFileName);
            }
        }

        public void SaveEnvironmentDeploymentJson()
        {
            var saveFileDialog = new SaveFileDialog
            {
                DefaultExt = "json",
                Filter = "Text files (*.json)|*.json|All files (*.*)|*.*",
                FileName = EnvironmentDeploymentSaveFileName
            };
            if (saveFileDialog.ShowDialog() != true) return;
            var json = JsonConvert.SerializeObject(EnvironmentDeployment, Formatting.Indented);
            File.WriteAllText(saveFileDialog.FileName, json);
        }

        public void LoadEnvironmentDeploymentJson()
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

        private VariableAction _selectedSetVariablesTo = VariableAction.Update;
        public VariableAction SelectedSetVariablesTo
        {
            get { return _selectedSetVariablesTo; }
            set
            {
                if (value == _selectedSetVariablesTo) return;
                _selectedSetVariablesTo = value;
                NotifyOfPropertyChange(() => SelectedSetVariablesTo);
            }
        }

        public void SetAllVariablesTo()
        {
            if (EnvironmentDeployment == null)
            {
                return;
            }

            var componentsToDeploy = EnvironmentDeployment
                .ProductDeployments
                .SelectMany(x => x.DeploymentSteps)
                .SelectMany(x => x.ComponentDeployments)
                .Where(x => x.Vertex.VariableAction != SelectedSetVariablesTo)
                .ToList();

            foreach (var componentToDeploy in componentsToDeploy)
            {
                componentToDeploy.Vertex.VariableAction = SelectedSetVariablesTo;
            }

            if (componentsToDeploy.Any())
            {
                var temp = EnvironmentDeployment;
                EnvironmentDeployment = null;
                EnvironmentDeployment = temp;
            }
        }

        private PlanAction _selectedSetAllDeploymentsTo = PlanAction.Change;
        public PlanAction SelectedSetAllDeploymentsTo
        {
            get { return _selectedSetAllDeploymentsTo; }
            set
            {
                if (value == _selectedSetAllDeploymentsTo) return;
                _selectedSetAllDeploymentsTo = value;
                NotifyOfPropertyChange(() => SelectedSetAllDeploymentsTo);
            }
        }

        public void SetAllDeploymentsTo()
        {
            if (EnvironmentDeployment == null)
            {
                return;
            }

            var componentsToDeploy = EnvironmentDeployment
                .ProductDeployments
                .SelectMany(x => x.DeploymentSteps)
                .SelectMany(x => x.ComponentDeployments)
                .Where(x => x.Vertex.DeploymentAction != SelectedSetAllDeploymentsTo)
                .ToList();

            var deploymentPlans = EnvironmentDeploymentPlan.DeploymentPlans
                .Where(x => x.Action != SelectedSetAllDeploymentsTo)
                .ToList();

            foreach (var componentToDeploy in componentsToDeploy)
            {
                componentToDeploy.Vertex.DeploymentAction = SelectedSetAllDeploymentsTo;
            }

            foreach (var deploymentPlan in deploymentPlans)
            {
                deploymentPlan.Action = SelectedSetAllDeploymentsTo;
            }

            if (componentsToDeploy.Any())
            {
                var temp = EnvironmentDeployment;
                EnvironmentDeployment = null;
                EnvironmentDeployment = temp;
            }

            if (deploymentPlans.Any())
            {
                var temp = EnvironmentDeploymentPlan;
                EnvironmentDeploymentPlan = null;
                EnvironmentDeploymentPlan = temp;
            }
        }

        public void SkipAllPassedDeployments()
        {
            var successfullyDeployedComponents = EnvironmentDeploymentResult
                .ProductDeployments
                .SelectMany(x => x.DeploymentSteps)
                .SelectMany(x => x.ComponentDeployments)
                .Where(x => x.Status == ComponentVertexDeploymentStatus.Success && x.Vertex.DeploymentAction != PlanAction.Skip)
                .ToList();

            var componentsToDeploy = EnvironmentDeployment
                .ProductDeployments
                .SelectMany(x => x.DeploymentSteps)
                .SelectMany(x => x.ComponentDeployments)
                .Where(x => successfullyDeployedComponents.Any(y => y.Vertex.Equals(x.Vertex)))
                .ToList();

            var deploymentPlans = EnvironmentDeploymentPlan.DeploymentPlans
                .Where(x => successfullyDeployedComponents.Any(y => y.Vertex.Id == x.Id && y.Vertex.Name == x.Name))
                .ToList();

            foreach (var successfullyDeployedComponent in successfullyDeployedComponents)
            {
                successfullyDeployedComponent.Vertex.DeploymentAction = PlanAction.Skip;
            }

            foreach (var componentToDeploy in componentsToDeploy)
            {
                componentToDeploy.Vertex.DeploymentAction = PlanAction.Skip;
            }

            foreach (var deploymentPlan in deploymentPlans)
            {
                deploymentPlan.Action = PlanAction.Skip;
            }

            if (successfullyDeployedComponents.Any())
            {
                var temp = EnvironmentDeploymentResult;
                EnvironmentDeploymentResult = null;
                EnvironmentDeploymentResult = temp;
            }

            if (componentsToDeploy.Any())
            {
                var temp = EnvironmentDeployment;
                EnvironmentDeployment = null;
                EnvironmentDeployment = temp;
            }

            if (deploymentPlans.Any())
            {
                var temp = EnvironmentDeploymentPlan;
                EnvironmentDeploymentPlan = null;
                EnvironmentDeploymentPlan = temp;
            }
        }

        private int _maximumParallelDeployment = 4;
        public int MaximumParallelDeployment
        {
            get { return _maximumParallelDeployment; }
            set
            {
                if (value == _maximumParallelDeployment) return;
                _maximumParallelDeployment = value;
                NotifyOfPropertyChange(() => MaximumParallelDeployment);
            }
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

        private Model.EnvironmentDeploymentResult _environmentDeploymentResult;
        public Model.EnvironmentDeploymentResult EnvironmentDeploymentResult
        {
            get { return _environmentDeploymentResult; }
            private set
            {
                if (value == _environmentDeploymentResult) return;
                _environmentDeploymentResult = value;
                NotifyOfPropertyChange(() => EnvironmentDeploymentResult);
            }
        }

        public void Report(ComponentVertexDeploymentProgress value)
        {
            var componentDeploymentResult = EnvironmentDeploymentResult
                .ProductDeployments
                .SelectMany(x => x.DeploymentSteps)
                .SelectMany(x => x.ComponentDeployments)
                .FirstOrDefault(x => x.Vertex == value.Vertex);

            if (componentDeploymentResult == null)
            {
                return;
            }

            componentDeploymentResult.MaximumValue = value.MaximumValue;
            componentDeploymentResult.MinimumValue = value.MinimumValue;
            componentDeploymentResult.Value = value.Value;
            componentDeploymentResult.Text = value.Text;
            componentDeploymentResult.Status = value.Status;
        }

        private CancellationTokenSource _cancellationTokenSource;
        public CancellationTokenSource CancellationTokenSource
        {
            get { return _cancellationTokenSource; }
            set
            {
                if (value == _cancellationTokenSource) return;
                _cancellationTokenSource = value;
                NotifyOfPropertyChange(() => CancellationTokenSource);
                NotifyOfPropertyChange(() => CanCancelDeploy);
            }
        }

        public void CancelEnvironmentDeployment()
        {
            var cancellationTokenSource = CancellationTokenSource;
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }

        public void ExecuteEnvironmentDeployment()
        {
            IsLoadingData = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var deployers = new IComponentVertexDeployer[]
                    {
                        new OctopusComponentVertexVariableUpdater(_apiSettings),
                        new OctopusComponentVertexDeployer(_apiSettings, EnvironmentToDeployTo),
                    };

                    CancellationTokenSource = new CancellationTokenSource();
                    var deploymentExecutor = new DeploymentExecutor(deployers, EnvironmentDeployment, CancellationTokenSource.Token, new OctopusLogMessages(_apiSettings.Url), this, MaximumParallelDeployment);
                    var allDeploymentsSucceded = deploymentExecutor.Execute().ConfigureAwait(false).GetAwaiter().GetResult();

                    SkipAllPassedDeployments();
                }
                catch
                {
                }
            }).ContinueWith(task =>
            {
                CancellationTokenSource = null;
                IsLoadingData = false;
            });
        }
    }
}
