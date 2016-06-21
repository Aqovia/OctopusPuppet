Octopus Puppet
==============

**Generate deployment plan**
![Generate deployment plan screen](/docs/img/Generate_deployment_plan.png?raw=true "Generate deployment plan screen")

**Visualize and configure deployment plan**
![Deployment plan screen](/docs/img/Deployment_Plan.png?raw=true "Deployment plan screen")

**Deploy**
![Deploy screen](/docs/img/Deploy.png?raw=true "Deploy screen")

# What

Provides a way to visualize, orchestrate differential deployments in Octopus. 

## Differential Branch Deployment

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

When running micro-services architecture one of the downsides is that there is plenty of stuff to deploy. To reduce this tax you need to ensure as much as possible is automated. Octopus goes a long way to making a great experince, but there are a few short comings.

Deployment Taxes:
* It is often difficult to see how components are dependent on each other. The ideal situation is a graphical graph which shows component deployment dependencies.
* Having one giant octopus deployment project is not feasable either. Often we don't want to deploy everything as this takes ages. The ideal situation is a differential deployment. It will only deploy what has not already been deployed.
* There appears to be no way of calling other deployments from a deployment in Octopus. This is due to Octopus will block the deployment. So you can't call the Octopus API and wait until the deployment is finished as Octopus will only start running the task once the wait is finished. The ideal situation is a way to call deployments from deployments or have something that can orchestrate this.

# How

## Component dependencies

There is no way to show component deployment dependencies in Octopus. To fake this, add a variable called "ComponentDependencies" to a component. This variable is a json string array of all components that this component is dependent on. 

```
['Payment Service', 'Fraud Service']
```

This is susceptible to people refactoring component names or deleting dependent components, but OctopusPuppet will detect when it is dependent on a component that does not exist. It will skip these dependencies and will show in red in the deployment planner.

## Scope of dependencies to deploy

There may be a number of unrelated products configured for an environment in Octopus. So when you want to do a branch deployment you may not care about all the other products. In order to support this, there is a concept of a component filter. This is simple a list of regular expressions to either include or exclude components based on their name. 

If you exclude a component that another component is dependent on it will filter the ComponentDependencies variable as well. 

This allows you do something like this:
* Create a filter for environment deployments
** Step 1 - Deploy database schema
** Step 2 - Deploy website
* Create a filter for environment refreshes
** Step 1 - Restore database
** Step 2 - Deploy database schema
** Step 3 - Deploy website

## Order to deploy components in
```
A -> B -> C
       -> D -> E
```

* Step 1
** Deploy A
* Step 2
** Deploy B
* Step 3
** Deploy C
** Deploy D
* Step 4
** Deploy E

The deployment scheduler will calculate the optimum deployment schedule, by parallezing deployments when it can. It can also deal with cyclical component dependencies.

Theory:
[https://en.wikipedia.org/wiki/Directed_graph](https://en.wikipedia.org/wiki/Directed_graph) on how to model component dependencies
[https://en.wikipedia.org/wiki/Directed_acyclic_graph](https://en.wikipedia.org/wiki/Directed_acyclic_graph) on how our deployment steps should be 

## Cyclical component dependencies

```
A -> B -> C -> A
       -> D -> E
```

* Step 1
** Deploy A
** Deploy B
** Deploy C
* Step 2
** Deploy D
* Step 3
** Deploy E

When the deployment scheduler detect cyclical component dependencies it will plan to deploy each of these components in parallel as there is no clear order to deploy them in. So it will deploy these components in parallel.

## Components selected for a product

```
A -> B -> C
       -> D -> E
Z->Y
```

* Product 1
** A
** B
** C
** D
** E
* Product 2
** Z
** Y

The deployment scheduler will detect product by grouping components that are strongly connected. A product is something that can be deployed without effecting another product, so its useful to calculate what a product it.

Theory:
[https://en.wikipedia.org/wiki/Strongly_connected_component](https://en.wikipedia.org/wiki/Strongly_connected_component) on how to work out product.

## Dealing with databases when switching branches

The are two approaches to database deployments. Change sets vs declarative schema. If you use declarative schema (e.g. Redgate) then you can switch between branches without too much trouble. If you use change sets (e.g. Liquibase) to deploy database changes then you are going to have to restore the database, then apply change sets when switching branches.

# Glossary of terms

Component = Octopus Project
Latest component = The component with the highest version number 
Exact component = The component with a specific version number
Product = Group of related components


