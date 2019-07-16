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
        List<IMyCargoContainer> UnassignedContainers = new List<IMyCargoContainer>();
        List<IMyCargoContainer> ComponentContainers = new List<IMyCargoContainer>();
        List<IMyCargoContainer> OreContainers = new List<IMyCargoContainer>();
        List<IMyCargoContainer> IngotContainers = new List<IMyCargoContainer>();
        List<IMyCargoContainer> ToolContainers = new List<IMyCargoContainer>();

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }


        public void Main(string argument, UpdateType updateSource)
        {
            UpdateContainers();
            SortStuff();
        }


        public void UpdateContainers()
        {
            List<IMyCargoContainer> AllContainers = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(AllContainers);

            UnassignedContainers.Clear();
            ComponentContainers.Clear();
            OreContainers.Clear();
            IngotContainers.Clear();
            ToolContainers.Clear();


            for (int i = 0; i < AllContainers.Count; i++)
            {
                switch (AllContainers[i].CustomData.ToLower())
                {
                    case "component":
                        ComponentContainers.Add(AllContainers[i]);
                        break;

                    case "ore":
                        OreContainers.Add(AllContainers[i]);
                        break;

                    case "ingot":
                        IngotContainers.Add(AllContainers[i]);
                        break;

                    case "tool":
                        ToolContainers.Add(AllContainers[i]);
                        break;

                    default:
                        UnassignedContainers.Add(AllContainers[i]);
                        break;
                }
            }
        }


        public void SortStuff()
        {
            MyInventoryItem Item;
            for(int i = 0; i < UnassignedContainers.Count; i++)
            {
                for(int x = UnassignedContainers[i].GetInventory(0).ItemCount-1; x >= 0; x--)
                {
                    Item = UnassignedContainers[i].GetInventory(0).GetItemAt(x).Value;



                    if (Item.Type.GetItemInfo().IsComponent)
                    {
                        for(int y = 0; y < ComponentContainers.Count; y++)
                        {
                            if((ComponentContainers[y].GetInventory(0).MaxVolume - ComponentContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                            {
                                UnassignedContainers[i].GetInventory(0).TransferItemTo(ComponentContainers[y].GetInventory(0), Item);
                                break;
                            }
                            else
                            {
                                UnassignedContainers[i].GetInventory(0).TransferItemTo(ComponentContainers[y].GetInventory(0), Item, (ComponentContainers[y].GetInventory(0).MaxVolume - ComponentContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                            }
                        }
                    }




                    if (Item.Type.GetItemInfo().IsOre)
                    {
                        for (int y = 0; y < OreContainers.Count; y++)
                        {
                            if ((OreContainers[y].GetInventory(0).MaxVolume - OreContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                            {
                                UnassignedContainers[i].GetInventory(0).TransferItemTo(OreContainers[y].GetInventory(0), Item);
                                break;
                            }
                            else
                            {
                                UnassignedContainers[i].GetInventory(0).TransferItemTo(OreContainers[y].GetInventory(0), Item, (OreContainers[y].GetInventory(0).MaxVolume - OreContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                            }
                        }
                    }
                    if (Item.Type.GetItemInfo().IsIngot)
                    {
                        for (int y = 0; y < IngotContainers.Count; y++)
                        {
                            if ((IngotContainers[y].GetInventory(0).MaxVolume - IngotContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                            {
                                UnassignedContainers[i].GetInventory(0).TransferItemTo(IngotContainers[y].GetInventory(0), Item);
                                break;
                            }
                            else
                            {
                                UnassignedContainers[i].GetInventory(0).TransferItemTo(IngotContainers[y].GetInventory(0), Item, (IngotContainers[y].GetInventory(0).MaxVolume - IngotContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1/ Item.Type.GetItemInfo().Volume));
                            }
                        }
                    }
                    if (Item.Type.GetItemInfo().IsTool)
                    {
                        for (int y = 0; y < ToolContainers.Count; y++)
                        {
                            if ((ToolContainers[y].GetInventory(0).MaxVolume - ToolContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                            {
                                UnassignedContainers[i].GetInventory(0).TransferItemTo(ToolContainers[y].GetInventory(0), Item);
                                break;
                            }
                            else
                            {
                                UnassignedContainers[i].GetInventory(0).TransferItemTo(ToolContainers[y].GetInventory(0), Item, (ToolContainers[y].GetInventory(0).MaxVolume - ToolContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                            }
                        }
                    }
                }
            }
        }






    }
}
