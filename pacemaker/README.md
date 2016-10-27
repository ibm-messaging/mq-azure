# IBM(R) MQ Queue Manager and Pacemaker

This sample shows how to use Pacemaker to manage two instances of an IBM MQ Queue Manager.

A total of three Virtual Machines are created and an asymmetric Pacemaker cluster is created out of them, with two of the three Virtual Machines running IBM MQ and the third acting just as a quorum node to avoid the possibility of a Split-Brain scenario.

The three Virtual Machines are placed into an Availability Set to minimise the chances that two of them fail or are updated at the same time.

The two MQ Virtual Machines are given Public IP Addresses so they can be contacted directly over the Internet.

The names of the Virtual Machines are based on the value of the clusterName property:
<clusterName>-MQ1
<clusterName>-MQ2
<clusterName>-Pacemaker

# Deploy Template

There are two steps to deploying the template:

1. create a resource group
2. create a deployment of the template in the resource group

## Create a Resource group

To create a resource group, run a command such as `azure group create <Resource Group Name> <Region>`

## Deploy the Template

To deploy the template to the resource group, run a command such as `azure group deployment create -f mq-pacemaker-deploy.json -e mq-pacemaker-parameters.json JC-MQ-Pacemaker-RG deployment1`