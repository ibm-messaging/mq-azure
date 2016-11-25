# IBM(R) MQ Queue Manager and Pacemaker

Microsoft Azure offers [File storage](https://azure.microsoft.com/en-gb/services/storage/files/) which provides file shares that use the SMB 3.0 protocol.

Such a file share can be used by an MQ multi-instance queue manager running on Windows but it does not meet the requirements of a multi-instance queue
manager running on Linux.

This sample shows how to use Pacemaker to manage two instances of a non-multi-instance queue manager that are using a file share. Pacemaker ensures that
only one instance of the queue manager is running at once and therefore the additional requirements for a multi-instance queue manager do not apply.

A total of three Virtual Machines are created and configured as a Pacemaker cluster:

1. a node named MQ1 which can run the queue manager
2. a node named MQ2 which can run the queue manager
3. a node named Pacemaker which cannot run the queue manager, its purpose is to ensure quorum for the Pacemaker cluster

The three Virtual Machines are placed into an Availability Set to minimise the chances that two of them fail or are updated at the same time.

The two MQ Virtual Machines are given Public IP Addresses so they can be contacted directly over the Internet. The Pacemaker virtual machine is
not given a public IP address so has to be managed from one of the other virtual machines.

The full names of the Virtual Machines are based on the value of the clusterName property:
<clusterName>-MQ1
<clusterName>-MQ2
<clusterName>-Pacemaker

Multiple Subnets and Network Security Groups are used to isolate the different logical networks, using the ability to associate multiple network
interfaces with a single virtual machine. The Subnets are:

1. Primary - this is the main subnet the Pacemaker nodes use to monitor each other
2. Alternate - this is an alternate subnet for the Pacemaker nodes to use in the Primary network fails
3. Data - this is the subnet that is used to access the shared data

# Contents of this sample

This sample contains the following files:

| File | Contents |
| --- | --- |
| configureForMQ | Sets various linux configuration parameters for MQ |
| configureHA_QM | Creates the Pacemaker configuration for an HA queue manager |
| configurePacemaker | configures a node to be part of the Pacemaker cluster |
| installMQ | installs MQ Advanced for Developers |
| installMQ_HA_QM | installs the Pacemaker Resource Agent script for MQ and the other necessary scripts |
| installPacemaker | installs Pacemaker from the standard Ubuntu repository |
| mountData | set up the mount of the file share |
| MQ_HA_QM | the Resource Agent script |
| MQ_HA_QM_monitor | the script to monitor a local HA queue manager |
| MQ_HA_QM_start | the script to start a local HA queue manager |
| MQ_HA_QM_stop | the script to stop a local HA queue manager |
| mq-pacemaker-deploy.json | the Azure Resource Manager deployment template |
| mq-pacemaker-parameters.json | the configurable parameters for the deployment template |
| README.md | this file |

# NTP

For production use I would strongly suggest that you configure NTP on each
of the virtual machines to keep their clocks in sync.

# Azure Resource Manager Template

## Understanding the Template

This section explains the contents of the template.

### parameters

The parameters section declares the parameters that are defined in the mq-pacemaker-parameters.json file.

### variables

The variables section declares identifiers for values that are used more than once in the template.

### resources

The bulk of the template consists of resource definitions. Each resource is described in this section.

#### Virtual Network

The first resource defined is a virtual network. The address range for the virtual network is 192.168.0.0/16 and it is divided into four subnets:

| subnet | address range | usage
| --- | --- | --- |
| primary | 192.168.1.0/24 | this subnet is the primary subnet used for Pacemaker communication between the three nodes |
| alternate | 192.168.2.0/24 | this subnet is the alternate subnet used for Pacemaker communication between the three nodes |
| data | 192.168.3.0/24 | this subnet is used by the MQ nodes to access the shared data
| public | 192.168.4.0/24 | this subnet is used to give access from the Internet so you can ssh to the virtual machines |

#### Primary Network Security Group

This Network Security Group allows Pacemaker to communicate over UDP using ports 5404 and 5405 which correspond to the configuration of the primary ring of the cluster.

It also allows ssh access from any other virtual machine in the same subnet so that you can ssh from one of the MQ nodes to the Pacemaker node to install Pacemaker etc.

#### Alternate Network Security Group

This Network Security Group allows Pacemaker to communicate over UDP using ports 5406 and 5406 which correspond to the configuration of the redundant ring of the cluster.

It also blocks access to the Internet.

#### Data Network Security Group

This Network Security Group allows the MQ nodes to access any address using TCP. The access could be restricted to the specific set of Azure public subnets for the
region where the template is deployed but that generally requires a large number of rules as most regions have a large number of subnets.

#### Public Network Security Group

This Network Security Group allows SSH access to the MQ nodes from the Internet and is what allows you to scp files to the MQ instances and ssh to them in order to configure them.

#### Pacemaker Virtual Machine

This virtual machine is of a smaller size than the MQ virtual machines and only has two network interfaces:

1. one for the Pacemaker primary subnet
2. one for the Pacemaker alternate subnet

#### MQ1 Virtual Machine

This virtual machine is of a larger size than the Pacemaker virtual machine and has four network interfaces:

1. one for the Pacemaker primary subnet
2. one for the Pacemaker alternate subnet
3. one for the Data subnet
4. one for the Public subnet

#### MQ2 Virtual Machine

This virtual machine is of a larger size than the Pacemaker virtual machine and has four network interfaces:

1. one for the Pacemaker primary subnet
2. one for the Pacemaker alternate subnet
3. one for the Data subnet
4. one for the Public subnet

#### Storage account

A new storage account is created for the disks for the virtual machines and the Azure Files share for the shared MQ data.

The template is set up to use Locally Replicated Storage (LRS).

#### Pacemaker VM Primary NIC

This is the NIC for the primary interface of the Pacemaker VM and has the static IP address 192.168.1.11

#### Pacemaker VM Alternate NIC

This is the NIC for the alternate interface of the Pacemaker VM and has the static IP address 192.168.2.11

#### MQ1 VM Primary NIC

This is the NIC for the primary interface of the MQ1 VM and has the static IP address 192.168.1.12

#### MQ1 VM Alternate NIC

This is the NIC for the alternate interface of the MQ1 VM and has the static IP address 192.168.2.12

#### MQ1 VM Data NIC

This is the NIC for the data interface of the MQ1 VM and has the static IP address 192.168.3.12

#### MQ1 VM Public NIC

This is the NIC for the public interface of the MQ1 VM and has the static IP address 192.168.4.12

#### MQ2 VM Primary NIC

This is the NIC for the primary interface of the MQ2 VM and has the static IP address 192.168.1.13

#### MQ2 VM Alternate NIC

This is the NIC for the alternate interface of the MQ2 VM and has the static IP address 192.168.2.13

#### MQ2 VM Data NIC

This is the NIC for the data interface of the MQ2 VM and has the static IP address 192.168.3.13

#### MQ2 VM Public NIC

This is the NIC for the public interface of the MQ2 VM and has the static IP address 192.168.4.13

#### MQ1 Public IP Addresses

This declares a public IP address for the MQ1 VM which is bound to the DNS name derived from the variable `dnsLabelPrefix1`

#### MQ2 Public IP Addresses

This declares a public IP address for the MQ2 VM which is bound to the DNS name derived from the variable `dnsLabelPrefix2`

### Outputs

The outputs are:

| name | contents |
| --- | --- |
| sshCommand1 | the ssh command to use to connect to the MQ1 virtual machine |
| sshCommand2 | the ssh command to use to connect to the MQ2 virtual machine |
| storageName | the generated name for the storage account used with the deployment of the template |
| storageKey | the value of the first key for the storage account |

## Deploying the Template

There are two steps to deploying the template:

1. create a resource group
2. create a deployment of the template in the resource group

Creating a resource group just for this sample makes it easy to delete all
the resources for the sample without disturbing any other resources
you have on Azure.

I use the [Azure CLI](https://azure.microsoft.com/en-gb/documentation/articles/xplat-cli-install/)
so the sample commands I will give will be for that.

## Create a Resource group

To create a resource group, I ran the command:

```
azure group create JC-MQ-Pacemaker-RG northeurope
```

## Deploy the Template

Before you can deploy the template you will have to edit the `mq-pacemaker-parameters.json` file and
specify the values you want.

To deploy the template to my resource group, I ran the command:

```
azure group deployment create -f mq-pacemaker-deploy.json -e mq-pacemaker-parameters.json JC-MQ-Pacemaker-RG deployment1
```

The Outputs of this command were:

| Name | Value |
| --- | --- |
| sshCommand1 | ssh colgrave@jcmqcluster1mq1.northeurope.cloudapp.azure.com |
| sshCommand2 | ssh colgrave@jcmqcluster1mq2.northeurope.cloudapp.azure.com |
| storageName | mq7i332jfsh7wstorage |
| storageKey | TDJUv6oK52TP8pvbACoBwp2A+4h4wDCi9eswol5W8MQ2PS+FWnHkwgzsHRTd1tYf9T9bzL70E1led+UF1ivbLQ== |

## Fault and Update Zones

To maintain availability of a queue manager it is important to ensure that the effect of either maintenance or a fault is limited to only one instance of the queue manager.
In Azure this is managed by fault domains and update domains as part of an availability set. You can find more information [here](https://docs.microsoft.com/en-us/azure/virtual-machines/virtual-machines-windows-manage-availability)

As described above, the template includes the definition of an availability set so each of the three virtual machines created should be in its own fault domain and
its own update domain.

To see that the three virtual machines have been put into different fault zones and update zones by the deployment, in the Microsoft Azure Portal
go to `All resources` and find the Availability set. Click on the Availability set name and in the right-hand window you should see a list of the virtual machines in the Availability set
and the fault domain and update domain for each.

When I did this I saw the following:

| NAME | FAULT DOMAIN | UPDATE DOMAIN |
| --- | --- | --- |
| mqcluster1-Pacemaker | 2 | 2 |
| mqcluster1mq1 | 1 | 1 |
| mqcluster1mq2 | 0 | 0 |

You can see that each virtual machine is in a unique fault domain and a unique update domain.

# Create Azure File share

While you have the outputs from deploying the template you can create the share that will be used for the MQ data with the command:

```
AZURE_STORAGE_ACCOUNT=<storageName> AZURE_STORAGE_ACCESS_KEY=<storageKey> azure storage share create mqshare
```

The name `mqshare` is assumed later so should not be changed.

# Copy the scripts to the MQ nodes

The following scripts need to be copied to each of the two MQ nodes:

1. configureForMQ
2. configureHA_QM
3. configurePacemaker
4. installMQ
5. installMQ_HA_QM
6. installPacemaker
7. mountData
8. MQ_HA_QM
9. MQ_HA_QM_monitor
10. MQ_HA_QM_start
11. MQ_HA_QM_stop

# Copy the MQ Advanced for Developers package to the MQ nodes

Download IBM MQ Advanced for Developers 9.0 [here](http://www14.software.ibm.com/cgi-bin/weblap/lap.pl?popup=Y&li_formnum=L-APIG-A4FHQ9&accepted_url=http://public.dhe.ibm.com/ibmdl/export/pub/software/websphere/messaging/mqadv/mqadv_dev90_linux_x86-64.tar.gz)

Once you have the `mqadv_dev90_linux_x86-64.tar.gz` file, copy it to each of the MQ nodes.

# Copy scripts to the Pacemaker node

The following scripts need to be copied from one of the MQ nodes to the Pacemaker node:

1. configurePacemaker
2. installMQ_HA_QM
3. installPacemaker
4. MQ_HA_QM
5. MQ_HA_QM_monitor
6. MQ_HA_QM_start
7. MQ_HA_QM_stop

# Install MQ

For each MQ node do the following:

```
ssh to the node
sudo ./configureForMQ
exit
ssh to the node
sudo ./installMQ <username>
exit
```

where `<username>` is the account you use to ssh to the node.

# Install and Configure Pacemaker

On each of the three nodes, run:
```
sudo ./installPacemaker
sudo ./installMQ_HA_QM
```

On the Pacemaker node run `sudo ./configurePacemaker <cluster name> Pacemaker`

On the MQ1 node run `sudo ./configurePacemaker <cluster name> MQ1`

On the MQ2 node run `sudo ./configurePacemaker <cluster name> MQ2`

# Mount the Azure File share

On each of the MQ nodes run `sudo ./mountData <storageName> <storageKey>`

# Create Queue Manager

On MQ1, run the following:

```
crtmqm -ld /mnt/mqshare/logs -md /mnt/mqshare/data -p 1417 HAQM1
dspmqinf -o command HAQM1
```

On MQ2, run the addmqinf command produced by the dspmqinf command.

# Configure Pacemaker for Queue Manager

On MQ1, run `sudo ./configureHA_QM HAQM1`

To check the configuration, run `sudo crm_mon -1`

When I did that I got:

```
Last updated: Wed Nov  9 15:38:18 2016
Last change: Wed Nov  9 15:37:48 2016 via crm_shadow on MQ1
Stack: corosync
Current DC: Pacemaker (1) - partition with quorum
Version: 1.1.10-42f2063
3 Nodes configured
1 Resources configured


Online: [ MQ1 MQ2 Pacemaker ]

 HAQM1	(ocf::IBM:MQ_HA_QM):	Started MQ1
```

This shows that the HAQM1 queue manager should be running on the MQ1 node. I checked that by running dspmq on MQ1 which produced:

```
QMNAME(HAQM1)                                             STATUS(Running)
```

# Testing Failover

To test failover, do the following on MQ1:

```
runmqsc HAQM1
DEFINE QLOCAL(QUEUE1) DEFPSIST(YES)
end
cd /opt/mqm/samp/bin
./amqsput QUEUE1 HAQM1
Message1
Message2
Message3

sudo crm node standby MQ1
```

The final command tells Pacemaker to stop running any resources on the node MQ1. As we have prevented the HAQM1 resource from running on the Pacemaker node the only thing
that Pacemaker can do is start HAQM1 on the MQ2 node.

Running `sudo crm_mon -1` should now produce:

```
Last updated: Wed Nov  9 15:41:04 2016
Last change: Wed Nov  9 15:40:48 2016 via crm_attribute on MQ1
Stack: corosync
Current DC: Pacemaker (1) - partition with quorum
Version: 1.1.10-42f2063
3 Nodes configured
1 Resources configured


Node MQ1 (2): standby
Online: [ MQ2 Pacemaker ]

 HAQM1	(ocf::IBM:MQ_HA_QM):	Started MQ2
```

This shows that the node MQ1 is in standby mode and that HAQM1 is now running on the node MQ2.

## Restoring MQ1

To restore the node MQ1 to active participation in the cluster, run the command `sudo crm node online MQ1`

Checking the cluster status now with `sudo crm_mon -1` should produce something like:

```
Last updated: Wed Nov  9 15:49:05 2016
Last change: Wed Nov  9 15:46:22 2016 via crm_attribute on Pacemaker
Stack: corosync
Current DC: Pacemaker (1) - partition with quorum
Version: 1.1.10-42f2063
3 Nodes configured
1 Resources configured


Online: [ MQ1 MQ2 Pacemaker ]

 HAQM1	(ocf::IBM:MQ_HA_QM):	Started MQ2
```

Note that even though MQ1 is now online again, HAQM1 is still running on MQ2. This is because we only have one resource running so Pacemaker will leave it where it is rather than
move it. If we had two resources running, Pacemaker would move one of them to MQ1 to balance the load. It is possible to influence this by setting a stickiness value, either as a
default for all resources or for a particular resource. There are other approaches that could be used but it is simplest to let Pacemaker balance the placement of the resources in
the cluster, subject to the constraints placed on the resources.

# Summary

This sample has shown hot to use Pacemaker to manage an IBM MQ queue manager that can run on two nodes in a three-node Pacemaker cluster, using shared data which in this case is
an Azure File share.