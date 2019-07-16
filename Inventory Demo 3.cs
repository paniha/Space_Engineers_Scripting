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
        IMyCargoContainer CompContainer;
        IMyAssembler Assembler;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            Assembler = GridTerminalSystem.GetBlockWithName("Assembler") as IMyAssembler;
            CompContainer = GridTerminalSystem.GetBlockWithName("Small Cargo Container") as IMyCargoContainer;
        }
        float ComputerCount, ConstructionCount, SteelPlateCount, MotorCount;
        double compAmount = 30;
        double constructAmount = 50;
        double steelAmount = 100;
        double motorAmount = 40;

        public void Main(string argument, UpdateType updateSource)
        {
            UpdateInv();

            if (ComputerCount < compAmount) { Assembler.AddQueueItem(MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ComputerComponent"), compAmount - ComputerCount); }
            if (ConstructionCount < constructAmount) { Assembler.AddQueueItem(MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ConstructionComponent"), constructAmount - ConstructionCount); }
            if (SteelPlateCount < steelAmount) { Assembler.AddQueueItem(MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SteelPlate"), steelAmount - SteelPlateCount);}
            if (MotorCount < motorAmount) { Assembler.AddQueueItem(MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MotorComponent"), motorAmount - MotorCount); }

            if (Assembler.GetInventory(1).IsItemAt(0))
            {
                Assembler.GetInventory(1).TransferItemTo(CompContainer.GetInventory(0), Assembler.GetInventory(1).GetItemAt(0).Value);
            }


        }

        public void UpdateInv()
        {
            List<MyProductionItem> Queue = new List<MyProductionItem>();
            ComputerCount = 0; ConstructionCount = 0; SteelPlateCount = 0;  MotorCount = 0; //Zero out amounts

            ComputerCount += (float)CompContainer.GetInventory(0).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "Computer"));
            ConstructionCount += (float)CompContainer.GetInventory(0).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "Construction"));
            SteelPlateCount += (float)CompContainer.GetInventory(0).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "SteelPlate"));
            MotorCount += (float)CompContainer.GetInventory(0).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "Motor"));

            ComputerCount += (float)Assembler.GetInventory(1).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "Computer"));
            ConstructionCount += (float)Assembler.GetInventory(1).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "Construction"));
            SteelPlateCount += (float)Assembler.GetInventory(1).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "SteelPlate"));
            MotorCount += (float)Assembler.GetInventory(1).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "Motor"));



            if (!Assembler.IsQueueEmpty)//If the assembler has items in queue
                {
                    Assembler.GetQueue(Queue);//stuff assembler queue into queue list
                    for (int x = 0; x < Queue.Count; x++)//for every item in queue
                    {
                        switch (Queue[x].BlueprintId.SubtypeName)//add the items to our amounts
                        {
                            case "ComputerComponent":
                                ComputerCount += (float)Queue[x].Amount;
                                break;

                            case "ConstructionComponent":
                                ConstructionCount += (float)Queue[x].Amount;
                                break;

                            case "SteelPlate":
                                SteelPlateCount += (float)Queue[x].Amount;
                                break;

                            case "MotorComponent":
                                MotorCount += (float)Queue[x].Amount;
                                break;
                        }
                    }
            }
        }



    }
}
