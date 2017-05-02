using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using OctopusPuppet.Deployer;
using OctopusPuppet.Scheduler;

namespace OctopusPuppet.Gui.Model
{
    public class ComponentDeploymentResult : PropertyChangedBase
    {
        public static implicit operator ComponentDeploymentResult(ComponentDeployment componentDeployment)
        {
            if (componentDeployment == null)
            {
                return null;
            }

            return new ComponentDeploymentResult
            {
                Vertex = componentDeployment.Vertex,
                Edges = componentDeployment.Edges
            };
        }

        public ComponentDeploymentVertex Vertex { get; set; }
        public List<ComponentDeploymentEdge> Edges { get; set; }

        public Visibility IsProgressBarVisible
        {
            get
            {
                if (Status == ComponentVertexDeploymentStatus.NotStarted)
                {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }

        private ComponentVertexDeploymentStatus _status;
        public ComponentVertexDeploymentStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value == _status) return;
                _status = value;
                NotifyOfPropertyChange(() => Status);
                NotifyOfPropertyChange(() => IsProgressBarVisible);
            }
        }

        private long _minimumValue;
        public long MinimumValue
        {
            get { return _minimumValue; }
            set
            {
                if (value == _minimumValue) return;
                _minimumValue = value;
                NotifyOfPropertyChange(() => MinimumValue);                
            }
        }

        private long _maximumValue;
        public long MaximumValue
        {
            get { return _maximumValue; }
            set
            {
                if (value == _maximumValue) return;
                _maximumValue = value;
                NotifyOfPropertyChange(() => MaximumValue);
            }
        }

        private long _value;
        public long Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                _value = value;
                NotifyOfPropertyChange(() => Value);
            }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                if (value == _text) return;
                _text = value;
                NotifyOfPropertyChange(() => Text);
            }
        }
    }
}