﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    // Interfaces to system nodes communication.

    public interface Broker
    {
        //TODO
    }

    public interface Subscriber
    {
        //TODO
    }

    public interface Publisher
    {
        //TODO
    }

    // Interfaces to test and control of system processes.

    public interface IGeneralControlServices
    {
        [OneWay]
        void Crash();
        void Freeze();
        void Unfreeze();
        void Status();
    }

    public interface IPublisherControlServices
    {
        void Publish(string topicName,int numberOfEvents,int interval);
    }

    public interface ISubscriberControlServices
    {
        void Subscribe(string topicName);
        void Unsubscribe(string topicName);
    }  

    public interface IPuppetMasterLog
    {
        void LogAction(string logMessage);
    }

    public interface IPuppetMasterLauncher
    {
        void LaunchProcess(string name,string args);
    }

}