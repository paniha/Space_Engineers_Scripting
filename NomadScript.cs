        List<IMyCargoContainer> UnassignedContainers = new List<IMyCargoContainer>();
        List<IMyCargoContainer> ComponentContainers = new List<IMyCargoContainer>();
        List<IMyCargoContainer> OreContainers = new List<IMyCargoContainer>();
        List<IMyCargoContainer> IngotContainers = new List<IMyCargoContainer>();
        List<IMyCargoContainer> ToolContainers = new List<IMyCargoContainer>();

        List<IMyAssembler> Assemblers = new List<IMyAssembler>();
        List<IMyRefinery> Refineries = new List<IMyRefinery>();
        List<IMyShipDrill> OreDrills = new List<IMyShipDrill>();
        List<IMyShipDrill> IceDrills = new List<IMyShipDrill>();
        List<IMyPistonBase> OrePistons = new List<IMyPistonBase>();
        List<IMyThrust> VertHydros = new List<IMyThrust>();
        List<IMyThrust> AllHydros = new List<IMyThrust>();
        List<IMyThrust> AllIons = new List<IMyThrust>();
        List<IMyGyro> Gyros = new List<IMyGyro>();
        List<IMyInteriorLight> Lights = new List<IMyInteriorLight>();
        List<IMyLandingGear> Gear = new List<IMyLandingGear>();

        IMyCameraBlock Camera;
        IMySensorBlock Sensor;
        IMyDoor InnerDoor;
        IMyDoor OuterDoor;
        IMyAirVent AirlockVent;
        IMyCockpit Cockpit;
        IMyPistonBase IcePiston;
        IMyMotorAdvancedStator IceRotor;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            GridTerminalSystem.GetBlocksOfType<IMyGyro>(Gyros);

            for (int i = Gyros.Count - 1; i >= 0; i--)
            {
                if (!Gyros[i].IsSameConstructAs(Me))
                {
                    Gyros.RemoveAt(i);
                }
            }
            Echo("Got Gyros");
            List<IMySoundBlock> Sounders = new List<IMySoundBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(Lights);
            GridTerminalSystem.GetBlocksOfType<IMyLandingGear>(Gear);
            Sensor = GridTerminalSystem.GetBlockWithName("Airlock Sensor") as IMySensorBlock;
            InnerDoor = GridTerminalSystem.GetBlockWithName("Inner Door") as IMyDoor;
            OuterDoor = GridTerminalSystem.GetBlockWithName("Outer Door") as IMyDoor;
            AirlockVent = GridTerminalSystem.GetBlockWithName("Airlock Vent") as IMyAirVent;
            Cockpit = GridTerminalSystem.GetBlockWithName("Flight Seat") as IMyCockpit;
            Camera = GridTerminalSystem.GetBlockWithName("Camera") as IMyCameraBlock;
            IcePiston = GridTerminalSystem.GetBlockWithName("Ice Piston") as IMyPistonBase;
            IceRotor = GridTerminalSystem.GetBlockWithName("Ice Rotor") as IMyMotorAdvancedStator;

            //There really must be an easier way of getting group blocks....
            IMyBlockGroup tempGroup = GridTerminalSystem.GetBlockGroupWithName("Hydro Vert");
            List<IMyTerminalBlock> tempList = new List<IMyTerminalBlock>();
            tempGroup.GetBlocks(tempList);
            for (int i = 0; i < tempList.Count; i++)
            {
                VertHydros.Add((IMyThrust)tempList[i]);
            }

            tempGroup = GridTerminalSystem.GetBlockGroupWithName("All Hydros");
            tempList.Clear();
            tempGroup.GetBlocks(tempList);
            for (int i = 0; i < tempList.Count; i++)
            {
                AllHydros.Add((IMyThrust)tempList[i]);
            }
            tempGroup = GridTerminalSystem.GetBlockGroupWithName("All Ions");
            tempList.Clear();
            tempGroup.GetBlocks(tempList);
            for (int i = 0; i < tempList.Count; i++)
            {
                AllIons.Add((IMyThrust)tempList[i]);
            }
            tempGroup = GridTerminalSystem.GetBlockGroupWithName("Ore Drills");
            tempList.Clear();
            tempGroup.GetBlocks(tempList);
            for (int i = 0; i < tempList.Count; i++)
            {
                OreDrills.Add((IMyShipDrill)tempList[i]);
            }
            tempGroup = GridTerminalSystem.GetBlockGroupWithName("Ice Drills");
            tempList.Clear();
            tempGroup.GetBlocks(tempList);
            for (int i = 0; i < tempList.Count; i++)
            {
                IceDrills.Add((IMyShipDrill)tempList[i]);
            }

            tempGroup = GridTerminalSystem.GetBlockGroupWithName("Ore Pistons");
            tempList.Clear();
            tempGroup.GetBlocks(tempList);
            for (int i = 0; i < tempList.Count; i++)
            {
                OrePistons.Add((IMyPistonBase)tempList[i]);
            }



        }
        double Pitch, Roll, OldPitch, OldRoll, DeltaPitch, DeltaRoll, Altitude;
        bool drilling = false;
        bool iceDrilling = false;
        bool LiftOff = false;
        bool PlanetFallEnable = false;
        int AirlockCounter = 0;
        float D = 0, OldVel = 0;
        float IcePistDist = 0;

        public void Main(string argument, UpdateType updateSource)
        {
            switch (argument.ToLower())
            {
                case "ore drill":
                    drilling = !drilling;
                    break;

                case "ice drill":
                    iceDrilling = !iceDrilling;
                    break;

                case "liftoff":
                    LiftOff = !LiftOff;
                    break;

                case "planetfall":
                    PlanetFallEnable = !PlanetFallEnable;
                    break;

            }

            if (drilling)
            {
                for (int i = 0; i < OreDrills.Count; i++)
                {
                    OreDrills[i].Enabled = true;
                }
                for (int i = 0; i < OrePistons.Count; i++)
                {
                    OrePistons[i].Velocity = 0.1f;
                }
            }
            else
            {
                for (int i = 0; i < OreDrills.Count; i++)
                {
                    OreDrills[i].Enabled = false;
                }
                for (int i = 0; i < OrePistons.Count; i++)
                {
                    OrePistons[i].Velocity = -1;
                }
            }

            AirlockManager();

            UpdateContainers();
            SortStuff();

            if (Cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out Altitude))
            {
                if (!PlanetFallEnable && !LiftOff && !OnGround())
                {
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        Lights[i].Color = Color.Red;
                        Lights[i].BlinkIntervalSeconds = 2;
                        Lights[i].BlinkLength = 20;
                    }

                    for (int i = 0; i < Gyros.Count; i++) { Gyros[i].GyroOverride = false; }
                }
                else
                {
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        Lights[i].Color = Color.Yellow;
                        Lights[i].BlinkIntervalSeconds = 0;
                    }
                    if (PlanetFallEnable)
                    {
                        PlanetFall();
                    }
                    if (LiftOff)
                    {
                        for (int i = 0; i < AllHydros.Count; i++)
                        {
                            AllHydros[i].Enabled = true;
                        }
                        MaintainVertical(true);
                        ThrustManager(true, 99, 0.01f);
                    }

                }
            }
            else
            {
                LiftOff = false;
                MaintainVertical(false);
                for (int i = 0; i < AllIons.Count; i++)
                {
                    AllIons[i].Enabled = true;
                }
                for (int i = 0; i < AllHydros.Count; i++)
                {
                    AllHydros[i].Enabled = false;
                }
                for (int i = 0; i < Lights.Count; i++)
                {
                    Lights[i].Color = Color.White;
                    Lights[i].BlinkIntervalSeconds = 0;
                }
                for (int i = 0; i < Gyros.Count; i++) { Gyros[i].GyroOverride = false; }
            }

            if (iceDrilling)
            {
                for (int i = 0; i < IceDrills.Count; i++)
                {
                    IceDrills[i].Enabled = true;
                }
                IcePiston.MaxLimit = IcePistDist;
                IcePiston.Velocity = 0.5f;
                if (IceRotor.TargetVelocityRPM < 0)
                {
                    if (IceRotor.Angle * (180 / Math.PI) <= -15)
                    {
                        IcePistDist += 1.5f;
                        IceRotor.TargetVelocityRPM = 0.75f;
                    }
                }
                else
                {
                    if (IceRotor.Angle * (180 / Math.PI) >= 160)
                    {
                        IcePistDist += 1.5f;
                        IceRotor.TargetVelocityRPM = -0.75f;
                    }
                }

            }
            else
            {
                for (int i = 0; i < IceDrills.Count; i++)
                {
                    IceDrills[i].Enabled = false;
                }
                IcePiston.Velocity = -1;
                IceRotor.TargetVelocityRPM = -1.5f;
                IcePistDist = 0;
            }

        }


        public void UpdateContainers()
        {
            List<IMyCargoContainer> AllContainers = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(AllContainers);
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Assemblers);
            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Refineries);
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
            for (int i = 0; i < Assemblers.Count; i++)
            {
                for (int x = Assemblers[i].GetInventory(1).ItemCount - 1; x >= 0; x--)
                {
                    Item = Assemblers[i].GetInventory(1).GetItemAt(x).Value;
                    for (int y = 0; y < ComponentContainers.Count; y++)
                    {
                        if ((ComponentContainers[y].GetInventory(0).MaxVolume - ComponentContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                        {
                            Assemblers[i].GetInventory(1).TransferItemTo(ComponentContainers[y].GetInventory(0), Item);
                            break;
                        }
                        else
                        {
                            Assemblers[i].GetInventory(1).TransferItemTo(ComponentContainers[y].GetInventory(0), Item, (ComponentContainers[y].GetInventory(0).MaxVolume - ComponentContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                        }
                    }
                }
            }

            for (int i = 0; i < Refineries.Count; i++)
            {
                for (int x = Refineries[i].GetInventory(1).ItemCount - 1; x >= 0; x--)
                {
                    Item = Refineries[i].GetInventory(1).GetItemAt(x).Value;
                    for (int y = 0; y < IngotContainers.Count; y++)
                    {
                        if ((IngotContainers[y].GetInventory(0).MaxVolume - IngotContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                        {
                            Refineries[i].GetInventory(1).TransferItemTo(IngotContainers[y].GetInventory(0), Item);
                            break;
                        }
                        else
                        {
                            Refineries[i].GetInventory(1).TransferItemTo(IngotContainers[y].GetInventory(0), Item, (IngotContainers[y].GetInventory(0).MaxVolume - IngotContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                        }
                    }
                }
            }

            SortContainer(UnassignedContainers);
            SortContainer(OreContainers);
            SortContainer(IngotContainers);
            SortContainer(ComponentContainers);
            SortContainer(ToolContainers);


        }

        public void SortContainer(List<IMyCargoContainer> Containers)
        {
            MyInventoryItem Item;
            for (int i = 0; i < Containers.Count; i++)
            {
                for (int x = Containers[i].GetInventory(0).ItemCount - 1; x >= 0; x--)
                {
                    Item = Containers[i].GetInventory(0).GetItemAt(x).Value;

                    if (Item.Type.GetItemInfo().IsComponent && !Containers.Equals(ComponentContainers))
                    {
                        for (int y = 0; y < ComponentContainers.Count; y++)
                        {
                            if ((ComponentContainers[y].GetInventory(0).MaxVolume - ComponentContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                            {
                                Containers[i].GetInventory(0).TransferItemTo(ComponentContainers[y].GetInventory(0), Item);
                                break;
                            }
                            else
                            {
                                Containers[i].GetInventory(0).TransferItemTo(ComponentContainers[y].GetInventory(0), Item, (ComponentContainers[y].GetInventory(0).MaxVolume - ComponentContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                            }
                        }
                    }




                    if (Item.Type.GetItemInfo().IsOre && !Containers.Equals(OreContainers))
                    {
                        for (int y = 0; y < OreContainers.Count; y++)
                        {
                            if ((OreContainers[y].GetInventory(0).MaxVolume - OreContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                            {
                                Containers[i].GetInventory(0).TransferItemTo(OreContainers[y].GetInventory(0), Item);
                                break;
                            }
                            else
                            {
                                Containers[i].GetInventory(0).TransferItemTo(OreContainers[y].GetInventory(0), Item, (OreContainers[y].GetInventory(0).MaxVolume - OreContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                            }
                        }
                    }
                    if (Item.Type.GetItemInfo().IsIngot && !Containers.Equals(IngotContainers))
                    {
                        for (int y = 0; y < IngotContainers.Count; y++)
                        {
                            if ((IngotContainers[y].GetInventory(0).MaxVolume - IngotContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                            {
                                Containers[i].GetInventory(0).TransferItemTo(IngotContainers[y].GetInventory(0), Item);
                                break;
                            }
                            else
                            {
                                Containers[i].GetInventory(0).TransferItemTo(IngotContainers[y].GetInventory(0), Item, (IngotContainers[y].GetInventory(0).MaxVolume - IngotContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                            }
                        }
                    }
                    if (Item.Type.GetItemInfo().IsTool && !Containers.Equals(ToolContainers))
                    {
                        for (int y = 0; y < ToolContainers.Count; y++)
                        {
                            if ((ToolContainers[y].GetInventory(0).MaxVolume - ToolContainers[y].GetInventory(0).CurrentVolume) > Item.Amount * Item.Type.GetItemInfo().Volume)
                            {
                                Containers[i].GetInventory(0).TransferItemTo(ToolContainers[y].GetInventory(0), Item);
                                break;
                            }
                            else
                            {
                                Containers[i].GetInventory(0).TransferItemTo(ToolContainers[y].GetInventory(0), Item, (ToolContainers[y].GetInventory(0).MaxVolume - ToolContainers[y].GetInventory(0).CurrentVolume) * (MyFixedPoint)(1 / Item.Type.GetItemInfo().Volume));
                            }
                        }
                    }
                }
            }
        }

        public void AirlockManager()
        {
            MyDetectedEntityInfo Detected = Sensor.LastDetectedEntity;

            if (!Detected.IsEmpty())
            {
                if ((InnerDoor.GetPosition() - Detected.Position).Length() < (OuterDoor.GetPosition() - Detected.Position).Length()) //If player is closer to the inner door
                {
                    AirlockCounter = 0;
                    OuterDoor.CloseDoor();
                    if (OuterDoor.Status == DoorStatus.Closed)
                    {
                        AirlockVent.Depressurize = false;
                        if (AirlockVent.Status == VentStatus.Pressurized)
                        {
                            InnerDoor.OpenDoor();
                        }
                    }
                }
                else
                {
                    InnerDoor.CloseDoor();
                    if (InnerDoor.Status == DoorStatus.Closed)
                    {
                        AirlockVent.Depressurize = true;
                        if (AirlockVent.Status == VentStatus.Depressurized || AirlockCounter > 30)
                        {
                            OuterDoor.OpenDoor();
                        }
                        else
                        {
                            AirlockCounter++;
                        }
                    }
                }
            }
        }

        public void ThrustManager(bool enable, float targetVel, float increment)//Attempts to maintain a constant velocity, can be negative, higher increment = faster response
        {
            float P, I;

            float Velocity = (float)Vector3D.TransformNormal(Cockpit.GetShipVelocities().LinearVelocity, MatrixD.Transpose(Cockpit.WorldMatrix)).Y;
            I = -(Velocity - OldVel);
            OldVel = Velocity;

            if (enable)
            {                P = (targetVel - Velocity) * 0.1f;
                if (Velocity < targetVel)
                {
                    D += increment;
                }
                else if (Velocity > targetVel)
                {
                    D -= increment;
                }
                if(D > 1) { D = 1; }
                if(D < 0) { D = 0; }


                for (int i = 0; i < VertHydros.Count; i++)
                {
                    VertHydros[i].Enabled = true;
                    VertHydros[i].ThrustOverridePercentage = (P + I * 3 + D);
                }
                if ((P + I * 3 + D) <= 0)
                {
                    for (int i = 0; i < VertHydros.Count; i++)
                    {
                        VertHydros[i].Enabled = true;
                        VertHydros[i].ThrustOverride = 1;
                    }
                }
                Cockpit.GetSurface(0).WriteText("P: " + P + "\nI: " + I + "\nD: " + D);
            }
            else
            {
                for (int i = 0; i < VertHydros.Count; i++)
                {
                    VertHydros[i].Enabled = false;
                }
                D = 0;
            }


        }
        public void MaintainVertical(bool enable)//Points the bottom of the craft towards planet center, does nothing in zero-G
        {
            Vector3D TargetDirection;
            if (enable && Cockpit.TryGetPlanetPosition(out TargetDirection))
            {

                Cockpit.TryGetPlanetPosition(out TargetDirection);
                TargetDirection = TargetDirection - Camera.GetPosition();//Using Camera for orientation since i don't know how to rotate cockpit worldmatrix...

                Vector3D bodyVector = Vector3D.TransformNormal(TargetDirection, MatrixD.Transpose(Camera.WorldMatrix));

                OldPitch = Pitch;
                OldRoll = Roll;

                Pitch = Math.Acos(bodyVector.Z / (Math.Sqrt(bodyVector.Z * bodyVector.Z + bodyVector.Y * bodyVector.Y))) * (180 / Math.PI);
                Roll = Math.Acos(bodyVector.Z / (Math.Sqrt(bodyVector.Z * bodyVector.Z + bodyVector.X * bodyVector.X))) * (180 / Math.PI);

                if (bodyVector.Y < 0) { Pitch = -Pitch; }
                if (bodyVector.X < 0) { Roll = -Roll; }

                DeltaPitch = Pitch - OldPitch;
                DeltaRoll = Roll - OldRoll;

                for (int i = 0; i < Gyros.Count; i++)
                {
                    Gyros[i].GyroOverride = true;
                    Gyros[i].Pitch = (float)Pitch / 10 + (float)DeltaPitch;
                    Gyros[i].Roll = -(float)Roll / 10 - (float)DeltaRoll;
                }
            }
            else
            {
                for (int i = 0; i < Gyros.Count; i++)
                {
                    Gyros[i].GyroOverride = false;
                }
            }

        }

        public void PlanetFall()
        {
            MaintainVertical(true);
            for (int i = 0; i < AllIons.Count; i++)
            {
                AllIons[i].Enabled = false;
            }
            for (int i = 0; i < AllHydros.Count; i++)
            {
                AllHydros[i].Enabled = true;
            }
            Cockpit.DampenersOverride = true;
            if (Altitude > 1500)
            {
                ThrustManager(false, 0, 1);
            }
            else
            {
                ThrustManager(true, -(float)Altitude / 12, 0.1f);
            }
            if (OnGround())
            {
                PlanetFallEnable = false;
                ThrustManager(false, 0, 1);
                MaintainVertical(false);
            }
        }

        public bool OnGround()
        {
            int gearOnGround = 0;
            bool IsOnGround = false;
            for (int i = 0; i < Gear.Count; i++)
            {
                if (Gear[i].DetailedInfo.Contains("Ready To Lock")) { gearOnGround++; }
            }

            if (gearOnGround >= 2) { IsOnGround = true; }
            return IsOnGround;
        }
