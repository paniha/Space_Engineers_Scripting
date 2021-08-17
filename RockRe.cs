        static double AssemEff = 3.0;
        static int BatsPerBank = 70;
        static string LCDName = "Reactor LCD";

        List<IMyShipGrinder> Grinders = new List<IMyShipGrinder>();
        List<IMyShipWelder> Welders = new List<IMyShipWelder>();
        List<IMyBatteryBlock> ABatts = new List<IMyBatteryBlock>();
        List<IMyBatteryBlock> BBatts = new List<IMyBatteryBlock>();
        List<IMyAssembler> Assemblers = new List<IMyAssembler>();
        List<IMyRefinery> Refineries = new List<IMyRefinery>();
        IMyMotorStator Rotor;
        IMyProjector Projector;
        IMyCargoContainer Output, Input, Trash;
        IMyTextPanel InfoLCD;

        MyItemType PowerCell = MyItemType.MakeComponent("PowerCell");
        MyItemType Gravel = MyItemType.MakeIngot("Stone");
        MyItemType Iron = MyItemType.MakeIngot("Iron");
        MyItemType Silicon = MyItemType.MakeIngot("Silicon");
        MyItemType Nickel = MyItemType.MakeIngot("Nickel");
        MyItemType Stone = MyItemType.MakeOre("Stone");
        MyItemType Scrap = MyItemType.MakeOre("Scrap");
        public Program()
        {
            if (Storage.Length > 0)
                stage = int.Parse(Storage);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            BatUpdate();
            GridTerminalSystem.GetBlocksOfType(Grinders);
            GridTerminalSystem.GetBlocksOfType(Welders);
            Rotor = GridTerminalSystem.GetBlockWithName("Reactor Rotor") as IMyMotorStator;
            Projector = GridTerminalSystem.GetBlockWithName("Reactor Projector") as IMyProjector;
            Output = GridTerminalSystem.GetBlockWithName("Reactor Central Container") as IMyCargoContainer;
            Input = GridTerminalSystem.GetBlockWithName("Reactor Input Container") as IMyCargoContainer;
            Trash = GridTerminalSystem.GetBlockWithName("Reactor Overflow Container") as IMyCargoContainer;
            InfoLCD = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextPanel;
            List<IMyAssembler> AllAssemblers = new List<IMyAssembler>();
            List<IMyRefinery> AllRefineries = new List<IMyRefinery>();
            GridTerminalSystem.GetBlocksOfType(AllRefineries);
            GridTerminalSystem.GetBlocksOfType(AllAssemblers);
            for (int i = 0; i < AllAssemblers.Count; i++)
                if (AllAssemblers[i].CustomName.Contains("Reactor"))
                    Assemblers.Add(AllAssemblers[i]);
            for (int i = 0; i < AllRefineries.Count; i++)
                if (AllRefineries[i].CustomName.Contains("Reactor"))
                    Refineries.Add(AllRefineries[i]);
        }

        public void Save()
        {
            Storage = stage.ToString();
        }

        int stage = 0;
        double CanBeTransferred, CanBeReceived, Remainder;
        static double SiAmount = (1.0 / AssemEff);
        static double NiAmount = (2.0 / AssemEff);
        static double FeAmount = (10.0 / AssemEff);
        static float RadToDeg = (float)(180 / Math.PI);
        public void Main(string argument, UpdateType updateSource)
        {
            GetStoredPower();
            BatUpdate();

            switch (stage)
            {
                case 0:
                    if (BatsReady(ABatts) == BatsPerBank && !(BatsReady(BBatts) == BatsPerBank && BatPerc(BBatts) > BatPerc(ABatts)))
                    {
                        stage = 1;
                        BatsEnable(false, BBatts);
                        BatsEnable(true, ABatts);
                    }

                    else if (BatsReady(BBatts) == BatsPerBank)
                    {
                        stage = 3;
                        BatsEnable(true, BBatts);
                        BatsEnable(false, ABatts);
                    }
                    else
                        NoBatError();
                    break;

                case 1:
                    BatsEnable(false, BBatts);
                    Rotor.TargetVelocityRPM = -2;
                    if (Math.Round(Rotor.Angle * RadToDeg) == 0)
                    {
                        Weld((BatsReady(BBatts) < BatsPerBank));
                    }
                    if (BatPerc(ABatts) < 2)
                    {
                        BatsEnable(true, BBatts);
                        stage = 2;
                    }
                    break;

                case 2:
                    if (Math.Round(Rotor.Angle * RadToDeg) == 0)
                    {
                        grind(true);
                        if (Projector.BuildableBlocksCount >= BatsPerBank)
                        {
                            grind(false);
                            stage = 3;
                        }
                    }
                    break;

                case 3:
                    BatsEnable(false, ABatts);
                    Rotor.TargetVelocityRPM = 2;
                    if (Math.Round(Rotor.Angle * RadToDeg) == 90)
                    {
                        Weld((BatsReady(ABatts) < 70));
                    }
                    if (BatPerc(BBatts) < 2)
                    {
                        BatsEnable(true, ABatts);
                        stage = 4;
                    }
                    break;

                case 4:
                    if (Math.Round(Rotor.Angle * RadToDeg) == 90)
                    {
                        grind(true);
                        if (Projector.BuildableBlocksCount >= BatsPerBank)
                        {
                            grind(false);
                            stage = 0;
                        }
                    }
                    break;
            }

            if (((double)Output.GetInventory().CurrentVolume + ((ABatts.Count + BBatts.Count) * 20 - (double)(Output.GetInventory().FindItem(PowerCell).HasValue ? Output.GetInventory().FindItem(PowerCell).Value.Amount : 0)) * (double)PowerCell.GetItemInfo().Volume) / (double)Output.GetInventory().MaxVolume > 90)
                for (int x = 0; x < Refineries.Count; x++)
                    Refineries[x].Enabled = false;
            if (((double)Output.GetInventory().CurrentVolume + ((ABatts.Count + BBatts.Count) * 20 - (double)(Output.GetInventory().FindItem(PowerCell).HasValue ? Output.GetInventory().FindItem(PowerCell).Value.Amount : 0)) * (double)PowerCell.GetItemInfo().Volume) / (double)Output.GetInventory().MaxVolume < 80)
                for (int x = 0; x < Refineries.Count; x++)
                    Refineries[x].Enabled = true;

            if ((Output.GetInventory().FindItem(PowerCell).HasValue ? Output.GetInventory().FindItem(PowerCell).Value.Amount : 0) > (ABatts.Count + BBatts.Count) * 20)
                for (int i = 0; i < Assemblers.Count; i++)
                    Assemblers[i].Enabled = false;
            else
                for (int i = 0; i < Assemblers.Count; i++)
                    Assemblers[i].Enabled = true;

            if (Input.GetInventory().FindItem(Stone).HasValue)
                if ((double)Input.GetInventory().FindItem(Stone).Value.Amount * (double)Stone.GetItemInfo().Volume > RefAvailInputVol())
                    for (int x = 0; x < Refineries.Count; x++)
                        TransferMax(Stone, Input.GetInventory(), Refineries[x].InputInventory);
                else
                    for (int x = 0; x < Refineries.Count; x++)
                        TransferSpecific(Stone, (double)Input.GetInventory().FindItem(Stone).Value.Amount / ((double)Refineries.Count - (double)x), Input.GetInventory(), Refineries[x].InputInventory);

            TransferMax(PowerCell, Input.GetInventory(), Output.GetInventory());
            TransferMax(Scrap, Output.GetInventory(), Trash.GetInventory());
            for (int i = 0; i < Assemblers.Count; i++)
                TransferMax(PowerCell, Assemblers[i].OutputInventory, Output.GetInventory());

            CanBeReceived = 0;
            CanBeTransferred = 0;
            if (RefHasNickel())
            {
                for (int x = 0; x < Refineries.Count; x++)
                    CanBeTransferred += (double)Refineries[x].OutputInventory.FindItem(Nickel).Value.Amount / (double)NiAmount;
                if (InvPercFull(Assemblers[0].GetInventory()) < 90)
                {
                    for (int i = 0; i < Assemblers.Count; i++)
                        CanBeReceived += (double)(Assemblers[i].InputInventory.MaxVolume - Assemblers[i].InputInventory.CurrentVolume) / (double)(NiAmount * Nickel.GetItemInfo().Volume + FeAmount * Iron.GetItemInfo().Volume + SiAmount * Silicon.GetItemInfo().Volume);
                    if (CanBeTransferred > CanBeReceived)
                    {
                        for (int i = 0; i < Assemblers.Count; i++)
                            for (int x = 0; x < Refineries.Count; x++)
                            {
                                TransferSpecific(Iron, FeAmount * CanBeReceived / ((double)Assemblers.Count * (double)Refineries.Count), Refineries[x].OutputInventory, Assemblers[i].InputInventory);
                                TransferSpecific(Nickel, NiAmount * CanBeReceived / ((double)Assemblers.Count * (double)Refineries.Count), Refineries[x].OutputInventory, Assemblers[i].InputInventory);
                                TransferSpecific(Silicon, SiAmount * CanBeReceived / ((double)Assemblers.Count * (double)Refineries.Count), Refineries[x].OutputInventory, Assemblers[i].InputInventory);
                            }
                        for (int x = 0; x < Refineries.Count; x++)
                        {
                            Remainder = (double)Refineries[x].OutputInventory.FindItem(Nickel).Value.Amount;
                            TransferSpecific(Iron, (double)Refineries[x].OutputInventory.FindItem(Iron).Value.Amount - FeAmount * Remainder, Refineries[x].OutputInventory, Trash.GetInventory());
                            TransferSpecific(Silicon, (double)Refineries[x].OutputInventory.FindItem(Silicon).Value.Amount - SiAmount * Remainder, Refineries[x].OutputInventory, Trash.GetInventory());
                            TransferMax(Gravel, Refineries[x].OutputInventory, Trash.GetInventory());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Assemblers.Count; i++)
                            for (int x = 0; x < Refineries.Count; x++)
                            {
                                TransferSpecific(Iron, FeAmount * CanBeTransferred / ((double)Assemblers.Count * (double)Refineries.Count), Refineries[x].OutputInventory, Assemblers[i].InputInventory);
                                TransferSpecific(Nickel, NiAmount * CanBeTransferred / ((double)Assemblers.Count * (double)Refineries.Count), Refineries[x].OutputInventory, Assemblers[i].InputInventory);
                                TransferSpecific(Silicon, SiAmount * CanBeTransferred / ((double)Assemblers.Count * (double)Refineries.Count), Refineries[x].OutputInventory, Assemblers[i].InputInventory);
                            }
                        for (int x = 0; x < Refineries.Count; x++)
                        {
                            TransferMax(Iron, Refineries[x].OutputInventory, Trash.GetInventory());
                            TransferMax(Silicon, Refineries[x].OutputInventory, Trash.GetInventory());
                            TransferMax(Gravel, Refineries[x].OutputInventory, Trash.GetInventory());
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < Refineries.Count; x++)
                    {
                        TransferSpecific(Iron, FeAmount * CanBeTransferred / (double)Refineries.Count, Refineries[x].OutputInventory, Output.GetInventory());
                        TransferSpecific(Nickel, NiAmount * CanBeTransferred / (double)Refineries.Count, Refineries[x].OutputInventory, Output.GetInventory());
                        TransferSpecific(Silicon, SiAmount * CanBeTransferred / (double)Refineries.Count, Refineries[x].OutputInventory, Output.GetInventory());
                        TransferMax(Iron, Refineries[x].OutputInventory, Trash.GetInventory());
                        TransferMax(Silicon, Refineries[x].OutputInventory, Trash.GetInventory());
                        TransferMax(Gravel, Refineries[x].OutputInventory, Trash.GetInventory());
                    }
                }
            }
            else if(Output.GetInventory().FindItem(Nickel).HasValue)
            {
                CanBeTransferred = (double)Output.GetInventory().FindItem(Nickel).Value.Amount / (double)NiAmount;
                if (InvPercFull(Assemblers[0].GetInventory()) < 90)
                {
                    for (int i = 0; i < Assemblers.Count; i++)
                        CanBeReceived += (double)(Assemblers[i].InputInventory.MaxVolume - Assemblers[i].InputInventory.CurrentVolume) / (double)(NiAmount * Nickel.GetItemInfo().Volume + FeAmount * Iron.GetItemInfo().Volume + SiAmount * Silicon.GetItemInfo().Volume);
                    if (CanBeTransferred > CanBeReceived)
                    {
                        for (int i = 0; i < Assemblers.Count; i++)
                        {
                            TransferSpecific(Iron, FeAmount * CanBeReceived / (double)Assemblers.Count, Output.GetInventory(), Assemblers[i].InputInventory);
                            TransferSpecific(Nickel, NiAmount * CanBeReceived / (double)Assemblers.Count, Output.GetInventory(), Assemblers[i].InputInventory);
                            TransferSpecific(Silicon, SiAmount * CanBeReceived / (double)Assemblers.Count, Output.GetInventory(), Assemblers[i].InputInventory);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Assemblers.Count; i++)
                        {
                            TransferSpecific(Iron, FeAmount * CanBeTransferred / (double)Assemblers.Count, Output.GetInventory(), Assemblers[i].InputInventory);
                            TransferSpecific(Nickel, NiAmount * CanBeTransferred / (double)Assemblers.Count, Output.GetInventory(), Assemblers[i].InputInventory);
                            TransferSpecific(Silicon, SiAmount * CanBeTransferred / (double)Assemblers.Count, Output.GetInventory(), Assemblers[i].InputInventory);
                        }
                    }
                }
            }
        }
        public void GetStoredPower()
        {
            double BatPower = 0, StonePower = 0, IngotPower = 0, CellPower = 0, Fe = 0, Si = 0, Ni = 0;

            for (int i = 0; i < ABatts.Count; i++)
                BatPower += ABatts[i].CurrentStoredPower;
            for (int i = 0; i < BBatts.Count; i++)
                BatPower += BBatts[i].CurrentStoredPower;

                StonePower += (Input.GetInventory().FindItem(Stone).HasValue) ? (double)Input.GetInventory().FindItem(Stone).Value.Amount : 0;
                StonePower += (Trash.GetInventory().FindItem(Stone).HasValue) ? (double)Trash.GetInventory().FindItem(Stone).Value.Amount : 0;
            for (int i = 0; i < Refineries.Count; i++)
            {
                StonePower += Refineries[i].OutputInventory.FindItem(Stone).HasValue ? (double)Refineries[i].OutputInventory.FindItem(Stone).Value.Amount : 0;
                Fe += Refineries[i].OutputInventory.FindItem(Iron).HasValue ? (double)Refineries[i].OutputInventory.FindItem(Iron).Value.Amount / FeAmount : 0;
                Si += Refineries[i].OutputInventory.FindItem(Silicon).HasValue ? (double)Refineries[i].OutputInventory.FindItem(Silicon).Value.Amount / SiAmount : 0;
                Ni += Refineries[i].OutputInventory.FindItem(Nickel).HasValue ? (double)Refineries[i].OutputInventory.FindItem(Nickel).Value.Amount / NiAmount : 0;
            }

            StonePower *= 0.000108f;

            for (int i = 0; i < Assemblers.Count; i++)
            {
                Fe += Assemblers[i].InputInventory.FindItem(Iron).HasValue ? (double)Assemblers[i].InputInventory.FindItem(Iron).Value.Amount / FeAmount : 0;
                Si += Assemblers[i].InputInventory.FindItem(Silicon).HasValue ? (double)Assemblers[i].InputInventory.FindItem(Silicon).Value.Amount / SiAmount : 0;
                Ni += Assemblers[i].InputInventory.FindItem(Nickel).HasValue ? (double)Assemblers[i].InputInventory.FindItem(Nickel).Value.Amount / NiAmount : 0;
                CellPower += Assemblers[i].InputInventory.FindItem(PowerCell).HasValue ? (double)Assemblers[i].InputInventory.FindItem(PowerCell).Value.Amount * 0.015 : 0;
            }
            Fe += Output.GetInventory().FindItem(Iron).HasValue ? (double)Output.GetInventory().FindItem(Iron).Value.Amount / FeAmount : 0;
            Si += Output.GetInventory().FindItem(Silicon).HasValue ? (double)Output.GetInventory().FindItem(Silicon).Value.Amount / SiAmount : 0;
            Ni += Output.GetInventory().FindItem(Nickel).HasValue ? (double)Output.GetInventory().FindItem(Nickel).Value.Amount / NiAmount : 0;
            CellPower += Output.GetInventory().FindItem(PowerCell).HasValue ? (double)Output.GetInventory().FindItem(PowerCell).Value.Amount * 0.015 : 0;

            IngotPower = Fe < Si ? Fe < Ni ? Fe : Ni : Si < Ni ? Si : Ni;

            IngotPower *= 0.015;
            
            if(InfoLCD == null)
            {
                Me.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
                Me.GetSurface(0).WriteText("Total Energy: " + Math.Round(BatPower + StonePower + IngotPower + CellPower,2) + " MWh" + "\nIn Batteries: " + Math.Round(BatPower,2) + " MWh" + "\nIn Power Cells: " + Math.Round(CellPower,2) + " MWh" + "\nIn Ingots: " + Math.Round(IngotPower,2) + " MWh" + "\nIn Stone: " + Math.Round(StonePower,2) + " MWh");
            }
            else
            {
                InfoLCD.ContentType = ContentType.TEXT_AND_IMAGE;
                InfoLCD.WriteText("Total Energy: " + Math.Round(BatPower + StonePower + IngotPower + CellPower, 2) + " MWh" + "\nIn Batteries: " + Math.Round(BatPower, 2) + " MWh" + "\nIn Power Cells: " + Math.Round(CellPower, 2) + " MWh" + "\nIn Ingots: " + Math.Round(IngotPower, 2) + " MWh" + "\nIn Stone: " + Math.Round(StonePower, 2) + " MWh");
            }

        }

        public void BatUpdate()
        {
            List<IMyBatteryBlock> AllBatts = new List<IMyBatteryBlock>();
            ABatts.Clear();
            BBatts.Clear();
            GridTerminalSystem.GetBlocksOfType(AllBatts);
            for (int i = 0; i < AllBatts.Count; i++)
                if (AllBatts[i].CustomName.Contains("BatteryA"))
                    ABatts.Add(AllBatts[i]);
                else if (AllBatts[i].CustomName.Contains("BatteryB"))
                    BBatts.Add(AllBatts[i]);
        }

        public void NoBatError()
        {
            Echo("ERROR NOT ENOUGH BATTERIES!");
        }
        public void Weld(bool weld)
        {
            for (int i = 0; i < Welders.Count; i++)
                Welders[i].Enabled = weld;
        }

        public void grind(bool grind)
        {
            for (int i = 0; i < Grinders.Count; i++)
                Grinders[i].Enabled = grind;
        }

        public void BatsEnable(bool enable, List<IMyBatteryBlock> batts)
        {
            for (int i = 0; i < batts.Count; i++)
                batts[i].Enabled = enable;
        }

        public int BatsReady(List<IMyBatteryBlock> batts)
        {
            int ret = 0;
            for (int i = 0; i < batts.Count; i++)
                if (batts[i].IsFunctional)
                    ret++;
            return ret;
        }

        public float BatPerc(List<IMyBatteryBlock> batts)
        {
            float perc = 1;
            for (int i = 0; i < batts.Count; i++)
                perc = ((batts[i].CurrentStoredPower / batts[i].MaxStoredPower) < perc) ? batts[i].CurrentStoredPower / batts[i].MaxStoredPower : perc;
            perc *= 100;
            return perc;
        }
        public double RefAvailInputVol()
        {
            double ret = 0;
            for (int x = 0; x < Refineries.Count; x++)
                ret += (double)Refineries[x].InputInventory.MaxVolume - (double)Refineries[x].InputInventory.CurrentVolume;


            return ret;
        }

        public bool RefHasNickel()
        {
            bool ret = true;
            for (int x = 0; x < Refineries.Count; x++)
                ret = Refineries[x].OutputInventory.FindItem(Nickel).HasValue ? ret : false;
            return ret;
        }
        public void TransferSpecific(MyItemType item, double amount, IMyInventory source, IMyInventory destination)
        {
            if (source.FindItem(item).HasValue)
                source.TransferItemTo(destination, source.FindItem(item).Value, (MyFixedPoint)amount);
        }
        public double InvPercFull(IMyInventory inv)
        {
            return (double)inv.CurrentVolume / (double)inv.MaxVolume;
        }

        public bool TransferMax(MyItemType item, IMyInventory source, IMyInventory destination)
        {
            //Input.GetInventory().TransferItemTo(Refinery.InputInventory, Input.GetInventory().FindItem(Stone).Value, (MyFixedPoint)(((double)(Refinery.InputInventory.MaxVolume - Refinery.InputInventory.CurrentVolume) / Stone.GetItemInfo().Volume > Stone.GetItemInfo().Volume * (double)Input.GetInventory().FindItem(Stone).Value.Amount) ? (double)(Refinery.InputInventory.MaxVolume - Refinery.InputInventory.CurrentVolume) / Stone.GetItemInfo().Volume : (double)Input.GetInventory().FindItem(Stone).Value.Amount));
            if (source.FindItem(item).HasValue)
                source.TransferItemTo(destination, source.FindItem(item).Value, (MyFixedPoint)(((double)(destination.MaxVolume - destination.CurrentVolume) / item.GetItemInfo().Volume > item.GetItemInfo().Volume * (double)source.FindItem(item).Value.Amount) ? (double)(destination.MaxVolume - destination.CurrentVolume) / item.GetItemInfo().Volume : (double)source.FindItem(item).Value.Amount));
            return (source.FindItem(item).HasValue);
        }
