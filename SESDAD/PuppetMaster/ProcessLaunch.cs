﻿using CommonTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class ProcessLauncher
    {
        public static string DEFAULT_ROUTING_POLICY = "flooding";
        public static string DEFAULT_LOG_LEVEL = "FIFO";
        public static string DEFAULT_ORDERING_POLICY = "light";

        private string routingPolicy = DEFAULT_ROUTING_POLICY;

        private string logLevel = DEFAULT_LOG_LEVEL;

        private string orderingPolicy = DEFAULT_ORDERING_POLICY;

        private List<LaunchNode> launchNodes;

        public string RoutingPolicy
        {
            get
            {
                return routingPolicy;
            }
            set
            {
                routingPolicy = value;
            }
        }

        public string LogLevel
        {
            get
            {
                return logLevel;
            }
            set
            {
                logLevel = value;
            }
        }

        public string OrderingPolicy
        {
            get
            {
                return orderingPolicy;
            }
            set
            {
                orderingPolicy = value;
            }
        }

        public ProcessLauncher()
        {
            launchNodes = new List<LaunchNode>();
        }

        public void AddNode(LaunchNode node)
        {
            launchNodes.Add(node);
        }

        public void LaunchAllProcesses(ManageSites sites)
        {
            foreach(LaunchNode node in launchNodes)
            {
                node.Launch(sites,OrderingPolicy,RoutingPolicy,LogLevel);
            }
        }
    }

    public abstract class LaunchNode
    {
        private string name;

        private string port;

        private string ip;

        private string site;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Ip
        {
            get
            {
                return ip;
            }
        }

        public string Port
        {
            get
            {
                return port;
            }
        }

        public string Site
        {
            get
            {
                return site;
            }
        }

        public LaunchNode(string name, string ip, string port, string site)
        {
            this.name = name;
            this.ip = ip;
            this.port = port;
            this.site = site;
        }

        protected void LaunchProcess(string processType,string args)
        {
            if (Ip.Equals("localhost") || Ip.Equals("127.0.0.1"))
            {
                if (CommonUtil.IsLinux)
                    Process.Start("mono",
                        string.Join(" ", CommonUtil.PROJECT_ROOT + name +
                        CommonUtil.EXE_PATH + name + ".exe", args));
                else
                    Process.Start(CommonUtil.PROJECT_ROOT + processType +
                        CommonUtil.EXE_PATH + processType, args);
            }
            else
            {
                IPuppetMasterLauncher launcher = Activator.GetObject(
                    typeof(IPuppetMasterLauncher), CommonUtil.MakeUrl("tcp",
                    Ip, CommonUtil.PUPPET_MASTER_NAME, CommonUtil.PUPPET_MASTER_NAME))
                    as IPuppetMasterLauncher;
                launcher.LaunchProcess(processType, args);
            }
        }

        public abstract void Launch(ManageSites sites,string orderingPolicy
            ,string routingPolicy,string logPolicy);

    }

    public abstract class LaunchEndNode : LaunchNode
    {
        public LaunchEndNode(string name,string ip, string port, string site) 
            : base(name,ip,port,site)
        { }

        public override void Launch(ManageSites sites, string orderingPolicy,
            string routingPolicy, string logPolicy)
        {
            string args = string.Join(" ", Port, Name,
                orderingPolicy,routingPolicy,logPolicy,
                CommonUtil.MakeUrl("tcp",CommonUtil.GetLocalIPAddress()
                , CommonUtil.PUPPET_MASTER_PORT.ToString(), CommonUtil.PUPPET_MASTER_NAME),
                sites.GetSiteByName(Site).GetBrokersUrl());
            string processType = this.GetType().Name.Substring(6);
            LaunchProcess(processType, args);
        }
    }

    public class LaunchBroker : LaunchNode
    {
        public LaunchBroker(string name,string ip, string port, string site) 
            : base(name,ip,port,site)
        {   }

        public override void Launch(ManageSites sites, string orderingPolicy
            , string routingPolicy, string logPolicy)
        {
            string temp = string.Join(" ", Port, Name,
                orderingPolicy,routingPolicy,logPolicy);
            string parent = "NoParent";
            string children = sites.GetSiteByName(Site).GetChildUrl();
            if (sites.GetSiteByName(Site).Parent != null)
                parent = sites.GetSiteByName(Site).Parent.GetBrokersUrl();
            string args = string.Join(" ",temp
                , CommonUtil.MakeUrl("tcp", CommonUtil.GetLocalIPAddress()
                , CommonUtil.PUPPET_MASTER_PORT.ToString(), 
                CommonUtil.PUPPET_MASTER_NAME), parent, children);
            LaunchProcess(this.GetType().Name.Substring(6), args);
        }
    }

    public class LaunchPublisher : LaunchEndNode
    {
        public LaunchPublisher(string name,string ip, string port, string site) 
            : base(name,ip,port,site){ }
    }

    public class LaunchSubscriber : LaunchEndNode
    {
        public LaunchSubscriber(string name,string ip, string port, string site) 
            : base(name,ip,port,site){ }
    }
   
}