        //This script will get all blocks with a specific name and then turn them on or off depending on which parameter you run the PB with.


        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(); //Creating a list that can contain any block, with basic functionality

        public Program()
        {
            //This means that the code will run once after you recompile or load your game, then wait for you to run it again
            Runtime.UpdateFrequency = UpdateFrequency.Once; 

            GridTerminalSystem.
            GridTerminalSystem.GetBlocks(blocks); //Getting all blocks and putting them into the list of blocks.

            //This bit of code goes through the list of blocks, from end to beginning.
            //If it sees a block that isn't named "Some Name" it will remove that block from the list.
            for (int i = blocks.Count-1; i > 0; i--) 
            {
                if (!blocks[i].Name.Equals("Some Name"))//Alternatively, you could use " !blocks[i].Name.Contains("Some Name") " instead, then it'll take any block with a name that contains that string
                {
                    blocks.RemoveAt(i);
                }
            }
        }


        public void Main(string argument, UpdateType updateSource)
        {
            if (argument.Equals("off"))//If we run the block with the parameter "off" (without ") we loop through all the blocks left in the list and turn them off
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    blocks[i].ApplyAction("OnOff_Off");
                }
            }

            if (argument.Equals("on"))//If we run the block with the parameter "on" (without ") we loop through all the blocks left in the list and turn them on
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    blocks[i].ApplyAction("OnOff_On");
                }
            }
        }
