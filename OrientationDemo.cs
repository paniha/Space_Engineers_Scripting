        IMyMotorStator pitchRotor, rollRotor;
        IMyShipController controller;
        IMyLandingGear gear;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            controller = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyShipController;
            pitchRotor = GridTerminalSystem.GetBlockWithName("Pitch Rotor") as IMyMotorStator;
            rollRotor = GridTerminalSystem.GetBlockWithName("Roll Rotor") as IMyMotorStator;
            gear = GridTerminalSystem.GetBlockWithName("Landing Gear") as IMyLandingGear;

        }

        float pitch, roll;
        public void Main(string argument, UpdateType updateSource)
        {
            Vector3D bodyVector = Vector3D.TransformNormal(controller.GetNaturalGravity(), MatrixD.Transpose(gear.WorldMatrix));//Makes a vector that points from the gear towards center of gravity

                bodyVector = bodyVector / bodyVector.Length();//makes the vector 1 unit long, we we can't do Asin to a value above 1 (i think)

                roll = (float)(Math.Asin(bodyVector.X) * (180 / Math.PI));//Using ArcSine to get the angle from the vector component
                pitch = (float)(Math.Asin(bodyVector.Z) * (180 / Math.PI));
            
            if (bodyVector.Y > 0)//Conditional used to get angles greater/lower than +/- 90 degrees
            {
                if (roll > 0) { roll = 180 - roll; }
                else { roll = -180 - roll; }

                if (pitch > 0) { pitch = 180 - pitch; }
                else { pitch = -180 - pitch; }
            }
            //Setting the rotors' speeds depending on angle, directions can vary depending on setup, could also include integration and derivative to get faster and more reliable action.
            pitchRotor.TargetVelocityRPM = pitch;
            rollRotor.TargetVelocityRPM = roll;
        }
