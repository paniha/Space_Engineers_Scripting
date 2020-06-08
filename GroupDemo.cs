        IMyBlockGroup group;                                                //Object for holding the block group
        List<IMyTerminalBlock> tempList = new List<IMyTerminalBlock>();     //Object for holding the basic blocks obtained from the block group
        List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();   //The list of IMyMotorSupsension blocks we're actually going to be using in our program
        public Program()
        {
            group = GridTerminalSystem.GetBlockGroupWithName("Group Name"); //I get the group using its name
            group.GetBlocks(tempList);                                      //I take all the blocks in the group, and put them into a list of basic blocks with no type, since that's what a list contains by default

            for (int i = 0; i < tempList.Count; i++)                        //i go through all the blocks i got from the blockgroup, and put them into a list of IMyMotorSuspension blocks, 
                wheels.Add((IMyMotorSuspension)tempList[i]);                //while telling the program to treat the basic blocks as if they were IMyMotorSuspension


        }

        public void Main(string argument, UpdateType updateSource)
        {

            for (int i = 0; i < wheels.Count; i++)                          //I go through all the IMyMotorSuspension blocks and set the friction to 50%
                wheels[i].Friction = 0.5f;



        }
    }
