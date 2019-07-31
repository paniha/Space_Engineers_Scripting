using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        IMyPistonBase up1;
        IMyPistonBase up2;

        IMyPistonBase out1;
        IMyPistonBase out2;

        IMyRadioAntenna antenna;

        MyIGCMessage packet = new MyIGCMessage();
        List<IMyBroadcastListener> listeners = new List<IMyBroadcastListener>();

        string Channel = "door";


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            antenna = GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;

            up1 = GridTerminalSystem.GetBlockWithName("up1") as IMyPistonBase;
            up2 = GridTerminalSystem.GetBlockWithName("up2") as IMyPistonBase;

            out1 = GridTerminalSystem.GetBlockWithName("out1") as IMyPistonBase;
            out2 = GridTerminalSystem.GetBlockWithName("out2") as IMyPistonBase;

            antenna.AttachedProgrammableBlock = Me.EntityId;

            IGC.RegisterBroadcastListener(Channel);
            IGC.GetBroadcastListeners(listeners);

            listeners[0].SetMessageCallback("door");
        }

        bool open = false;
        string data;

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument.Equals("door"))
            {
                packet = listeners[0].AcceptMessage();
                data = packet.Data.ToString();
                if (data.Equals("password"))
                {
                    open = !open;
                } 
            }
            if (argument.Equals("doorLocal"))
            {
                open = !open;
            }

            if (open)
            {
                out1.Extend();
                out2.Extend();
                if (out1.CurrentPosition == 5)
                {
                    up1.Extend();
                    up2.Extend();
                }
            }
            else
            {
                up1.Retract();
                up2.Retract();
                if(up1.CurrentPosition == 0)
                {
                    out1.Retract();
                    out2.Retract();
                }
            }
        }

    }
}