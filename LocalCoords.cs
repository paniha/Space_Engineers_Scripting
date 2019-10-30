        /*
		Functions for turning global coordinates into local ones, relative to a cockpit or remote control.
		
		Usage:
		
		Paste the two functions (or the one you need, they both do the same) into the bottom of your script, outside the main loop
		
		Vector3D LocalPos = LocalCoords(WorldPos,cockpit);
		
		or
		
		Vector3D LocalPos = LocalCoords(WorldPos,remote);
	
		*/
		
	public Vector3D LocalCoords(Vector3D worldPos,IMyCockpit cockpit)
        {
            return RoundVector(Vector3D.TransformNormal(worldPos - cockpit.GetPosition(), MatrixD.Transpose(cockpit.WorldMatrix)));
        }

        public Vector3D LocalCoords(Vector3D worldPos, IMyRemoteControl cockpit)
        {
            return RoundVector(Vector3D.TransformNormal(worldPos - cockpit.GetPosition(), MatrixD.Transpose(cockpit.WorldMatrix)));
        }
