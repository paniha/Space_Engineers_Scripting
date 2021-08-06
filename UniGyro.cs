        IMyGyro G1;
        IMyCockpit cockpit;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            
            cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;
            G1 = GridTerminalSystem.GetBlockWithName("G1") as IMyGyro;
            G1.GyroOverride = true;
        }

        Vector3 Vin;
        public void Main(string argument, UpdateType updateSource)
        {
            Vin.X = cockpit.RotationIndicator.X;
            Vin.Y = cockpit.RotationIndicator.Y;
            Vin.Z = cockpit.RollIndicator;

            SetGyro(Vin.X, Vin.Y, Vin.Z, G1, cockpit);
        }

        public void SetGyro(float Pitch, float Yaw, float Roll, IMyGyro Gyr, IMyShipController reference)
        {
            Vector3 Input = new Vector3(Pitch, Yaw, Roll);
            Vector3 output = Vector3.TransformNormal(Gyr.GetPosition() - reference.GetPosition(), Matrix.Transpose(reference.WorldMatrix)); //Convert Gyr position to local, relative to Cockpit
            output += Input; //Add the input to the local position
            output = Vector3.Transform(output, reference.WorldMatrix); //Convert position back to world position, from the cockpit local grid
            output = Vector3.TransformNormal(output - Gyr.GetPosition(), Matrix.Transpose(Gyr.WorldMatrix));//Convert world position to local relative to gyro.

            Gyr.Pitch = output.X;
            Gyr.Yaw = output.Y;
            Gyr.Roll = output.Z;
        }
