/*
The following list contains the names of components in the game, the first part before the semicolon (:) is for use in checking inventory, the second part in quotation marks ("") is for use in assembler queues.
*/

SteelPlate: "MyObjectBuilder_BlueprintDefinition/SteelPlate"
Construction: "MyObjectBuilder_BlueprintDefinition/ConstructionComponent"
PowerCell: "MyObjectBuilder_BlueprintDefinition/PowerCell"
Computer : "MyObjectBuilder_BlueprintDefinition/ComputerComponent" 
LargeTube : "MyObjectBuilder_BlueprintDefinition/LargeTube"
Motor : "MyObjectBuilder_BlueprintDefinition/MotorComponent"
Display : "MyObjectBuilder_BlueprintDefinition/Display"
MetalGrid : "MyObjectBuilder_BlueprintDefinition/MetalGrid"
InteriorPlate : "MyObjectBuilder_BlueprintDefinition/InteriorPlate"
SmallTube  : "MyObjectBuilder_BlueprintDefinition/SmallTube"
RadioCommunication : "MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent"
BulletproofGlass  : "MyObjectBuilder_BlueprintDefinition/BulletproofGlass"
Girder : "MyObjectBuilder_BlueprintDefinition/GirderComponent"
Explosives: "MyObjectBuilder_BlueprintDefinition/ExplosivesComponent"
Detector : "MyObjectBuilder_BlueprintDefinition/DetectorComponent"
Medical : "MyObjectBuilder_BlueprintDefinition/MedicalComponent"
GravityGenerator : "MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent"
Superconductor : "MyObjectBuilder_BlueprintDefinition/Superconductor"
Thrust : "MyObjectBuilder_BlueprintDefinition/ThrustComponent"
Reactor : "MyObjectBuilder_BlueprintDefinition/ReactorComponent"
SolarCell : "MyObjectBuilder_BlueprintDefinition/SolarCell"

//Assembler Example:
Assembler.AddQueueItem(MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ComputerComponent"), 120.0);
/*
This will add 120 Computers to the queue of the assembler called "Assembler"

For inventory items the structure is a bit different, it is as follows:
*/
new MyItemType("MyObjectBuilder_Component", "Computer")
/*
Where the last part, the part that says "Computer" in this case, is the only one that changes, so if i wanted to get the number of Thruster Components in an inventory, i would do the following:
*/
ThrusterCount =  Block.GetInventory(x).GetItemAmount(new MyItemType("MyObjectBuilder_Component", "Thrust"));
/*
Where ThrusterCount is the variable i'm storing the amount in, Block is the block whos inventory i'm accessing and x  is the number of the inventory i'm accessing, Cargo containers only have one inventory, so x=0 in that case, but assemblers and Refineries have 2 inventories, so x=0 or x=1 in those cases, depending on which inventory you're accessing.
*/