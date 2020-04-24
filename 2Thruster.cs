        IMyCockpit Cockpit;

        IMyMotorStator yawRotorLeft, yawRotorRight, pitchRotorLeft, pitchRotorRight;

        IMyThrust ThrusterLeft, ThrusterRight;

        Vector3D TargetDirection;
        Vector3D bodyVector, vectorTest;
        Vector3D[] leftArray = new Vector3D[10];
        Vector3D[] rightArray = new Vector3D[10];

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            Cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;

            ThrusterLeft = GridTerminalSystem.GetBlockWithName("Thruster Left") as IMyThrust;
            yawRotorLeft = GridTerminalSystem.GetBlockWithName("rotorYawLeft") as IMyMotorStator;
            pitchRotorLeft = GridTerminalSystem.GetBlockWithName("rotorPitchLeft") as IMyMotorStator;

            ThrusterRight = GridTerminalSystem.GetBlockWithName("Thruster Right") as IMyThrust;
            yawRotorRight = GridTerminalSystem.GetBlockWithName("rotorYawRight") as IMyMotorStator;
            pitchRotorRight = GridTerminalSystem.GetBlockWithName("rotorPitchRight") as IMyMotorStator;



        }

        double pitch, yaw;
        Vector3 MoveInd;
        float MoveMod = 1.0f;  // MODIFY THIS VALUE TO AFFECT CONTROL INPUT MODIFIER,  probably to 0.001 or less on console

        public void Main(string argument, UpdateType updateSource)
        {
            MoveInd = Cockpit.MoveIndicator * MoveMod;

            UpdateOrientation(ThrusterRight);
            yawRotorRight.TargetVelocityRPM = -(float)yaw;
            pitchRotorRight.TargetVelocityRPM = (float)pitch;
            ThrustCtrl(ThrusterRight, yawRotorRight);


            UpdateOrientation(ThrusterLeft);
            yawRotorLeft.TargetVelocityRPM = (float)yaw;
            pitchRotorLeft.TargetVelocityRPM = (float)pitch;
            ThrustCtrl(ThrusterLeft, yawRotorLeft);



            Cockpit.GetSurface(0).ContentType = ContentType.TEXT_AND_IMAGE;
            Cockpit.GetSurface(0).WriteText("Pitch: " + pitch + "\nYaw: " + yaw + "\n" + vectorTest);

        }
        public void ThrustCtrl(IMyThrust Thruster, IMyMotorStator Rotor)
        {// 
            if (pitch < 10 && yaw < 10 && pitch > -10 && yaw > -10 && Rotor.Angle*(180/Math.PI) > 40 && Rotor.Angle * (180 / Math.PI) < 320)//If pitch is within 20 degrees of target and thruster isn't pointing at craft
            {
                if (MoveInd.Length() == 0 && Cockpit.DampenersOverride)
                {
                    //Thruster.ThrustOverridePercentage = (float)Cockpit.GetShipSpeed();
                    Thruster.ThrustOverridePercentage = (float)Math.Sqrt((4f / 5f) * (Cockpit.GetShipVelocities().LinearVelocity.Length() / 100f));
                }
                else
                {
                    Thruster.ThrustOverridePercentage = 0.5f * Math.Abs(MoveInd.Length());
                }
            }
            else
            {
                Thruster.ThrustOverridePercentage = 0;
            }
        }
        public void UpdateOrientation(IMyThrust Thruster)
        {
            
            if (Cockpit.DampenersOverride)
            {
                TargetDirection = new Vector3D(0, 0, 0);
                if (Thruster.CustomName.Contains("Left"))
                {
                    for (int i = (leftArray.Length-1); i > 0; i--)
                    {
                        leftArray[i] = leftArray[i - 1];
                    }
                    leftArray[0] = -Cockpit.GetShipVelocities().LinearVelocity;

                    for (int i = 1; i < leftArray.Length; i++)
                    {
                        TargetDirection += leftArray[i];
                    }
                    TargetDirection = TargetDirection / leftArray.Length;
                }
                else
                {
                    for (int i = (rightArray.Length-1); i > 0; i--)
                    {
                        rightArray[i] = rightArray[i - 1];
                    }
                    rightArray[0] = -Cockpit.GetShipVelocities().LinearVelocity;

                    for (int i = 1; i < rightArray.Length; i++)
                    {
                        TargetDirection += rightArray[i];
                    }
                    TargetDirection = TargetDirection / rightArray.Length;
                }




                

                //TargetDirection = -Cockpit.GetShipVelocities().LinearVelocity;
                TargetDirection = TargetDirection + Vector3D.TransformNormal((Vector3D)MoveInd * (1 + Cockpit.GetShipSpeed() * 1.1), Cockpit.WorldMatrix);
            }
            else
            {
                TargetDirection = Vector3D.TransformNormal((Vector3D)MoveInd * 5, Cockpit.WorldMatrix);
                if(TargetDirection.Length() == 0)
                {
                    TargetDirection = Vector3D.TransformNormal(new Vector3D(0, 0, -1), Cockpit.WorldMatrix); //Face thrusters towards the rear when idle
                }
            }
            vectorTest = Vector3D.TransformNormal(TargetDirection, MatrixD.Transpose(Cockpit.WorldMatrix));
            bodyVector = Vector3D.TransformNormal(TargetDirection, MatrixD.Transpose(Thruster.WorldMatrix));

            if(TargetDirection.Length() > 0.3)
            {
                pitch = Math.Acos(bodyVector.Z / (Math.Sqrt(bodyVector.Z * bodyVector.Z + bodyVector.Y * bodyVector.Y))) * (180 / Math.PI);
                yaw = Math.Acos(bodyVector.Z / (Math.Sqrt(bodyVector.Z * bodyVector.Z + bodyVector.X * bodyVector.X))) * (180 / Math.PI);
            }
            else
            {
                pitch = 0;
                yaw = 0;
            }


            if (bodyVector.Y < 0) { pitch = -pitch; }
            if (bodyVector.X < 0) { yaw = -yaw; }

        }
