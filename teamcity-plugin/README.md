Octopus Puppet Teamcity Plugin
==============================

# What

Provides a way run Octopus Puppet from Teamcity so that you can orchestrate differential deployments in Octopus. See [https://github.com/Aqovia/OctopusPuppet](https://github.com/Aqovia/OctopusPuppet) for information about Octopus Puppet.

## Branch Deployment

Will deploy all components that have SemVer pre-release that matches given branch. The latest component will be selected. If the target environment already has the exact component it will be skipped.

All components that do not have a matching SemVer pre-release will then match no SemVer pre-release (i.e. master). The latest component will be selected. If the target environment already has the component it will be skipped.

_This is often used for testing a feature branch. Features can span multiple repositories which means multiple components._

## Mirror Environment Deployment

Will deploy all components from source environment to target environment. The exact component is matched. If the target environment already has the exact component it will be skipped.

_This is often used for promoting a product from one environment to another or when developers want to debug a product on their own machines._

## Redeployment

Will redeploy all components. The exact component is matched. 

_This is often used for redeploying when settings/feature toggles have changed._

# Why

Often you want to orchastrate the deployment of environments during CI. The most common case is doing a branch deployment to run integration tests against it. You might also want to deploy master branch environment for overnight tests (soak/performance/huge regression test suites).

# How

Teamcity plugin the will expose the command line version of Octopus Puppet via the Teamcity GUI