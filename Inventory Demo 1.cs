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
        IMyCargoContainer SAContainer, SBContainer, PBAContainer, PBBContainer;
        IMyTextPanel SALCD, SBLCD, PBALCD, PBBLCD;

        IMyConveyorSorter Sorter;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            Sorter = GridTerminalSystem.GetBlockWithName("Sorter") as IMyConveyorSorter;

            SAContainer = GridTerminalSystem.GetBlockWithName("Sorter A") as IMyCargoContainer;
            SBContainer = GridTerminalSystem.GetBlockWithName("Sorter B") as IMyCargoContainer;
            PBAContainer = GridTerminalSystem.GetBlockWithName("PB A") as IMyCargoContainer;
            PBBContainer = GridTerminalSystem.GetBlockWithName("PB B") as IMyCargoContainer;

            SALCD = GridTerminalSystem.GetBlockWithName("Sorter A LCD") as IMyTextPanel;
            SBLCD = GridTerminalSystem.GetBlockWithName("Sorter B LCD") as IMyTextPanel;
            PBALCD = GridTerminalSystem.GetBlockWithName("PB A LCD") as IMyTextPanel;
            PBBLCD = GridTerminalSystem.GetBlockWithName("PB B LCD") as IMyTextPanel;

        }


        public void Main(string argument, UpdateType updateSource)
        {
            if (argument.ToLower().Equals("transfer"))
            {
                Sorter.Enabled = true;
                for(int i = PBAContainer.GetInventory(0).ItemCount-1; i >= 0 ; i--)
                {
                    if(PBBContainer.GetInventory(0).CanItemsBeAdded(PBAContainer.GetInventory(0).GetItemAt(i).Value.Amount, PBAContainer.GetInventory(0).GetItemAt(i).Value.Type))
                    {
                        PBAContainer.GetInventory(0).TransferItemTo(PBBContainer.GetInventory(0), PBAContainer.GetInventory(0).GetItemAt(i).Value);
                    }
                }
            }


            PrintInventory(SAContainer, SALCD);
            PrintInventory(SBContainer, SBLCD);
            PrintInventory(PBAContainer, PBALCD);
            PrintInventory(PBBContainer, PBBLCD);

        }

        public void PrintInventory(IMyCargoContainer Container, IMyTextPanel LCD)
        {
            string output = "";

            for (int i = 0; i < Container.GetInventory(0).ItemCount; i++)
            {
                output += Container.GetInventory(0).GetItemAt(i).Value.Amount + "  " + Container.GetInventory(0).GetItemAt(i).Value.Type.SubtypeId + "\n";
            }
            LCD.ContentType = ContentType.TEXT_AND_IMAGE;
            LCD.WriteText(output);
        }


    }
}
